using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Memory-optimized file service for handling large files efficiently
/// Uses memory mapping and advanced buffer management
/// </summary>
public class MemoryOptimizedFileService
{
    private readonly PerformanceSettings _settings;
    
    public event EventHandler<PerformanceProgressEventArgs>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    public MemoryOptimizedFileService(PerformanceSettings? settings = null)
    {
        _settings = settings ?? PerformanceSettings.AutoConfigure();
    }

    /// <summary>
    /// Copy large files using memory mapping for maximum efficiency
    /// </summary>
    public async Task<bool> CopyLargeFileMemoryMappedAsync(
        string sourcePath, 
        string destPath, 
        Action<long, long>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceInfo = new FileInfo(sourcePath);
            var fileSize = sourceInfo.Length;
            
            OnStatusChanged($"💾 Memory-mapping: {Path.GetFileName(sourcePath)} ({FormatFileSize(fileSize)})");

            // For very large files (>1GB), use memory mapping
            if (fileSize > 1_073_741_824) // 1GB
            {
                return await CopyVeryLargeFileAsync(sourcePath, destPath, fileSize, progressCallback, cancellationToken);
            }
            else
            {
                return await CopyMediumFileOptimizedAsync(sourcePath, destPath, progressCallback, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            OnStatusChanged($"❌ Memory mapping failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Copy very large files (1GB+) using memory mapping
    /// </summary>
    private async Task<bool> CopyVeryLargeFileAsync(
        string sourcePath,
        string destPath,
        long fileSize,
        Action<long, long>? progressCallback,
        CancellationToken cancellationToken)
    {
        const long chunkSize = 268_435_456; // 256MB chunks
        long totalCopied = 0;

        try
        {
            // Pre-allocate destination file
            using (var destFileTemp = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                destFileTemp.SetLength(fileSize);
            }

            using var sourceFile = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destFile = new FileStream(destPath, FileMode.Open, FileAccess.Write, FileShare.None);

            for (long offset = 0; offset < fileSize; offset += chunkSize)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                var currentChunkSize = Math.Min(chunkSize, fileSize - offset);
                
                // Create memory mapped file for this chunk
                using var mmf = MemoryMappedFile.CreateFromFile(
                    sourceFile, 
                    null, 
                    currentChunkSize, 
                    MemoryMappedFileAccess.Read, 
                    HandleInheritability.None, 
                    false);

                using var accessor = mmf.CreateViewAccessor(offset, currentChunkSize, MemoryMappedFileAccess.Read);
                
                // Copy chunk to destination
                await CopyChunkAsync(accessor, destFile, offset, currentChunkSize, cancellationToken);
                
                totalCopied += currentChunkSize;
                progressCallback?.Invoke(totalCopied, fileSize);
            }

            // Flush to ensure data is written
            destFile.Flush(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Copy memory mapped chunk to destination
    /// </summary>
    private async Task CopyChunkAsync(
        MemoryMappedViewAccessor accessor,
        FileStream destStream,
        long offset,
        long chunkSize,
        CancellationToken cancellationToken)
    {
        const int bufferSize = 1_048_576; // 1MB buffer
        var buffer = new byte[bufferSize];
        
        destStream.Seek(offset, SeekOrigin.Begin);
        
        for (long pos = 0; pos < chunkSize; pos += bufferSize)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var bytesToRead = (int)Math.Min(bufferSize, chunkSize - pos);
            
            // Read from memory mapped file
            accessor.ReadArray(pos, buffer, 0, bytesToRead);
            
            // Write to destination
            await destStream.WriteAsync(buffer, 0, bytesToRead, cancellationToken);
        }
    }

    /// <summary>
    /// Optimized copy for medium files (under 1GB)
    /// </summary>
    private async Task<bool> CopyMediumFileOptimizedAsync(
        string sourcePath,
        string destPath,
        Action<long, long>? progressCallback,
        CancellationToken cancellationToken)
    {
        try
        {
            using var sourceStream = new FileStream(
                sourcePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                _settings.BufferSize,
                FileOptions.SequentialScan);

            using var destStream = new FileStream(
                destPath, 
                FileMode.Create, 
                FileAccess.Write, 
                FileShare.None, 
                _settings.BufferSize,
                _settings.FlushToDisk ? FileOptions.WriteThrough : FileOptions.None);

            if (_settings.PreAllocateFiles)
            {
                destStream.SetLength(sourceStream.Length);
            }

            var buffer = new byte[_settings.BufferSize];
            long totalBytes = sourceStream.Length;
            long copiedBytes = 0;
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                copiedBytes += bytesRead;

                progressCallback?.Invoke(copiedBytes, totalBytes);
            }

            if (_settings.FlushToDisk)
            {
                destStream.Flush(true);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Batch copy multiple small files efficiently
    /// Groups small files and copies them in optimized batches
    /// </summary>
    public async Task<List<FileItem>> BatchCopySmallFilesAsync(
        List<FileItem> smallFiles,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        var successfulFiles = new List<FileItem>();
        var semaphore = new SemaphoreSlim(_settings.MaxParallelism);
        var tasks = new List<Task>();
        var lockObject = new object();

        OnStatusChanged($"🚀 Batch copying {smallFiles.Count} small files...");

        foreach (var file in smallFiles)
        {
            await semaphore.WaitAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    var destPath = Path.Combine(destinationFolder, file.Name);
                    
                    // Use optimized copy for small files
                    var success = await CopySmallFileOptimizedAsync(file.Path, destPath, cancellationToken);
                    
                    if (success)
                    {
                        lock (lockObject)
                        {
                            successfulFiles.Add(file);
                        }
                        OnStatusChanged($"✅ {file.Name}");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        return successfulFiles;
    }

    /// <summary>
    /// Optimized copy for small files (under 10MB)
    /// Uses single-shot copy with optimal buffer
    /// </summary>
    private async Task<bool> CopySmallFileOptimizedAsync(
        string sourcePath,
        string destPath,
        CancellationToken cancellationToken)
    {
        try
        {
            // For very small files, read everything into memory at once
            var sourceInfo = new FileInfo(sourcePath);
            if (sourceInfo.Length < 10_485_760) // 10MB
            {
                var data = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
                await File.WriteAllBytesAsync(destPath, data, cancellationToken);
                return true;
            }
            else
            {
                // Use standard optimized copy for larger small files
                return await CopyMediumFileOptimizedAsync(sourcePath, destPath, null, cancellationToken);
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Clean up memory and force garbage collection
    /// </summary>
    public void OptimizeMemoryUsage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        OnStatusChanged("🧹 Memory optimized");
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int index = 0;
        double size = bytes;

        while (size >= 1024 && index < suffixes.Length - 1)
        {
            size /= 1024;
            index++;
        }

        return $"{size:F1} {suffixes[index]}";
    }

    protected virtual void OnProgressChanged(PerformanceProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }

    protected virtual void OnStatusChanged(string status)
    {
        StatusChanged?.Invoke(this, status);
    }
}