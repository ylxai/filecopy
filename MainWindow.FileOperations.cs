using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using FileCopyUtility.Services;
using FileCopyUtility.Models;
using FileCopyUtility.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Windows.Media.Animation;

namespace FileCopyUtility;

/// <summary>
/// Partial class for MainWindow - File Operations
/// </summary>
public partial class MainWindow
{
    private async Task CopyFilesAsync()
    {
        // Get thread count
        int threadCount = CmbThreadCount.SelectedIndex switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            3 => 8,
            _ => 4
        };

        // Output is always subfolders in source folder
        var rawOutputFolder = Path.Combine(_sourceFolder, "RAW");
        var jpgOutputFolder = Path.Combine(_sourceFolder, "JPG");

        // Create folders
        Directory.CreateDirectory(rawOutputFolder);
        Directory.CreateDirectory(jpgOutputFolder);

        // Convert to FileItem objects - copy to appropriate subfolder
        var rawItems = new List<FileItem>();
        var jpgItems = new List<FileItem>();

        foreach (var filePath in _validFilesManager.GetAllFiles())
        {
            try
            {
                // Sanitize and validate the file path
                var sanitizedPath = PathSecurityValidator.SanitizePath(filePath);

                // Only process if path is safe
                if (PathSecurityValidator.IsSafeFileName(Path.GetFileName(sanitizedPath)))
                {
                    var fileInfo = new FileInfo(sanitizedPath);
                    var extension = fileInfo.Extension.ToLower();

                    var item = new FileItem
                    {
                        Path = sanitizedPath,
                        Name = fileInfo.Name,
                        Size = fileInfo.Length
                    };

                    // Separate RAW and JPG
                    if (new[] { ".jpg", ".jpeg" }.Contains(extension))
                    {
                        jpgItems.Add(item);
                    }
                    else
                    {
                        rawItems.Add(item);
                    }
                }
            }
            catch { }
        }

