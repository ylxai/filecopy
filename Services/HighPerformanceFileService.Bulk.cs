using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FileCopyUtility.Helpers;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for HighPerformanceFileService - Bulk copy operations
/// </summary>
public partial class HighPerformanceFileService
{
    /// <summary>
    /// Copy multiple files with optimized parallel processing
    /// </summary>
    public async Task<PerformanceCopyResult> CopyFilesAsync(
        List<FileItem> files,
        string destinationFolder,
        int maxParallelism = 8,
        CancellationToken cancellationToken = default)
    {
        var result = new PerformanceCopyResult();
        result.StartTime = DateTime.Now;
        result.TotalFiles = files.Count;
        result.TotalBytes = files.Sum(f => f.Size);

        // Create destination directory if it doesn't exist
        Directory.CreateDirectory(destinationFolder);

        // Use semaphore to control parallelism
        using var semaphore = new SemaphoreSlim(maxParallelism);
        var tasks = new List<Task<CopyOperationResult>>();

        foreach (var file in files)
        {
            // Wait for available slot
            await semaphore.WaitAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    var destPath = Path.Combine(destinationFolder, Path.GetFileName(file.Path));

                    // Choose copy method based on file size
                    var success = await CopyFileOptimizedAsync(
                        file.Path,
                        destPath,
                        cancellationToken);

                    var operationResult = new CopyOperationResult
                    {
                        Success = success,
                        File = file,
                        DestinationPath = destPath
                    };

                    if (success)
                    {
                        // Get actual file size after copy for accurate tracking
                        var fileInfo = new FileInfo(destPath);
                        operationResult.BytesTransferred = fileInfo.Length;
                    }

                    return operationResult;
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        // Wait for all operations to complete
        var operationResults = await Task.WhenAll(tasks);

        // Process results
        foreach (var operationResult in operationResults)
        {
            if (operationResult.Success)
            {
                result.SuccessfulFiles.Add(operationResult.File);
                result.TotalBytesTransferred += operationResult.BytesTransferred;
            }
            else
            {
                result.FailedFiles.Add(new FailedFileItem
                {
                    File = operationResult.File,
                    Error = "Copy operation failed"
                });
            }
        }

        result.EndTime = DateTime.Now;
        var durationSec = (result.EndTime.Value - result.StartTime).TotalSeconds;
        result.AverageSpeedMBps = durationSec > 0 ? (result.TotalBytesTransferred / (1024.0 * 1024.0)) / durationSec : 0;

        return result;
    }

    /// <summary>
    /// Determines optimal copy method based on file size
    /// </summary>
    private async Task<bool> CopyFileOptimizedAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(sourcePath);
        var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

        // Use different strategies based on file size
        if (fileSizeMB > 50) // Large files (>50MB)
        {
            return await CopyLargeFileOptimizedAsync(sourcePath, destPath, cancellationToken);
        }
        else // Small files (≤50MB)
        {
            return await CopySmallFileOptimizedAsync(sourcePath, destPath, cancellationToken);
        }
    }

}

/// <summary>
/// Result of individual copy operation
/// </summary>
internal class CopyOperationResult
{
    public bool Success { get; set; }
    public FileItem File { get; set; } = new();
    public string DestinationPath { get; set; } = string.Empty;
    public long BytesTransferred { get; set; }
}