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
/// Partial class for MainWindow - UI Handlers and Helpers
/// </summary>
public partial class MainWindow
{
    private void SetupFolderValidation()
    {
        // Add real-time validation when user types in the source folder text box
        if (TxtSourceFolder != null)
        {
            TxtSourceFolder.TextChanged += (s, e) =>
            {
                var folderPath = TxtSourceFolder.Text;
                if (!string.IsNullOrEmpty(folderPath) && folderPath != _sourceFolder)
                {
                    ValidateSourceFolderAsync(folderPath);
                }
            };
        }
    }

    private async void ValidateSourceFolderAsync(string folderPath)
    {
        await Task.Delay(1000); // Debounce - wait 1 second before validating

        if (TxtSourceFolder?.Text != folderPath) return; // User has continued typing

        var (isValid, message, fileCount) = Services.FolderPickerService.ValidateFolder(folderPath);

        Dispatcher.Invoke(() =>
        {
            if (isValid && fileCount > 0)
            {
                // Show green checkmark or positive indicator
                UpdateFolderStatus("✅ Valid folder", System.Windows.Media.Colors.Green);
                _sourceFolder = folderPath;
            }
            else if (Directory.Exists(folderPath))
            {
                // Folder exists but empty or no photos
                UpdateFolderStatus("⚠️ " + message, System.Windows.Media.Colors.Orange);
            }
            else
            {
                // Invalid folder
                UpdateFolderStatus("❌ Invalid folder", System.Windows.Media.Colors.Red);
            }
        });
    }

    private void UpdateFolderStatus(string message, System.Windows.Media.Color color)
    {
        // This would update a status label if you have one in your XAML
        // For now, we'll update the tooltip
        if (TxtSourceFolder != null)
        {
            TxtSourceFolder.ToolTip = message;
            // You could also change the border color or background here
        }
    }

    private void ShowFolderQuickInfo(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;

        try
        {
            var (isValid, message, fileCount) = Services.FolderPickerService.ValidateFolder(folderPath);
            var displayName = Services.FolderPickerService.GetFolderDisplayName(folderPath);

            var info = $"📁 {displayName}\n{message}";

            // Show in status or as a quick popup
            TxtSourceFolder.ToolTip = info;
        }
        catch
        {
            // Ignore errors in quick info
        }
    }

    private void TxtSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Check if window is fully loaded and all components are initialized
        if (!IsLoaded || TxtSearchFilter == null || TxtFileList == null)
            return;

        // Safely get the filter text
        var filterText = TxtSearchFilter.Text;
        if (filterText == null || filterText == "🔍 Filter files...")
        {
            // Don't filter when showing placeholder text
            return;
        }

