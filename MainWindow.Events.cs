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
/// Partial class for MainWindow - Event Handlers
/// </summary>
public partial class MainWindow
{
    private void FileService_ProgressChanged(object? sender, ProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            TxtTotalProgress.Text = $"Total Progress: {e.ProcessedFiles} / {e.TotalFiles} files";
            ProgressTotal.Value = e.TotalProgress;

            TxtCurrentFile.Text = $"Current File: {e.CurrentFileName}";
            // ProgressCurrent.Value = e.CurrentFileProgress;

            if (e.CopySpeedMBps > 0)
            {
                TxtDashboardSpeed.Text = $"{e.CopySpeedMBps:F2} MB/s";
            }

            // Update dashboard
            TxtDashboardTotal.Text = e.TotalFiles.ToString();
            TxtDashboardSuccess.Text = e.ProcessedFiles.ToString();
        });
    }

    private void FileService_StatusChanged(object? sender, string status)
    {
        Dispatcher.Invoke(() =>
        {
            TxtStatusLog.AppendText(status + Environment.NewLine);
            TxtStatusLog.ScrollToEnd();
        });
    }

    private void OnHighPerformanceProgress(object? sender, PerformanceProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update progress displays with enhanced performance metrics
            // Calculate file-based progress percentage to ensure it doesn't exceed 100%
            var fileProgressPercentage = e.TotalFiles > 0 ? Math.Min(100, ((e.ProcessedFiles + e.SkippedCount) * 100) / e.TotalFiles) : 0;
            TxtTotalProgress.Text = $"Progress: {e.ProcessedFiles} copied / {e.SkippedCount} skipped / {e.TotalFiles} total ({fileProgressPercentage}%)";
            ProgressTotal.Value = e.TotalProgress;

            TxtCurrentFile.Text = $"⚡ Ultra-copying: {e.CurrentFileName}";
            // ProgressCurrent.Value = e.CurrentFileProgress;

            // Enhanced speed display
            TxtDashboardSpeed.Text = e.SpeedDisplay;

            // Update dashboard with real-time performance
            TxtDashboardTotal.Text = e.TotalFiles.ToString();
            TxtDashboardSuccess.Text = e.ProcessedFiles.ToString();

            // Show ETA if available
            if (e.EstimatedTimeRemaining.TotalSeconds > 0)
            {
                TxtElapsedTime.Text = $"ETA: {e.ETADisplay}";
            }
        });
    }

    private void OnStatusChanged(string status)
    {
        Dispatcher.Invoke(() =>
        {
            TxtStatusLog.AppendText(status + Environment.NewLine);
            TxtStatusLog.ScrollToEnd();
        });
    }

    private void OnDashboardUpdated(object? sender, DashboardUpdateEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update dashboard with real-time stats
            // This would update live charts and metrics
            var stats = e.Stats;

            // Update any live dashboard elements here
            TxtDashboardSpeed.Text = $"{stats.CurrentSpeed:F1} MB/s";
        });
    }

    private void FastScanService_ProgressUpdated(object? sender, ScanProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update progress in the active scan window
            if (_currentScanWindow?.Tag is object tag)
            {
                var type = tag.GetType();
                var filesCountProp = type.GetProperty("FilesCount");
                var photosCountProp = type.GetProperty("PhotosCount");
                var foldersCountProp = type.GetProperty("FoldersCount");

                if (filesCountProp?.GetValue(tag) is TextBlock filesText)
                    filesText.Text = $"Files: {e.TotalFiles:N0}";

                if (photosCountProp?.GetValue(tag) is TextBlock photosText)
                    photosText.Text = $"Photos: {e.PhotoFiles:N0}";

                if (foldersCountProp?.GetValue(tag) is TextBlock foldersText)
                    foldersText.Text = $"Folders: {e.FoldersScanned:N0}";
            }
        });
    }

    private void UpdateElapsedTime()
    {
        if (_copyStopwatch.IsRunning)
        {
            Dispatcher.Invoke(() =>
            {
                var elapsed = _copyStopwatch.Elapsed;
                TxtElapsedTime.Text = $"Time: {elapsed:mm\\:ss}";
            });
        }
    }

    private void BtnSelectSource_Click(object sender, RoutedEventArgs e)
    {
        var selectedFolder = Services.FolderPickerService.ShowSourceFolderDialog(_sourceFolder);

        if (!string.IsNullOrEmpty(selectedFolder))
        {
            _sourceFolder = selectedFolder;
            TxtSourceFolder.Text = Services.FolderPickerService.GetFolderDisplayName(_sourceFolder);

            // Validate and show folder info
            var (isValid, message, fileCount) = Services.FolderPickerService.ValidateFolder(_sourceFolder);

            if (isValid)
            {
                var folderName = Path.GetFileName(_sourceFolder);
                _toastService.ShowSuccess(
                    "📁 Source Folder Set Successfully",
                    $"Folder: {folderName}\nLocation: {_sourceFolder}\n{message}",
                    6
                );
            }
            else
            {
                var folderName = Path.GetFileName(_sourceFolder);
                _toastService.ShowWarning(
                    "⚠️ Folder Warning",
                    $"Folder: {folderName}\n{message}\nYou can still proceed with caution",
                    8
                );
            }
        }
    }

    private void BtnPasteClipboard_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                var text = System.Windows.Clipboard.GetText();
                TxtFileList.Text = text;

                var files = text.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(s => s.Trim())
                               .Where(s => !string.IsNullOrWhiteSpace(s))
                               .ToList();
                _fileListManager.Clear();
                _fileListManager.AddFiles(files);

                BtnValidate.IsEnabled = _fileListManager.Any();

                _toastService.ShowSuccess(
                    "📋 Paste Successful",
                    $"Added: {_fileListManager.Count()} filenames from clipboard\nNext: Validate files to check availability",
                    5
                );
            }
            else
            {
                _toastService.ShowInfo(
                    "📋 Clipboard Empty",
                    "Copy filenames from Excel/notepad first\nFormat: one filename per line (no extensions needed)",
                    5
                );
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("📋 Paste Error", $"Failed to paste from clipboard: {ex.Message}", 6);
        }
    }

    private void TxtFileList_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtFileList.Text))
        {
            _fileListManager.Clear();
            BtnValidate.IsEnabled = false;
        }
        else
        {
            var files = TxtFileList.Text.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .ToList();
            _fileListManager.Clear();
            _fileListManager.AddFiles(files);
            BtnValidate.IsEnabled = _fileListManager.Any();
        }
    }

    private async void BtnScanFolder_Click(object sender, RoutedEventArgs e)
    {
        var selectedFolder = Services.FolderPickerService.ShowScanFolderDialog(_sourceFolder);

        if (!string.IsNullOrEmpty(selectedFolder))
        {
            _fileListManager.Clear();

            // Quick pre-scan for immediate feedback
            var quickStats = await _fastScanService.GetQuickFolderStatsAsync(selectedFolder);

            if (quickStats.TotalFiles == 0 && string.IsNullOrEmpty(quickStats.Error))
            {
                _toastService.ShowInfo("📁 Empty Folder", $"No files found in selected folder\nFolder: {Services.FolderPickerService.GetFolderDisplayName(selectedFolder)}", 5);
                return;
            }

            if (!string.IsNullOrEmpty(quickStats.Error))
            {
                _toastService.ShowError("⚠️ Access Error", $"Cannot access folder: {quickStats.Error}", 6);
                return;
            }

            // Show enhanced progress window with cancel support
            var progressWindow = CreateAdvancedProgressWindow(
                "🚀 Fast Scanning...",
                $"High-speed scan of: {Services.FolderPickerService.GetFolderDisplayName(selectedFolder)}",
                quickStats);
            _currentScanWindow = progressWindow;
            progressWindow.Show();

            // Setup cancellation
            _scanCancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Use optimized scanning service
                var scanOptions = new ScanOptions
                {
                    IncludeSubdirectories = true,
                    SkipHiddenFiles = true,
                    MinFileSizeBytes = 1024, // Skip files smaller than 1KB
                    MaxParallelism = Environment.ProcessorCount,
                    MaxConcurrentDirectories = 4,
                    ProgressUpdateInterval = 25 // Report every 25 files for smoother progress
                };

                var result = await _fastScanService.ScanFolderAsync(
                    selectedFolder,
                    _scanCancellationTokenSource.Token,
                    scanOptions);

                Dispatcher.Invoke(() =>
                {
                    progressWindow.Close();

                    if (result.Success)
                    {
                        _fileListManager.Clear();
                        _fileListManager.AddFiles(result.AllFiles);
                        TxtFileList.Text = string.Join(Environment.NewLine, result.AllFiles);
                        BtnValidate.IsEnabled = _fileListManager.Any();

                        // Enhanced scan completion notification
                        var duration = result.Duration.TotalSeconds;
                        var speed = duration > 0 ? (result.TotalFiles / duration) : 0;
                        var folderName = Services.FolderPickerService.GetFolderDisplayName(selectedFolder);

                        var toastMessage = $"Scanned: {result.TotalFiles:N0} files in {result.Duration:mm\\:ss}\nPhotos: {result.PhotoCount:N0} • Speed: {speed:F0} files/sec";

                        // Use only toast notification
                        if (result.PhotoCount > 0)
                        {
                            _toastService.ShowSuccess(
                                "🚀 Fast Scan Complete",
                                toastMessage,
                                6
                            );
                        }
                        else
                        {
                            _toastService.ShowInfo(
                                "📁 Scan Complete",
                                $"Scanned: {result.TotalFiles:N0} files\nNo photo files detected in: {folderName}",
                                5
                            );
                        }
                    }
                    else
                    {
                        var errorMessage = result.Errors.Any()
                            ? string.Join("\n", result.Errors.Take(3))
                            : "Unknown error occurred";

                        _toastService.ShowError("❌ Scan Failed", $"Scan failed or was cancelled\n{errorMessage}", 6);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Dispatcher.Invoke(() =>
                {
                    progressWindow.Close();
                    _toastService.ShowInfo("🛑 Scan Cancelled", "Folder scan cancelled by user", 4);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    progressWindow.Close();
                    _toastService.ShowError("❌ Scan Error", $"Unexpected error during scan: {ex.Message}", 6);
                });
            }
            finally
            {
                _scanCancellationTokenSource?.Dispose();
                _scanCancellationTokenSource = null;
                _currentScanWindow = null;
            }
        }
    }

    private void BtnImportFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            Title = "Select File List"
        };

        if (dialog.ShowDialog() == true)
        {
            var files = File.ReadAllLines(dialog.FileName).ToList();
            _fileListManager.Clear();
            _fileListManager.AddFiles(files);
            TxtFileList.Text = string.Join(Environment.NewLine, files);
            BtnValidate.IsEnabled = _fileListManager.Any();
        }
    }

    private void BtnImportCsv_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Select CSV File"
        };

        if (dialog.ShowDialog() == true)
        {
            var lines = File.ReadAllLines(dialog.FileName);
            var files = lines.Skip(1) // Skip header
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(',')[0].Trim('"'))
                .ToList();

            _fileListManager.Clear();
            _fileListManager.AddFiles(files);

            TxtFileList.Text = string.Join(Environment.NewLine, files);
            BtnValidate.IsEnabled = _fileListManager.Any();
        }
    }

    private void Window_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            string[] items = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

            // Check if any dropped item is a folder
            bool hasFolder = items.Any(item => Services.FolderPickerService.IsValidDroppedFolder(item));

            if (hasFolder)
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                // You could show visual feedback here
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
        }
        e.Handled = true;
    }

    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            string[] items = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

            // Check if user dropped a folder - prioritize folder over files
            var folders = items.Where(item => Services.FolderPickerService.IsValidDroppedFolder(item)).ToArray();

            if (folders.Length > 0)
            {
                // User dropped one or more folders
                var selectedFolder = folders.First(); // Use first folder

                // Ask user what they want to do with the dropped folder
                var result = System.Windows.MessageBox.Show(
                    $"📁 Dropped folder: {Services.FolderPickerService.GetFolderDisplayName(selectedFolder)}\n\n" +
                    "What would you like to do?\n\n" +
                    "Yes: Set as Source Folder\n" +
                    "No: Scan folder for file list",
                    "Folder Dropped",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Set as source folder
                    SetSourceFolder(selectedFolder);
                }
                else if (result == System.Windows.MessageBoxResult.No)
                {
                    // Scan folder
                    ScanDroppedFolder(selectedFolder);
                }
                // Cancel = do nothing
            }
            else
            {
                // Process as files (existing functionality)
                ProcessDroppedFiles(items);
            }
        }
    }

    private void DropZone_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        e.Handled = true;
    }

    private void DropZone_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            ProcessDroppedFiles(files);
        }
    }

    private async void BtnStartCopy_Click(object sender, RoutedEventArgs e)
    {
        // ExpanderProgress.Visibility = Visibility.Visible;
        // ExpanderProgress.IsExpanded = true;
        BtnStartCopy.IsEnabled = false;
        BtnPause.IsEnabled = true;
        BtnCancel.IsEnabled = true;

        TxtStatusLog.Clear();
        TxtDashboardSpeed.Text = "0 MB/s";

        _cancellationTokenSource = new CancellationTokenSource();
        _pauseTokenSource = new PauseTokenSource();
        _copyStopwatch.Restart();
        _uiUpdateTimer?.Start();

        await CopyFilesAsync();

        _uiUpdateTimer?.Stop();
        _copyStopwatch.Stop();
        BtnPause.IsEnabled = false;
        BtnCancel.IsEnabled = false;
        BtnStartCopy.IsEnabled = true;
    }

    private void BtnPause_Click(object sender, RoutedEventArgs e)
    {
        if (_pauseTokenSource != null)
        {
            _pauseTokenSource.IsPaused = !_pauseTokenSource.IsPaused;
            BtnPause.Content = _pauseTokenSource.IsPaused ? "▶️ RESUME" : "⏸️ PAUSE";

            if (_pauseTokenSource.IsPaused)
            {
                _copyStopwatch.Stop();
            }
            else
            {
                _copyStopwatch.Start();
            }
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _toastService.ShowInfo("⏹️ Copy Cancelled", "Operation stopped • Copied files remain in destination\nSource files are SAFE • You can restart anytime", 6);
    }

    private void BtnWebGallery_Click(object sender, RoutedEventArgs e)
    {
        var galleryWindow = new Windows.GalleryWindow();
        galleryWindow.Show();
    }

    private void BtnProfiles_Click(object sender, RoutedEventArgs e)
    {
        var profileWindow = new Windows.ProfileWindow();
        profileWindow.ShowDialog();
    }

    private void BtnScheduler_Click(object sender, RoutedEventArgs e)
    {
        _toastService.ShowInfo("⏰ Scheduler", "Scheduler feature available in full version\nComing soon with advanced scheduling options", 5);
    }

    private void TxtSearchFilter_GotFocus(object sender, RoutedEventArgs e)
    {
        if (TxtSearchFilter.Text == "🔍 Filter files...")
        {
            TxtSearchFilter.Text = "";
            TxtSearchFilter.Foreground = System.Windows.Media.Brushes.Black;
        }
    }

    private void TxtSearchFilter_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtSearchFilter.Text))
        {
            TxtSearchFilter.Text = "🔍 Filter files...";
            var grayBrush = (System.Windows.Media.SolidColorBrush)FindResource("GlassTextSecondaryBrush");
            TxtSearchFilter.Foreground = grayBrush;
        }
    }
}