        try
        {
            // 🚀 ULTRA-FAST COPY using high-performance service
            OnStatusChanged("🚀 Initializing Ultra-Fast Copy Engine...");

            // Initialize performance tracking
            _copyStartTime = DateTime.Now;
            _totalBytesToCopy = rawItems.Sum(f => f.Size) + jpgItems.Sum(f => f.Size);
            _totalBytesCopied = 0;

            // Start performance tracking
            StartPerformanceTracking();

            // Set optimal parallelism (auto-detected based on CPU cores)
            var optimalThreads = Math.Max(threadCount, Environment.ProcessorCount * 2);

            // Copy RAW files with ultra-fast engine
            PerformanceCopyResult rawResult;
            OnStatusChanged($"⚡ Ultra-copying {rawItems.Count} RAW files...");
            rawResult = await _highPerformanceService.CopyFilesUltraFastAsync(
                rawItems,
                rawOutputFolder,
                optimalThreads,
                _cancellationTokenSource!.Token);

            // Update total bytes copied after RAW files
            Interlocked.Add(ref _totalBytesCopied, rawResult.TotalBytesTransferred);

            // Check for cancellation before proceeding with JPG files
            if (rawResult.Cancelled)
            {
                throw new OperationCanceledException("Copy operation was cancelled after RAW files");
            }

            // Copy JPG files with ultra-fast engine
            PerformanceCopyResult jpgResult;
            OnStatusChanged($"⚡ Ultra-copying {jpgItems.Count} JPG files...");
            jpgResult = await _highPerformanceService.CopyFilesUltraFastAsync(
                jpgItems,
                jpgOutputFolder,
                optimalThreads,
                _cancellationTokenSource!.Token);

            // Combine ultra-fast results with performance metrics
            var result = new PerformanceCopyResult
            {
                StartTime = rawResult.StartTime,
                EndTime = jpgResult.EndTime ?? DateTime.Now,
                SuccessfulFiles = rawResult.SuccessfulFiles.Concat(jpgResult.SuccessfulFiles).ToList(),
                FailedFiles = rawResult.FailedFiles.Concat(jpgResult.FailedFiles).ToList(),
                SkippedFiles = rawResult.SkippedFiles.Concat(jpgResult.SkippedFiles).ToList(),
                VerifiedFiles = rawResult.VerifiedFiles.Concat(jpgResult.VerifiedFiles).ToList(),
                Cancelled = rawResult.Cancelled || jpgResult.Cancelled,
                TotalBytes = rawResult.TotalBytes + jpgResult.TotalBytes,
                TotalBytesTransferred = rawResult.TotalBytesTransferred + jpgResult.TotalBytesTransferred,
                AverageSpeedMBps = Math.Max(rawResult.AverageSpeedMBps, jpgResult.AverageSpeedMBps),
                PeakSpeedMBps = Math.Max(rawResult.PeakSpeedMBps, jpgResult.PeakSpeedMBps)
            };

            // Set the total bytes copied to the actual transferred amount
            _totalBytesCopied = result.TotalBytesTransferred;

            // Stop performance tracking
            StopPerformanceTracking();

            // Update final stats
            Dispatcher.Invoke(() =>
            {
                TxtDashboardTotal.Text = (result.SuccessfulFiles.Count + result.FailedFiles.Count).ToString();
                TxtDashboardSuccess.Text = result.SuccessfulFiles.Count.ToString();
                TxtDashboardFailed.Text = result.FailedFiles.Count.ToString();
            });

            // Show enhanced completion with ultra-fast performance metrics
            var message = result.Cancelled
                ? $"⏹️ Copy cancelled\n✅ Success: {result.SuccessfulFiles.Count}\n❌ Failed: {result.FailedFiles.Count}"
                : $"🚀 ULTRA-FAST COPY COMPLETED!\n\n" +
                  $"✅ Success: {result.SuccessfulFiles.Count} files\n" +
                  $"⏭️ Skipped: {result.SkippedFiles.Count} files (already exist)\n" +
                  $"🔍 Verified: {result.VerifiedFiles.Count} files\n" +
                  $"❌ Failed: {result.FailedFiles.Count}\n" +
                  $"⏱️ Duration: {result.Duration:mm\\:ss}\n" +
                  $"💾 Total Size: {FormatFileSize(result.TotalBytesTransferred)}\n" +
                  $"⚡ Avg Speed: {result.AverageSpeedMBps:F1} MB/s\n" +
                  $"🏆 Performance: {result.PerformanceGrade}\n" +
                  $"🔥 Speedup vs Explorer: {result.PerformanceRatio:F1}x faster!";

            message += "\n\n📂 Files organized:\n";
            message += $"   • RAW files → {rawOutputFolder}\n";
            message += $"   • JPG files → {jpgOutputFolder}";

            // Enhanced copy complete toast notification
            var duration = result.Duration;
            var speed = result.AverageSpeedMBps > 0 ? $"{result.AverageSpeedMBps:F1} MB/s" : "N/A";
            var successMessage = $"Files: {result.SuccessfulFiles.Count:N0} • Duration: {duration:mm\\:ss}\nSpeed: {speed} • Source files SAFE!";

            _toastService.ShowSuccess(
                "🎉 Copy Complete Successfully!",
                successMessage,
                8
            );

            // Ask for report generation via toast-like dialog
            var reportResult = System.Windows.MessageBox.Show(
                "📊 Generate detailed performance report?\n\nIncludes speed metrics, file analysis, and recommendations.",
                "Generate Report?",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (reportResult == System.Windows.MessageBoxResult.Yes)
            {
                await GenerateDetailedReportAsync(result, _sourceFolder);
            }

            // Save error log if needed
            if (result.FailedFiles.Any())
            {
                var logPath = Path.Combine(_sourceFolder, "copy_errors.log");
                await _fileService.SaveErrorLogAsync(logPath, result.FailedFiles);
            }
        }
        catch (Exception ex)
        {
            // Stop performance tracking in case of error
            StopPerformanceTracking();

            OnStatusChanged($"❌ Copy failed: {ex.Message}");
            _toastService.ShowError("❌ Copy Error", $"Copy operation failed: {ex.Message}", 8);
        }
    }

