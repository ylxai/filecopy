using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileCopyUtility.Services
{
    public class FastFolderScanService
    {
        private readonly string[] _photoExtensions = { 
            ".nef", ".jpg", ".jpeg", ".png", ".raw", ".cr2", ".arw", 
            ".dng", ".raf", ".orf", ".tiff", ".tif", ".bmp", ".gif" 
        };

        public event EventHandler<ScanProgressEventArgs>? ProgressUpdated;

        /// <summary>
        /// High-performance parallel folder scanning with real-time progress
        /// </summary>
        public async Task<ScanResult> ScanFolderAsync(
            string rootPath, 
            CancellationToken cancellationToken = default,
            ScanOptions? options = null)
        {
            options ??= new ScanOptions();
            var result = new ScanResult { StartTime = DateTime.Now };
            
            try
            {
                // Use parallel processing for better performance
                var files = new ConcurrentBag<string>();
                var photoFiles = new ConcurrentBag<string>();
                var errors = new ConcurrentBag<string>();
                var scanState = new ScanState();

                await ScanDirectoryParallel(
                    rootPath, 
                    files, 
                    photoFiles, 
                    errors,
                    scanState,
                    options,
                    cancellationToken);

                result.AllFiles = files.ToList();
                result.PhotoFiles = photoFiles.ToList();
                result.Errors = errors.ToList();
                result.TotalFiles = files.Count;
                result.PhotoCount = photoFiles.Count;
                result.FoldersScanned = scanState.ProcessedFolders;
                result.EndTime = DateTime.Now;
                result.Success = true;

                return result;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.EndTime = DateTime.Now;
                result.Errors = new List<string> { "Scan was cancelled by user" };
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.EndTime = DateTime.Now;
                result.Errors = new List<string> { $"Scan failed: {ex.Message}" };
                return result;
            }
        }

        private class ScanState
        {
            public int ProcessedFolders;
            public int ProcessedFiles;
        }

        /// <summary>
        /// Parallel directory scanning with optimized I/O
        /// </summary>
        private async Task ScanDirectoryParallel(
            string directoryPath,
            ConcurrentBag<string> allFiles,
            ConcurrentBag<string> photoFiles,
            ConcurrentBag<string> errors,
            ScanState scanState,
            ScanOptions options,
            CancellationToken cancellationToken)
        {
            try
            {
                var dirInfo = new DirectoryInfo(directoryPath);
                if (!dirInfo.Exists) return;

                // Parallel file processing in current directory
                var files = dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
                
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = options.MaxParallelism
                };

                await Task.Run(() =>
                {
                    Parallel.ForEach(files, parallelOptions, file =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            // Skip hidden/system files if requested
                            if (options.SkipHiddenFiles && 
                                (file.Attributes & FileAttributes.Hidden) != 0)
                                return;

                            // Skip files that are too small (likely not photos)
                            if (options.MinFileSizeBytes > 0 && 
                                file.Length < options.MinFileSizeBytes)
                                return;

                            // Skip files that are too large 
                            if (options.MaxFileSizeBytes > 0 && 
                                file.Length > options.MaxFileSizeBytes)
                                return;

                            var filePath = file.FullName;
                            allFiles.Add(filePath);

                            // Check if it's a photo file
                            var extension = file.Extension.ToLowerInvariant();
                            if (_photoExtensions.Contains(extension))
                            {
                                photoFiles.Add(filePath);
                            }

                            // Update progress every N files
                            var currentCount = Interlocked.Increment(ref scanState.ProcessedFiles);
                            if (currentCount % options.ProgressUpdateInterval == 0)
                            {
                                ReportProgress(currentCount, photoFiles.Count, scanState.ProcessedFolders);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Error processing file {file.FullName}: {ex.Message}");
                        }
                    });
                }, cancellationToken);

                Interlocked.Increment(ref scanState.ProcessedFolders);

                // Recursively scan subdirectories with controlled parallelism
                if (options.IncludeSubdirectories)
                {
                    var subdirectories = dirInfo.EnumerateDirectories();
                    var semaphore = new SemaphoreSlim(options.MaxConcurrentDirectories);
                    
                    var tasks = subdirectories.Select(async subdir =>
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            await ScanDirectoryParallel(
                                subdir.FullName,
                                allFiles,
                                photoFiles,
                                errors,
                                scanState,
                                options,
                                cancellationToken);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }
            catch (UnauthorizedAccessException)
            {
                errors.Add($"Access denied to directory: {directoryPath}");
            }
            catch (DirectoryNotFoundException)
            {
                errors.Add($"Directory not found: {directoryPath}");
            }
            catch (Exception ex)
            {
                errors.Add($"Error scanning directory {directoryPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Quick file count estimation for large folders
        /// </summary>
        public async Task<FolderStats> GetQuickFolderStatsAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            var stats = new FolderStats();
            
            try
            {
                await Task.Run(() =>
                {
                    var dirInfo = new DirectoryInfo(folderPath);
                    if (!dirInfo.Exists) return;

                    // Quick enumeration without full recursive scan
                    var files = dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
                    var subdirs = dirInfo.EnumerateDirectories();

                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        stats.TotalFiles++;
                        stats.TotalSize += file.Length;

                        var ext = file.Extension.ToLowerInvariant();
                        if (_photoExtensions.Contains(ext))
                        {
                            stats.PhotoFiles++;
                        }
                    }

                    stats.TotalDirectories = subdirs.Count();
                    stats.HasSubdirectories = stats.TotalDirectories > 0;

                }, cancellationToken);
            }
            catch (Exception ex)
            {
                stats.Error = ex.Message;
            }

            return stats;
        }

        private void ReportProgress(int totalFiles, int photoFiles, int foldersScanned)
        {
            ProgressUpdated?.Invoke(this, new ScanProgressEventArgs
            {
                TotalFiles = totalFiles,
                PhotoFiles = photoFiles,
                FoldersScanned = foldersScanned,
                Timestamp = DateTime.Now
            });
        }
    }

    public class ScanOptions
    {
        public bool IncludeSubdirectories { get; set; } = true;
        public bool SkipHiddenFiles { get; set; } = true;
        public long MinFileSizeBytes { get; set; } = 1024; // Skip files smaller than 1KB
        public long MaxFileSizeBytes { get; set; } = 0; // 0 = no limit
        public int MaxParallelism { get; set; } = Environment.ProcessorCount;
        public int MaxConcurrentDirectories { get; set; } = 4;
        public int ProgressUpdateInterval { get; set; } = 50; // Report progress every 50 files
    }

    public class ScanResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool Success { get; set; }
        public List<string> AllFiles { get; set; } = new();
        public List<string> PhotoFiles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalFiles { get; set; }
        public int PhotoCount { get; set; }
        public int FoldersScanned { get; set; }
    }

    public class FolderStats
    {
        public int TotalFiles { get; set; }
        public int PhotoFiles { get; set; }
        public int TotalDirectories { get; set; }
        public long TotalSize { get; set; }
        public bool HasSubdirectories { get; set; }
        public string? Error { get; set; }
    }

    public class ScanProgressEventArgs : EventArgs
    {
        public int TotalFiles { get; set; }
        public int PhotoFiles { get; set; }
        public int FoldersScanned { get; set; }
        public DateTime Timestamp { get; set; }
    }
}