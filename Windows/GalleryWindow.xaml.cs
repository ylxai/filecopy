using System;
using System.Windows;
using Microsoft.Win32;
using FileCopyUtility.Services;
using System.Threading.Tasks;

namespace FileCopyUtility.Windows
{
    public partial class GalleryWindow : Window
    {
        private GalleryHistoryService _historyService;

        public GalleryWindow()
        {
            InitializeComponent();
            _historyService = new GalleryHistoryService();
            Loaded += GalleryWindow_Loaded;
        }

        private async void GalleryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            await _historyService.LoadHistoryAsync();
            ListHistory.ItemsSource = _historyService.GetHistory();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Select Source Folder with Photos";
            if (dialog.ShowDialog() == true)
            {
                TxtSourceFolder.Text = dialog.FolderName;
            }
        }

        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSourceFolder.Text))
            {
                System.Windows.MessageBox.Show("Please select a source folder.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            BtnPublish.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            TxtLog.Text = "Starting process...";

            try
            {
                // Instantiate services directly (Simple approach for this window)
                var driveService = new GoogleDriveService();
                var generator = new GalleryGeneratorService(driveService);

                var progress = new Progress<string>(msg => 
                {
                    // Update Log
                    TxtLog.Text += $"\n{msg}";
                    TxtLog.ScrollToEnd();

                    // Parse progress for UI updates (Expected format: "[5/50] Uploaded: ...")
                    if (msg.StartsWith("["))
                    {
                        try
                        {
                            int closeBracketIndex = msg.IndexOf(']');
                            if (closeBracketIndex > 1)
                            {
                                string counts = msg.Substring(1, closeBracketIndex - 1);
                                string[] parts = counts.Split('/');
                                if (parts.Length == 2 && int.TryParse(parts[0], out int current) && int.TryParse(parts[1], out int total))
                                {
                                    ProgressBar.Maximum = total;
                                    ProgressBar.Value = current;
                                    TxtProgressStatus.Text = $"Uploading {current} of {total} photos...";
                                }
                            }
                        }
                        catch { /* Ignore parsing errors */ }
                    }
                    else if (msg.StartsWith("Creating") || msg.StartsWith("Checking"))
                    {
                        TxtProgressStatus.Text = "Initializing...";
                        ProgressBar.IsIndeterminate = true;
                    }
                    else if (msg.StartsWith("Found existing") || msg.StartsWith("Setting folder"))
                    {
                        ProgressBar.IsIndeterminate = false;
                    }
                });

                // Capture values from UI thread before starting background task
                // Capture values from UI thread before starting background task
                string sourcePath = TxtSourceFolder.Text;

                // Run in background to keep UI responsive
                // The service uses Dispatcher for UI-thread image processing where needed
                string folderId = await Task.Run(() => generator.GenerateAndUploadGalleryAsync(
                    sourcePath, 
                    progress));

                // Save to History
                var historyItem = new GalleryHistoryItem
                {
                    FolderId = folderId,
                    FolderName = new System.IO.DirectoryInfo(sourcePath).Name,
                    CreatedAt = DateTime.Now,
                    SourcePath = sourcePath
                };
                await _historyService.AddItemAsync(historyItem);
                await LoadHistoryAsync(); // Refresh list

                TxtLog.Text += $"\n\nSUCCESS! Gallery created.";
                TxtLog.Text += $"\nGoogle Drive Folder ID: {folderId}";
                TxtLog.Text += $"\n(Copy this ID to your Web App)";
                
                // Show ID in the UI for manual copying
                TxtResultId.Text = folderId;
                TxtResultId.SelectAll();
                TxtResultId.Focus();

                // Copy ID to clipboard automatically for convenience
                System.Windows.Clipboard.SetText(folderId);
                
                System.Windows.MessageBox.Show($"Gallery published successfully!\nFolder ID: {folderId}\n(Copied to clipboard)", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtLog.Text += $"\nERROR: {ex.Message}";
                System.Windows.MessageBox.Show($"Error: {ex.Message}\n\nMake sure 'credentials.json' is in the app folder.", "Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                BtnPublish.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnRefreshHistory_Click(object sender, RoutedEventArgs e)
        {
            await LoadHistoryAsync();
        }

        private void BtnCopyHistoryId_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string folderId)
            {
                System.Windows.Clipboard.SetText(folderId);
                System.Windows.MessageBox.Show($"Folder ID copied to clipboard:\n{folderId}", "Copied", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string folderId)
            {
                var result = System.Windows.MessageBox.Show("Are you sure you want to delete this history item?\n(This will NOT delete the folder from Google Drive)", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    await _historyService.DeleteItemAsync(folderId);
                    await LoadHistoryAsync();
                }
            }
        }

        private void TxtWebLink_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://clients.hafiportrait.photography",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
