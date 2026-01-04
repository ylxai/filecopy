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
/// Partial class for RealTimeDashboardService - Timeline and heatmap
/// </summary>
public partial class RealTimeDashboardService
{
    /// <summary>
    /// Create file transfer timeline
    /// </summary>
    public StackPanel CreateTransferTimeline()
    {
        var timeline = new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Vertical,
            Margin = new Thickness(10)
        };

        var header = new TextBlock
        {
            Text = "📈 Transfer Timeline",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x65, 0x70)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        timeline.Children.Add(header);
        return timeline;
    }

    /// <summary>
    /// Add timeline entry
    /// </summary>
    public void AddTimelineEntry(StackPanel timeline, string fileName, double sizeMB, double speedMBps, bool success)
    {
        var entry = new Border
        {
            Background = success
                ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 0x22, 0xc5, 0x5e))
                : new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 0xef, 0x44, 0x44)),
            BorderBrush = success
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xef, 0x44, 0x44)),
            BorderThickness = new Thickness(1, 0, 0, 0),
            Padding = new Thickness(8, 4, 8, 4),
            Margin = new Thickness(0, 1, 0, 1)
        };

        var entryPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

        var statusIcon = new TextBlock
        {
            Text = success ? "✅" : "❌",
            FontSize = 12,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var fileInfo = new TextBlock
        {
            Text = $"{fileName} ({sizeMB:F1} MB)",
            FontSize = 11,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x65, 0x70)),
            VerticalAlignment = VerticalAlignment.Center,
            Width = 150,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var speedInfo = new TextBlock
        {
            Text = success ? $"{speedMBps:F1} MB/s" : "Failed",
            FontSize = 11,
            Foreground = success
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xef, 0x44, 0x44)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };

        entryPanel.Children.Add(statusIcon);
        entryPanel.Children.Add(fileInfo);
        entryPanel.Children.Add(speedInfo);
        entry.Child = entryPanel;

        timeline.Children.Insert(1, entry); // Insert after header

        // Keep only last 10 entries
        while (timeline.Children.Count > 11) // Header + 10 entries
        {
            timeline.Children.RemoveAt(timeline.Children.Count - 1);
        }
    }

    /// <summary>
    /// Create performance heatmap
    /// </summary>
    public Grid CreatePerformanceHeatmap(int rows = 8, int cols = 12)
    {
        var grid = new Grid();

        // Create grid structure
        for (int i = 0; i < rows; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        for (int i = 0; i < cols; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // Fill with initial cells
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var cell = new System.Windows.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0xb8, 0x54, 0x93)),
                    Margin = new Thickness(1),
                    RadiusX = 2,
                    RadiusY = 2
                };

                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                grid.Children.Add(cell);
            }
        }

        return grid;
    }

    /// <summary>
    /// Update heatmap with performance data
    /// </summary>
    public void UpdateHeatmap(Grid heatmap, double intensity)
    {
        // Shift existing data left
        var rows = heatmap.RowDefinitions.Count;
        var cols = heatmap.ColumnDefinitions.Count;

        // Move existing rectangles
        for (int col = 0; col < cols - 1; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var currentRect = GetRectangleAt(heatmap, row, col + 1);
                if (currentRect != null)
                {
                    Grid.SetColumn(currentRect, col);
                }
            }
        }

        // Add new column data
        var normalizedIntensity = Math.Min(1.0, intensity / 500.0); // Normalize to 500 MB/s max
        for (int row = 0; row < rows; row++)
        {
            var cellIntensity = Math.Max(0, normalizedIntensity - (row * 0.125)); // Decrease by row
            var alpha = (byte)(cellIntensity * 255);

            var newRect = new System.Windows.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, 0x22, 0xc5, 0x5e)),
                Margin = new Thickness(1),
                RadiusX = 2,
                RadiusY = 2
            };

            Grid.SetRow(newRect, row);
            Grid.SetColumn(newRect, cols - 1);

            var oldRect = GetRectangleAt(heatmap, row, cols - 1);
            if (oldRect != null)
            {
                heatmap.Children.Remove(oldRect);
            }

            heatmap.Children.Add(newRect);
        }
    }

    private System.Windows.Shapes.Rectangle? GetRectangleAt(Grid grid, int row, int col)
    {
        return grid.Children.OfType<System.Windows.Shapes.Rectangle>()
            .FirstOrDefault(r => Grid.GetRow(r) == row && Grid.GetColumn(r) == col);
    }
}