    private void ProcessDroppedFiles(string[] paths)
    {
        _fileListManager.Clear();

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                _fileListManager.AddFile(path);
            }
            else if (Directory.Exists(path))
            {
                ScanDirectory(path);
            }
        }

        var fileList = _fileListManager.GetAllFiles();
        TxtFileList.Text = string.Join(Environment.NewLine, fileList);
        BtnValidate.IsEnabled = _fileListManager.Any();
        _toastService.ShowSuccess("📁 Files Loaded", $"Added {_fileListManager.Count()} files from dropped items", 4);
    }

    private void ScanDirectory(string directory)
    {
        try
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                _fileListManager.AddFile(file);
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                ScanDirectory(subDir);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("📁 Scan Error", $"Error scanning directory: {ex.Message}", 6);
        }
    }

    private void ScanDirectoryWithProgress(string directory, int foundFiles, Window progressWindow)
    {
        try
        {
            // Scan files in current directory
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                _fileListManager.AddFile(file);
                // foundFiles++; // Remove ref increment since it's not ref parameter

                // Update progress every 10 files
                if (foundFiles % 10 == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (progressWindow.Content is StackPanel stack && stack.Children.Count > 2)
                        {
                            if (stack.Children[2] is TextBlock countText)
                                countText.Text = $"Found: {foundFiles} files...";
                        }
                    });
                }
            }

            // Recursively scan subdirectories
            var subDirs = Directory.GetDirectories(directory);
            foreach (var subDir in subDirs)
            {
                ScanDirectoryWithProgress(subDir, foundFiles, progressWindow);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                _toastService.ShowWarning("⚠️ Scan Warning", $"Some folders could not be scanned: {ex.Message}", 6);
            });
        }
    }

    private void SetSourceFolder(string folderPath)
    {
        _sourceFolder = folderPath;
        TxtSourceFolder.Text = Services.FolderPickerService.GetFolderDisplayName(_sourceFolder);

        // Validate and show folder info
        var (isValid, message, fileCount) = Services.FolderPickerService.ValidateFolder(_sourceFolder);

        if (isValid)
        {
            System.Windows.MessageBox.Show(
                $"✅ Source folder set successfully!\n\n" +
                $"📁 Folder: {_sourceFolder}\n" +
                $"📊 {message}",
                "Source Folder Set",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(
                $"⚠️ Warning: {message}\n\n" +
                $"📁 Folder: {_sourceFolder}\n\n" +
                "You can still proceed, but make sure this folder contains your photos.",
                "Folder Warning",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }

        // Update tooltip with folder summary
        ShowFolderQuickInfo(_sourceFolder);
    }

    private void ScanDroppedFolder(string folderPath)
    {
        _fileListManager.Clear();

        // Validate folder first
        var (isValid, message, fileCount) = Services.FolderPickerService.ValidateFolder(folderPath);

        if (!isValid && fileCount == 0)
        {
            System.Windows.MessageBox.Show(
                $"⚠️ {message}\n\nThe dropped folder appears to be empty or inaccessible.",
                "Folder Validation",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Show enhanced progress window
        var progressWindow = CreateProgressWindow("🔍 Scanning Dropped Folder...", $"Searching for files in: {Services.FolderPickerService.GetFolderDisplayName(folderPath)}");
        progressWindow.Show();

        // Scan in background with better progress reporting
        Task.Run(() =>
        {
            ScanDirectoryWithProgress(folderPath, 0, progressWindow);

            Dispatcher.Invoke(() =>
            {
                progressWindow.Close();
                var allFiles = _fileListManager.GetAllFiles();
                TxtFileList.Text = string.Join(Environment.NewLine, allFiles);
                BtnValidate.IsEnabled = _fileListManager.Any();

                var photoExtensions = new[] { ".nef", ".jpg", ".jpeg", ".png", ".raw", ".cr2", ".arw", ".dng", ".raf", ".orf" };
                var photoCount = allFiles.Count(f => photoExtensions.Contains(Path.GetExtension(f).ToLower()));

                System.Windows.MessageBox.Show(
                    $"✅ Scan completed successfully!\n\n" +
                    $"📁 Dropped folder: {Services.FolderPickerService.GetFolderDisplayName(folderPath)}\n" +
                    $"📄 Total files found: {_fileListManager.Count()}\n" +
                    $"📸 Photo files: {photoCount}\n" +
                    $"📂 Full path: {folderPath}",
                    "Scan Complete",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            });
        });
    }
}