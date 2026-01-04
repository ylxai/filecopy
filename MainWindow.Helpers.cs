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
/// Partial class for MainWindow - Helper Functions
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Format file size for display
    /// </summary>
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

    /// <summary>
    /// Generate detailed performance report with multiple export formats
    /// </summary>
    private async Task GenerateDetailedReportAsync(PerformanceCopyResult result, string sourceFolder)
    {
        try
        {
            OnStatusChanged("📊 Generating detailed performance report...");

            // Generate comprehensive report
            var report = await _reportingService.GenerateDetailedReportAsync(result, sourceFolder);

            // Show export options dialog
            var exportDialog = CreateExportOptionsDialog(report, sourceFolder);
            exportDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            OnStatusChanged($"❌ Error generating report: {ex.Message}");
            _toastService.ShowError("📊 Report Error", $"Failed to generate report: {ex.Message}", 8);
        }
    }

    /// <summary>
    /// Create export options dialog for reports
    /// </summary>
    private Window CreateExportOptionsDialog(CopyReport report, string defaultPath)
    {
        var dialog = new Window
        {
            Title = "📊 Export Performance Report",
            Width = 500,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var mainPanel = new StackPanel { Margin = new Thickness(20) };

        // Header
        var header = new TextBlock
        {
            Text = "📊 Export Performance Report",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xb8, 0x54, 0x93)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        // Report summary
        var summary = new TextBlock
        {
            Text = $"📈 Performance Grade: {report.PerformanceGrade}\n" +
                   $"⚡ Average Speed: {report.AverageSpeedMBps:F1} MB/s\n" +
                   $"📁 Files Processed: {report.TotalFilesProcessed:N0}\n" +
                   $"💾 Data Transferred: {FormatFileSize(report.TotalBytesTransferred)}\n" +
                   $"⏱️ Duration: {report.OperationDuration:mm\\:ss}",
            FontSize = 12,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 0xb8, 0x54, 0x93)),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 15)
        };

        // Export format options
        var formatLabel = new TextBlock
        {
            Text = "📄 Select Export Format:",
            FontWeight = FontWeights.Medium,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var formatPanel = new StackPanel();

        var htmlOption = new System.Windows.Controls.RadioButton
        {
            Content = "🌐 HTML Report (Interactive with charts)",
            IsChecked = true,
            Margin = new Thickness(0, 5, 0, 5),
            Tag = ExportFormat.HTML
        };

        var jsonOption = new System.Windows.Controls.RadioButton
        {
            Content = "📋 JSON Data (Machine readable)",
            Margin = new Thickness(0, 5, 0, 5),
            Tag = ExportFormat.JSON
        };

        var csvOption = new System.Windows.Controls.RadioButton
        {
            Content = "📊 CSV Data (Spreadsheet compatible)",
            Margin = new Thickness(0, 5, 0, 5),
            Tag = ExportFormat.CSV
        };

        var txtOption = new System.Windows.Controls.RadioButton
        {
            Content = "📝 Text Report (Human readable)",
            Margin = new Thickness(0, 5, 0, 5),
            Tag = ExportFormat.TXT
        };

        formatPanel.Children.Add(htmlOption);
        formatPanel.Children.Add(jsonOption);
        formatPanel.Children.Add(csvOption);
        formatPanel.Children.Add(txtOption);

        // Buttons
        var buttonPanel = new DockPanel { Margin = new Thickness(0, 15, 0, 0) };

        var exportBtn = new System.Windows.Controls.Button
        {
            Content = "📊 Export Report",
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(15, 8, 15, 8),
            FontWeight = FontWeights.Medium
        };

        var cancelBtn = new System.Windows.Controls.Button
        {
            Content = "❌ Cancel",
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x72, 0x80)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(10, 0, 0, 0)
        };

        DockPanel.SetDock(cancelBtn, Dock.Right);
        buttonPanel.Children.Add(exportBtn);
        buttonPanel.Children.Add(cancelBtn);

        // Event handlers
        exportBtn.Click += async (s, e) =>
        {
            try
            {
                // Get selected format
                var selectedFormat = ExportFormat.HTML;
                foreach (var child in formatPanel.Children.OfType<System.Windows.Controls.RadioButton>())
                {
                    if (child.IsChecked == true)
                    {
                        selectedFormat = (ExportFormat)child.Tag;
                        break;
                    }
                }

                exportBtn.IsEnabled = false;
                exportBtn.Content = "🔄 Generating...";

                // Export report (use default path)
                var exportedPath = await _reportingService.ExportReportAsync(report, selectedFormat, defaultPath);

                dialog.Close();

                // Show success and ask to open
                var openResult = System.Windows.MessageBox.Show(
                    $"📊 Report exported successfully!\n\n📁 Location: {exportedPath}\n\nOpen report now?",
                    "Export Complete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);

                if (openResult == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exportedPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                exportBtn.IsEnabled = true;
                exportBtn.Content = "📊 Export Report";
                _toastService.ShowError("📊 Export Error", $"Failed to export report: {ex.Message}", 6);
            }
        };

        cancelBtn.Click += (s, e) => dialog.Close();

        // Assemble dialog
        mainPanel.Children.Add(header);
        mainPanel.Children.Add(summary);
        mainPanel.Children.Add(formatLabel);
        mainPanel.Children.Add(formatPanel);
        mainPanel.Children.Add(buttonPanel);

        dialog.Content = mainPanel;
        return dialog;
    }

    private void SetupAnimations()
    {
        // Apply hover effects to buttons and cards
        Loaded += async (s, e) =>
        {
            // Animate dashboard cards with staggered effect
            var cards = new[]
            {
                FindName("TxtDashboardTotal"),
                FindName("TxtDashboardSuccess"),
                FindName("TxtDashboardFailed"),
                FindName("TxtDashboardSpeed")
            }.OfType<UIElement>().ToList();

            if (cards.Any())
            {
                await AnimationHelper.StaggeredAnimationAsync(cards, AnimationType.FadeIn, 150);
            }
        };
    }

    private void AnimateCounterUpdate(TextBlock textBlock, int newValue)
    {
        if (textBlock == null) return;

        // Parse current value
        var currentText = textBlock.Text;
        var currentValue = 0;

        if (currentText.Contains('/'))
        {
            // Handle formats like "247 / 1,247"
            var parts = currentText.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[0].Trim().Replace(",", ""), out var parsed))
            {
                currentValue = parsed;
            }
        }
        else
        {
            // Handle simple numbers
            if (int.TryParse(currentText.Replace(",", "").Split(' ')[0], out var parsed))
            {
                currentValue = parsed;
            }
        }

        // Animate from current to new value
        var duration = TimeSpan.FromMilliseconds(500);
        var storyboard = new Storyboard();

        var animation = new Int32Animation
        {
            From = currentValue,
            To = newValue,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        animation.CurrentTimeInvalidated += (s, e) =>
        {
            if (animation.GetCurrentValue(currentValue, newValue, animation.CreateClock()) is int animatedValue)
            {
                textBlock.Text = animatedValue.ToString("N0");
            }
        };

        Storyboard.SetTarget(animation, textBlock);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Tag")); // Dummy property

        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private void ShowProgressAnimation()
    {
        // Create animated progress indicator
        var progressWindow = CreateAdvancedProgressWindow(
            "🚀 Processing...",
            "Performing file operations with smooth animations");

        progressWindow.Show();

        // Simulate progress with smooth animation
        Task.Run(async () =>
        {
            for (int i = 0; i <= 100; i += 5)
            {
                await Task.Delay(100);
                // Update progress with animation
            }

            Dispatcher.Invoke(() => progressWindow.Close());
        });
    }

    private void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        var mainGrid = this.Content as Grid;
        if (mainGrid != null)
        {
            _ = AnimationHelper.ShowNotificationAsync(mainGrid, message, type);
        }
    }

    private void UpdateDashboardWithAnimation(int total, int success, int failed, string speed)
    {
        // Update counters with smooth animations
        AnimateCounterUpdate(TxtDashboardTotal, total);
        AnimateCounterUpdate(TxtDashboardSuccess, success);
        AnimateCounterUpdate(TxtDashboardFailed, failed);

        // Update speed with fade effect
        if (TxtDashboardSpeed != null)
        {
            var storyboard = new Storyboard();
            var fadeOut = new DoubleAnimation
            {
                To = 0.3,
                Duration = AnimationHelper.Fast,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            fadeOut.Completed += (s, e) =>
            {
                TxtDashboardSpeed.Text = speed;
                var fadeIn = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = AnimationHelper.Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                TxtDashboardSpeed.BeginAnimation(OpacityProperty, fadeIn);
            };

            TxtDashboardSpeed.BeginAnimation(OpacityProperty, fadeOut);
        }
    }

    private void AnimateButtonClick(System.Windows.Controls.Button button)
    {
        if (button == null) return;

        // Quick pulse animation on click
        var storyboard = new Storyboard();
        var scaleX = new DoubleAnimation
        {
            To = 0.95,
            Duration = new Duration(TimeSpan.FromMilliseconds(75)),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleY = new DoubleAnimation
        {
            To = 0.95,
            Duration = new Duration(TimeSpan.FromMilliseconds(75)),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(scaleX, button);
        Storyboard.SetTarget(scaleY, button);
        Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Begin();
    }
}