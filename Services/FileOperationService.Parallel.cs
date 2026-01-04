using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for FileOperationService - Parallel functionality
/// </summary>
public partial class FileOperationService
{
    /// <summary>
    /// Copy files dengan multi-threading (v1.5+)
    /// </summary>
    public async Task<CopyResult> CopyFilesParallelAsync(
        List<FileItem> files,
        string destinationFolder,
        int maxParallelism = 4,
        CancellationToken cancellationToken = default,
        PauseTokenSource? pauseTokenSource = null,
        DuplicateHandling duplicateHandling = DuplicateHandling.Overwrite,
        bool autoRetry = false,
        int maxRetries = 3,
        HashAlgorithmType hashAlgorithm = HashAlgorithmType.None)
    {
        var result = new CopyResult();
        result.StartTime = DateTime.Now;

        var semaphore = new SemaphoreSlim(maxParallelism);
        var tasks = new List<Task>();
        var processedCount = 0;
        var lockObject = new object();
        var hashingService = new HashingService();

        foreach (var file in files)
        {
            await semaphore.WaitAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    // Check for pause
                    if (pauseTokenSource != null)
                    {
                        await pauseTokenSource.WaitWhilePausedAsync();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Cancelled = true;
                        return;
                    }

                    var destPath = Path.Combine(destinationFolder, file.Name);

                    // Handle duplicates
                    destPath = HandleDuplicateFile(destPath, duplicateHandling);

                    if (string.IsNullOrEmpty(destPath))
                    {
                        // Skip this file
                        lock (lockObject)
                        {
                            processedCount++;
                            OnStatusChanged($"⏭️ Skipped: {file.Name} (already exists)");
                        }
                        return;
                    }

                    OnStatusChanged($"Copying: {file.Name}");

                    // Retry logic
                    int retries = 0;
                    bool success = false;

                    while (!success && retries <= (autoRetry ? maxRetries : 0))
                    {
                        try
                        {
                            await CopyFileWithProgressAsync(
                                file.Path,
                                destPath,
                                (progress) =>
                                {
                                    lock (lockObject)
                                    {
                                        OnProgressChanged(new ProgressEventArgs
                                        {
                                            TotalFiles = files.Count,
                                            ProcessedFiles = processedCount,
                                            CurrentFileName = file.Name,
                                            CurrentFileProgress = progress
                                        });
                                    }
                                },
                                cancellationToken);

                            // Verify integrity if enabled
                            if (hashAlgorithm != HashAlgorithmType.None)
                            {
                                OnStatusChanged($"🔍 Verifying: {file.Name}");
                                var isValid = await hashingService.VerifyFileIntegrityAsync(
                                    file.Path,
                                    destPath,
                                    hashAlgorithm);

                                if (!isValid)
                                {
                                    throw new Exception("Hash verification failed");
                                }
                            }

                            success = true;
                        }
                        catch (Exception ex)
                        {
                            retries++;
                            if (retries > maxRetries || !autoRetry)
                            {
                                throw;
                            }
                            OnStatusChanged($"🔄 Retry {retries}/{maxRetries}: {file.Name} - {ex.Message}");
                            await Task.Delay(1000 * retries); // Exponential backoff
                        }
                    }

                    lock (lockObject)
                    {
                        result.SuccessfulFiles.Add(file);
                        processedCount++;
                        var verifyText = hashAlgorithm != HashAlgorithmType.None ? " (verified)" : "";
                        OnStatusChanged($"✅ {file.Name}{verifyText}");
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        result.FailedFiles.Add(new FailedFileItem
                        {
                            File = file,
                            Error = ex.Message
                        });
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

        await Task.WhenAll(tasks);
        result.EndTime = DateTime.Now;
        return result;
    }
}