        var filter = filterText.ToLower();
        if (string.IsNullOrEmpty(filter))
        {
            var allFiles = _fileListManager.GetAllFiles();
            TxtFileList.Text = string.Join(Environment.NewLine, allFiles);
        }
        else
        {
            var allFiles = _fileListManager.GetAllFiles();
            var filtered = allFiles.Where(f => f.ToLower().Contains(filter)).ToList();
            TxtFileList.Text = string.Join(Environment.NewLine, filtered);
        }
    }

    private async void BtnValidate_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_sourceFolder))
        {
            _toastService.ShowWarning("⚠️ Source Folder Required", "Please select Source Folder first\nClick '📁 Browse' button to choose folder containing your photos", 6);
            return;
        }

        // Show loading state
        SetLoadingState(true, "🔍 Validating files, please wait...");

        var unique = _fileListManager.GetAllFiles().Distinct().ToList();
        _validFilesManager.Clear();
        var notFound = new List<string>();

        // All photo extensions to search for
        var rawExtensions = new[] { ".nef", ".raw", ".cr2", ".arw", ".dng", ".raf", ".orf" };
        var jpgExtensions = new[] { ".jpg", ".jpeg" };

        // Search for files in source folder
        var progressWindow = new Window
        {
            Title = "Searching Files...",
            Width = 450,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var stack = new StackPanel { Margin = new Thickness(20), VerticalAlignment = VerticalAlignment.Center };
        var text = new TextBlock { Text = "Searching files in source folder...", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };
        var text2 = new TextBlock { Text = "(Searching RAW and JPG files)", FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 10), Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4a, 0xde, 0x80)) };
        var bar = new System.Windows.Controls.ProgressBar { IsIndeterminate = true, Height = 10 };
        var count = new TextBlock { Text = "Found: 0 files", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0), FontWeight = FontWeights.Bold };

        stack.Children.Add(text);
        stack.Children.Add(text2);
        stack.Children.Add(bar);
        stack.Children.Add(count);
        progressWindow.Content = stack;
        progressWindow.Show();

        await Task.Run(() =>
        {
            long totalSize = 0;
            int jpgCount = 0;
            int rawCount = 0;

            foreach (var baseFilename in unique)
            {
                // Remove extension if provided
                var filenameWithoutExt = Path.GetFileNameWithoutExtension(baseFilename);
                bool foundAny = false;

                // Search for all RAW extensions
                foreach (var ext in rawExtensions)
                {
                    var searchPattern = filenameWithoutExt + ext;
                    var foundFiles = Directory.GetFiles(_sourceFolder, searchPattern, SearchOption.AllDirectories);

                    foreach (var filePath in foundFiles)
                    {
                        _validFilesManager.AddFile(filePath);
                        foundAny = true;
                        rawCount++;

                        try
                        {
                            var fileInfo = new FileInfo(filePath);
                            totalSize += fileInfo.Length;
                        }
                        catch { }
                    }
                }

                // Search for JPG files (now we COPY them too)
                foreach (var ext in jpgExtensions)
                {
                    var searchPattern = filenameWithoutExt + ext;
                    var foundFiles = Directory.GetFiles(_sourceFolder, searchPattern, SearchOption.AllDirectories);

                    foreach (var filePath in foundFiles)
                    {
                        _validFilesManager.AddFile(filePath);
                        foundAny = true;
                        jpgCount++;

                        try
                        {
                            var fileInfo = new FileInfo(filePath);
                            totalSize += fileInfo.Length;
                        }
                        catch { }
                    }
                }

                if (!foundAny)
                {
                    notFound.Add(baseFilename);
                }

                Dispatcher.Invoke(() => count.Text = $"Found: {_validFilesManager.Count()} files (RAW: {rawCount}, JPG: {jpgCount})");
            }

            var sizeInMB = totalSize / (1024.0 * 1024.0);
            var sizeInGB = totalSize / (1024.0 * 1024.0 * 1024.0);
            var sizeText = sizeInGB >= 1 ? $"{sizeInGB:F2} GB" : $"{sizeInMB:F2} MB";

            Dispatcher.Invoke(() =>
            {
                progressWindow.Close();

                var validFilesCount = _validFilesManager.Count();
                var resultMessage = $"Found: {validFilesCount} files ({sizeText})\nRAW: {rawCount} • JPG: {jpgCount}\nNot Found: {notFound.Count} files";

                if (validFilesCount > 0)
                {
                    _toastService.ShowSuccess(
                        "🔍 File Search Complete",
                        resultMessage,
                        8
                    );
                }
                else
                {
                    _toastService.ShowWarning(
                        "⚠️ No Files Found",
                        $"Searched: {unique.Count} files\nNot found: {notFound.Count} files\nCheck file names and source folder",
                        10
                    );
                }

                var result = $"📸 Search Results:\n\n" +
                           $"✅ Total found: {_validFilesManager.Count()} files ({sizeText})\n" +
                           $"   • RAW files: {rawCount}\n" +
                           $"   • JPG files: {jpgCount}\n" +
                           $"❌ Not Found: {notFound.Count} files\n" +
                           $"🔄 Duplicates removed: {_fileListManager.Count() - unique.Count}";

                if (notFound.Any())
                {
                    result += $"\n\nNot found (no RAW file):\n{string.Join("\n", notFound.Take(10))}";
                    if (notFound.Count > 10)
                        result += $"\n... and {notFound.Count - 10} more";
                }

                System.Windows.MessageBox.Show(result, "Search Result", System.Windows.MessageBoxButton.OK,
                    notFound.Any() ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);

                BtnStartCopy.IsEnabled = _validFilesManager.Any();

                // Hide loading state
                SetLoadingState(false, "✅ Validation complete");
            });
        });
    }

    private Window? _currentScanWindow;

    private Window CreateProgressWindow(string title, string message)
    {
        var progressWindow = new Window
        {
            Title = title,
            Width = 450,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };

        var stack = new StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center
        };

        var text = new TextBlock
        {
            Text = message,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            FontSize = 14
        };

        var progressBar = new System.Windows.Controls.ProgressBar
        {
            IsIndeterminate = true,
            Height = 12,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var countText = new TextBlock
        {
            Text = "Preparing...",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue)
        };

        stack.Children.Add(text);
        stack.Children.Add(progressBar);
        stack.Children.Add(countText);
        progressWindow.Content = stack;

        return progressWindow;
    }

    private Window CreateAdvancedProgressWindow(string title, string message, FolderStats? quickStats = null)
    {
        var progressWindow = new Window
        {
            Title = title,
            Width = 500,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };

        var mainStack = new StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = message,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            FontSize = 14,
            FontWeight = FontWeights.Medium
        };

        // Quick stats display
        if (quickStats != null)
        {
            var statsText = new TextBlock
            {
                Text = $"📄 Found {quickStats.TotalFiles} files ({quickStats.PhotoFiles} photos) in top level",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 12,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue)
            };
            mainStack.Children.Add(statsText);
        }

        var progressBar = new System.Windows.Controls.ProgressBar
        {
            IsIndeterminate = true,
            Height = 12,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var statusStack = new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var filesText = new TextBlock
        {
            Text = "Files: 0",
            Margin = new Thickness(0, 0, 20, 0),
            FontWeight = FontWeights.Bold
        };
        filesText.Name = "FilesCount";

        var photosText = new TextBlock
        {
            Text = "Photos: 0",
            Margin = new Thickness(0, 0, 20, 0),
            FontWeight = FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green)
        };
        photosText.Name = "PhotosCount";

        var foldersText = new TextBlock
        {
            Text = "Folders: 0",
            FontWeight = FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Purple)
        };
        foldersText.Name = "FoldersCount";

        statusStack.Children.Add(filesText);
        statusStack.Children.Add(photosText);
        statusStack.Children.Add(foldersText);

        // Cancel button
        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "❌ Cancel Scan",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Padding = new Thickness(15, 5, 15, 5),
            Margin = new Thickness(0, 10, 0, 0)
        };

        cancelButton.Click += (s, e) =>
        {
            _scanCancellationTokenSource?.Cancel();
            ((System.Windows.Controls.Button)s).IsEnabled = false;
            ((System.Windows.Controls.Button)s).Content = "Cancelling...";
        };

        mainStack.Children.Add(titleText);
        mainStack.Children.Add(progressBar);
        mainStack.Children.Add(statusStack);
        mainStack.Children.Add(cancelButton);
        progressWindow.Content = mainStack;

        // Store references for updates
        progressWindow.Tag = new { FilesCount = filesText, PhotosCount = photosText, FoldersCount = foldersText };

        return progressWindow;
    }
}