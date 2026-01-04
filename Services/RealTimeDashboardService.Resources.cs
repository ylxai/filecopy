using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for RealTimeDashboardService - Resource monitoring
/// </summary>
public partial class RealTimeDashboardService
{
    /// <summary>
    /// Create system resource monitor
    /// </summary>
    public Grid CreateResourceMonitor()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // CPU Usage
        var cpuPanel = CreateResourcePanel("💻 CPU Usage", "#3b82f6");
        Grid.SetRow(cpuPanel, 0);
        grid.Children.Add(cpuPanel);

        // Memory Usage
        var memoryPanel = CreateResourcePanel("💾 Memory Usage", "#22c55e");
        Grid.SetRow(memoryPanel, 1);
        grid.Children.Add(memoryPanel);

        // Disk I/O
        var diskPanel = CreateResourcePanel("💽 Disk I/O", "#f59e0b");
        Grid.SetRow(diskPanel, 2);
        grid.Children.Add(diskPanel);

        return grid;
    }

    private Border CreateResourcePanel(string title, string color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 0xb8, 0x54, 0x93)),
            BorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)!),
            BorderThickness = new Thickness(2, 0, 0, 0),
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(0, 2, 0, 2)
        };

        var panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x65, 0x70)),
            VerticalAlignment = VerticalAlignment.Center,
            Width = 120
        };

        var progressBar = new System.Windows.Controls.ProgressBar
        {
            Width = 100,
            Height = 6,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xe8, 0xe5, 0xf0)),
            Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)!),
            Value = 0,
            Margin = new Thickness(10, 0, 10, 0)
        };

        var valueText = new TextBlock
        {
            Text = "0%",
            FontSize = 11,
            Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)!),
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(titleText);
        panel.Children.Add(progressBar);
        panel.Children.Add(valueText);
        border.Child = panel;

        return border;
    }
}