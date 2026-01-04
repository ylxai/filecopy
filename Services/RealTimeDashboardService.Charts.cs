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
/// Partial class for RealTimeDashboardService - Chart creation and updates
/// </summary>
public partial class RealTimeDashboardService
{
    /// <summary>
    /// Create real-time speed chart
    /// </summary>
    public Canvas CreateSpeedChart(double width = 300, double height = 150)
    {
        var canvas = new Canvas
        {
            Width = width,
            Height = height,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 0xb8, 0x54, 0x93))
        };

        // Grid lines
        for (int i = 0; i <= 4; i++)
        {
            var y = (height / 4) * i;
            var line = new Line
            {
                X1 = 0, X2 = width,
                Y1 = y, Y2 = y,
                Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0x6b, 0x65, 0x70)),
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }

        return canvas;
    }

    /// <summary>
    /// Update speed chart with new data point
    /// </summary>
    public void UpdateSpeedChart(Canvas chart, double speedMBps)
    {
        var timestamp = DateTime.Now;
        _speedHistory.Enqueue(new DataPoint { Timestamp = timestamp, Value = speedMBps });

        // Remove old data points
        while (_speedHistory.Count > _maxDataPoints)
        {
            _speedHistory.Dequeue();
        }

        RedrawSpeedChart(chart);
    }

    private void RedrawSpeedChart(Canvas chart)
    {
        // Clear existing polylines
        var linesToRemove = chart.Children.OfType<Polyline>().ToList();
        foreach (var line in linesToRemove)
        {
            chart.Children.Remove(line);
        }

        if (_speedHistory.Count < 2) return;

        var points = new PointCollection();
        var maxSpeed = Math.Max(100, _speedHistory.Max(d => d.Value)); // Minimum scale of 100 MB/s

        for (int i = 0; i < _speedHistory.Count; i++)
        {
            var x = (chart.Width / (_maxDataPoints - 1)) * i;
            var y = chart.Height - ((_speedHistory.ElementAt(i).Value / maxSpeed) * chart.Height);
            points.Add(new System.Windows.Point(x, y));
        }

        var polyline = new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e)),
            StrokeThickness = 2,
            Fill = new LinearGradientBrush(
                System.Windows.Media.Color.FromArgb(60, 0x22, 0xc5, 0x5e),
                System.Windows.Media.Color.FromArgb(10, 0x22, 0xc5, 0x5e),
                new System.Windows.Point(0, 0), new System.Windows.Point(0, 1))
        };

        chart.Children.Add(polyline);
    }
}