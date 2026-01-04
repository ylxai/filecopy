using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileCopyUtility;

/// <summary>
/// Partial class for MainWindow - Loading indicators and UI enhancements
/// </summary>
public partial class MainWindow
{
    private void SetLoadingState(bool isLoading, string statusMessage = "")
    {
        _isLoading = isLoading;

        Dispatcher.Invoke(() =>
        {
            // Disable controls during loading
            TxtSourceFolder.IsEnabled = !isLoading;
            TxtFileList.IsEnabled = !isLoading;
            BtnSelectSource.IsEnabled = !isLoading;
            BtnValidate.IsEnabled = !isLoading && !_isLoading;
            BtnStartCopy.IsEnabled = !isLoading && _validFilesManager.Any(); // Only enable if not loading and files are valid

            // Update status if provided
            if (!string.IsNullOrEmpty(statusMessage))
            {
                TxtStatusLog.AppendText($"{statusMessage}\n");
                TxtStatusLog.ScrollToEnd();
            }

            // Change cursor to indicate loading
            Cursor = isLoading ? System.Windows.Input.Cursors.Wait : System.Windows.Input.Cursors.Arrow;
        });
    }
    
    private void ShowLoadingSpinner()
    {
        // In a real implementation, this would show a visual loading spinner
        // For now, we'll just update the status
        SetLoadingState(true, "⏳ Processing, please wait...");
    }
    
    private void HideLoadingSpinner()
    {
        SetLoadingState(false, "✅ Ready");
    }

    private void ShowErrorMessage(string title, string message)
    {
        Dispatcher.Invoke(() =>
        {
            var result = System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        });
    }

    private void ShowWarningMessage(string title, string message)
    {
        Dispatcher.Invoke(() =>
        {
            var result = System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        });
    }

    private void ShowInfoMessage(string title, string message)
    {
        Dispatcher.Invoke(() =>
        {
            var result = System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        });
    }
}