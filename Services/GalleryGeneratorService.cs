using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileCopyUtility.Services
{
    public class GalleryGeneratorService
    {
        private readonly GoogleDriveService _driveService;

        public GalleryGeneratorService(GoogleDriveService driveService)
        {
            _driveService = driveService;
        }

        public async Task<string> GenerateAndUploadGalleryAsync(string sourcePath, IProgress<string> progress)
        {
            // 1. Authenticate
            if (!_driveService.IsAuthenticated)
            {
                progress.Report("Authenticating with Google Drive...");
                bool authSuccess = await _driveService.AuthenticateAsync();
                if (!authSuccess)
                {
                    throw new Exception("Authentication failed. Please check credentials.json.");
                }
            }

            // 2. Create "FileCopy Web Galleries" root folder if needed
            // 2. Create or Reuse "FileCopy Web Galleries" root folder
            string eventName = new DirectoryInfo(sourcePath).Name;
            
            progress.Report($"Checking for existing folder '{eventName}'...");
            string? existingId = await _driveService.FindFolderAsync(eventName);
            string eventFolderId;

            if (existingId != null)
            {
                progress.Report($"Found existing folder. Using ID: {existingId}");
                eventFolderId = existingId;
            }
            else
            {
                progress.Report($"Creating folder '{eventName}' in Drive...");
                eventFolderId = await _driveService.CreateFolderAsync(eventName);
                
                progress.Report("Setting folder permissions (Public)...");
                await _driveService.MakeFolderPublicAsync(eventFolderId);
            }

            // 3. Process and Upload Photos
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var files = Directory.GetFiles(sourcePath)
                                 .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                                 .ToArray();

            int totalFiles = files.Length;
            int processedCount = 0;
            progress.Report($"Found {totalFiles} photos. Starting parallel upload...");

            // Limit concurrency to avoid hitting API rate limits (e.g., 5 concurrent uploads)
            using (var semaphore = new System.Threading.SemaphoreSlim(5))
            {
                var tasks = files.Select(async file =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        // Upload original file directly with retry logic
                        await TryUploadWithRetryAsync(file, eventFolderId, fileName, null); // Pass null progress to avoid spamming main log
                        
                        int current = System.Threading.Interlocked.Increment(ref processedCount);
                        progress.Report($"[{current}/{totalFiles}] Uploaded: {fileName}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }

            return eventFolderId;
        }

        private async Task TryUploadWithRetryAsync(string filePath, string folderId, string fileName, IProgress<string>? progress)
        {
            int maxRetries = 3;
            int delayMs = 2000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _driveService.UploadFileAsync(filePath, folderId, fileName);
                    return; // Success
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        progress?.Report($"ERROR uploading {fileName}: {ex.Message}");
                        throw; // Rethrow if last attempt failed
                    }

                    // Only report retry if we have a progress reporter (main thread)
                    // For parallel uploads, we might skip this to reduce noise, or handle differently
                    if (progress != null)
                    {
                        progress.Report($"Warning: Upload failed for {fileName}. Retrying ({i + 1}/{maxRetries})...");
                    }
                    
                    await Task.Delay(delayMs * (i + 1)); // Exponential backoff-ish
                }
            }
        }
    }
}
