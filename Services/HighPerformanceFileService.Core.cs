using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for HighPerformanceFileService - Core functionality and Win32 API declarations
/// </summary>
public partial class HighPerformanceFileService
{
    #region Win32 API Declarations

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CopyFileEx(
        string lpExistingFileName,
        string lpNewFileName,
        CopyProgressDelegate lpProgressRoutine,
        IntPtr lpData,
        ref bool pbCancel,
        CopyFileFlags dwCopyFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetFileValidData(SafeFileHandle hFile, long validDataLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushFileBuffers(SafeFileHandle hFile);

    [DllImport("kernel32.dll")]
    private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

    private delegate uint CopyProgressDelegate(
        long totalFileSize, long totalBytesTransferred, long streamSize, long streamBytesTransferred,
        uint dwStreamNumber, uint dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

    [Flags]
    private enum CopyFileFlags : uint
    {
        COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
        COPY_FILE_RESTARTABLE = 0x00000002,
        COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
        COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
        COPY_FILE_NO_BUFFERING = 0x00001000  // Direct I/O for speed
    }

    #endregion

    public event EventHandler<PerformanceProgressEventArgs>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    private readonly object _lockObject = new();
    private long _totalBytesTransferred = 0;
    private DateTime _lastSpeedCalculation = DateTime.MinValue;
    private long _lastBytesTransferred = 0;
    private double _peakSpeedMBps = 0; // Track peak speed

    /// <summary>
    /// Ultra-fast file copy using multiple optimization techniques
    /// </summary>
    public async Task<PerformanceCopyResult> CopyFilesUltraFastAsync(
        List<FileItem> files,
        string destinationFolder,
        int maxParallelism = 8, // Default optimal parallelism
        CancellationToken cancellationToken = default)
    {
        var result = new PerformanceCopyResult();
        result.StartTime = DateTime.Now;

        // Pre-allocate destination directory structure
        await Task.Run(() => EnsureDestinationStructure(destinationFolder));

        // Group files by size for optimal batching
        var fileGroups = GroupFilesByOptimalSize(files);

        // Use semaphore for controlled parallelism
        var semaphore = new SemaphoreSlim(maxParallelism);
        var tasks = new List<Task>();
        var processedCount = 0;

        foreach (var group in fileGroups)
        {
            foreach (var file in group)
            {
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        var destPath = Path.Combine(destinationFolder, file.Name);

                        OnStatusChanged($"⚡ Ultra-copying: {file.Name}");

                        // Smart Skip Logic
                        bool shouldSkip = false;
                        if (File.Exists(destPath))
                        {
                            try
                            {
                                var destInfo = new FileInfo(destPath);
                                if (destInfo.Length == file.Size)
                                {
                                    shouldSkip = true;
                                }
                            }
                            catch { }
                        }

                        if (shouldSkip)
                        {
                            lock (_lockObject)
                            {
                                result.SkippedFiles.Add(file);
                                processedCount++;
                                Interlocked.Add(ref _totalBytesTransferred, file.Size);
                                
                                // Update progress for skipped files
                                OnProgressChanged(new PerformanceProgressEventArgs
                                {
                                    TotalFiles = files.Count,
                                    ProcessedFiles = processedCount,
                                    CurrentFileName = file.Name,
                                    TotalBytes = result.TotalBytes,
                                    BytesTransferred = Interlocked.Read(ref _totalBytesTransferred),
                                    CopySpeedMBps = result.CurrentSpeedMBps, // Keep last known speed
                                    PeakSpeedMBps = _peakSpeedMBps,
                                    SkippedCount = result.SkippedFiles.Count,
                                    EstimatedTimeRemaining = TimeSpan.Zero
                                });

                                OnStatusChanged($"⏭️ Skipped: {file.Name}");
                            }
                            return;
                        }

                        // Choose optimal copy method based on file size
                        bool success = file.Size > 50_000_000 // 50MB+
                            ? await CopyLargeFileOptimizedAsync(file.Path, destPath, cancellationToken)
                            : await CopySmallFileOptimizedAsync(file.Path, destPath, cancellationToken);

                        if (success)
                        {
                            lock (_lockObject)
                            {
                                result.SuccessfulFiles.Add(file);
                                processedCount++;
                                Interlocked.Add(ref _totalBytesTransferred, file.Size);

                                // Calculate real-time speed
                                var now = DateTime.Now;
                                var timeDiff = (now - _lastSpeedCalculation).TotalSeconds;
                                if (timeDiff >= 1.0 || _lastSpeedCalculation == DateTime.MinValue) // Update every second or first time
                                {
                                    var currentBytesTransferred = Interlocked.Read(ref _totalBytesTransferred);
                                    var bytesDiff = currentBytesTransferred - _lastBytesTransferred;
                                    var speedMBps = timeDiff > 0 ? (bytesDiff / (1024.0 * 1024.0)) / timeDiff : 0;

                                    // Track peak speed
                                    if (speedMBps > _peakSpeedMBps)
                                    {
                                        _peakSpeedMBps = speedMBps;
                                    }

                                    OnProgressChanged(new PerformanceProgressEventArgs
                                    {
                                        TotalFiles = files.Count,
                                        ProcessedFiles = processedCount,
                                        CurrentFileName = file.Name,
                                        TotalBytes = result.TotalBytes,
                                        BytesTransferred = currentBytesTransferred,
                                        CopySpeedMBps = speedMBps,
                                        PeakSpeedMBps = _peakSpeedMBps,
                                        SkippedCount = result.SkippedFiles.Count,
                                        EstimatedTimeRemaining = CalculateETA(speedMBps, result.TotalBytes - currentBytesTransferred)
                                    });

                                    _lastSpeedCalculation = now;
                                    _lastBytesTransferred = currentBytesTransferred;
                                }

                                OnStatusChanged($"⚡ ✅ {file.Name} ({FormatFileSize(file.Size)}) - {FormatSpeed(result.CurrentSpeedMBps)}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (_lockObject)
                        {
                            result.FailedFiles.Add(new FailedFileItem { File = file, Error = ex.Message });
                            OnStatusChanged($"❌ {file.Name}: {ex.Message}");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }
        }

        // Calculate total size for progress tracking
        result.TotalBytes = files.Sum(f => f.Size);

        await Task.WhenAll(tasks);

        // Post-Copy Verification
        if (!cancellationToken.IsCancellationRequested)
        {
            OnStatusChanged("🔍 Verifying files...");
            int verifiedCount = 0;
            
            // Verify both successful and skipped files
            var filesToVerify = result.SuccessfulFiles.Concat(result.SkippedFiles).ToList();
            
            foreach (var file in filesToVerify)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var destPath = Path.Combine(destinationFolder, file.Name);
                bool isVerified = false;

                if (File.Exists(destPath))
                {
                    try
                    {
                        var destInfo = new FileInfo(destPath);
                        if (destInfo.Length == file.Size)
                        {
                            isVerified = true;
                        }
                    }
                    catch { }
                }

                if (isVerified)
                {
                    result.VerifiedFiles.Add(file);
                    verifiedCount++;
                }
                else
                {
                    // If verification failed for a "successful" file, move it to failed?
                    // For now, just log it.
                    OnStatusChanged($"⚠️ Verification failed: {file.Name}");
                }
            }
            OnStatusChanged($"✅ Verification complete: {verifiedCount}/{filesToVerify.Count} files verified");
        }
        result.EndTime = DateTime.Now;

        // Calculate final statistics
        var totalSeconds = (result.EndTime.Value - result.StartTime).TotalSeconds;
        var finalBytesTransferred = Interlocked.Read(ref _totalBytesTransferred);
        result.TotalBytesTransferred = finalBytesTransferred;
        result.AverageSpeedMBps = totalSeconds > 0 ? (result.TotalBytesTransferred / (1024.0 * 1024.0)) / totalSeconds : 0;
        result.PeakSpeedMBps = _peakSpeedMBps;

        OnStatusChanged($"🚀 ULTRA-FAST COPY COMPLETE! Average Speed: {FormatSpeed(result.AverageSpeedMBps)}, Peak: {FormatSpeed(result.PeakSpeedMBps)}");

        return result;
    }

    /// <summary>
    /// Calculate performance grade based on speed
    /// </summary>
    private string CalculatePerformanceGrade(double speedMBps)
    {
        if (speedMBps > 200) return "🚀 ULTRA FAST";
        if (speedMBps > 100) return "⚡ VERY FAST";
        if (speedMBps > 50) return "🔥 FAST";
        if (speedMBps > 20) return "✅ GOOD";
        if (speedMBps > 5) return "🟡 MODERATE";
        return "⚠️ SLOW";
    }

    /// <summary>
    /// Calculate performance ratio compared to baseline
    /// </summary>
    private double CalculatePerformanceRatio(double speedMBps)
    {
        const double baselineSpeed = 150.0; // Typical high-performance baseline for comparison
        return Math.Round(speedMBps / baselineSpeed, 3);
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