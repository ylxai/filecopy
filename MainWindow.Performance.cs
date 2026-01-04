using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace FileCopyUtility;

/// <summary>
/// Partial class for MainWindow - Performance tracking functionality
/// </summary>
public partial class MainWindow
{
    // Performance tracking properties
    private DispatcherTimer? _performanceUpdateTimer;
    private Stopwatch _performanceStopwatch = new();
    private long _previousBytesCopied = 0;
    private DateTime _previousTime = DateTime.MinValue;
    private readonly Queue<double> _speedHistory = new Queue<double>(10); // Keep last 10 speed measurements

    // Initialize performance tracking components
    private void InitializePerformanceTracking()
    {
        // Initialize performance timer for real-time updates
        _performanceUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // Update every 500ms
        };
        _performanceUpdateTimer.Tick += PerformanceUpdateTimer_Tick;
    }

    // Start performance tracking
    private void StartPerformanceTracking()
    {
        _previousBytesCopied = _totalBytesCopied;
        _previousTime = DateTime.Now;
        _performanceStopwatch.Restart();
        _performanceUpdateTimer?.Start();
    }

    // Stop performance tracking
    private void StopPerformanceTracking()
    {
        _performanceUpdateTimer?.Stop();
        _performanceStopwatch.Stop();
    }

    // Update performance statistics in real-time
    private void PerformanceUpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (_copyStopwatch.IsRunning)
        {
            var currentTime = DateTime.Now;
            var currentBytes = _totalBytesCopied;

            // Calculate instantaneous speed
            var timeDiff = (currentTime - _previousTime).TotalSeconds;
            var bytesDiff = currentBytes - _previousBytesCopied;

            if (timeDiff > 0)
            {
                var currentSpeed = (bytesDiff / 1024.0 / 1024.0) / timeDiff; // MB/s
                
                // Add to history and maintain queue size
                _speedHistory.Enqueue(currentSpeed);
                if (_speedHistory.Count > 10)
                    _speedHistory.Dequeue();
                
                // Calculate average speed from history
                var avgSpeed = _speedHistory.Count > 0 ? _speedHistory.Average() : 0;
                
                // Update UI with real-time performance data
                Dispatcher.Invoke(() =>
                {
                    TxtDashboardSpeed.Text = $"{avgSpeed:F2} MB/s";
                    
                    // Update progress bar with percentage
                    if (_totalBytesToCopy > 0)
                    {
                        var progressPercentage = (double)_totalBytesCopied / _totalBytesToCopy * 100;
                        ProgressTotal.Value = progressPercentage;
                        
                        // Update status text
                        TxtTotalProgress.Text = $"Total Progress: {FormatFileSize(_totalBytesCopied)} / {FormatFileSize(_totalBytesToCopy)} ({progressPercentage:F1}%)";
                    }
                    
                    // Update elapsed time
                    TxtElapsedTime.Text = $"Time: {_performanceStopwatch.Elapsed:mm\\:ss}";
                });
                
                // Update previous values for next calculation
                _previousBytesCopied = currentBytes;
                _previousTime = currentTime;
            }
        }
    }

    // Calculate estimated time of arrival
    private TimeSpan CalculateETA(double currentSpeedMBps)
    {
        if (currentSpeedMBps <= 0 || _totalBytesToCopy <= 0)
            return TimeSpan.Zero;

        var remainingBytes = _totalBytesToCopy - _totalBytesCopied;
        var remainingMB = remainingBytes / (1024.0 * 1024.0);
        var secondsRemaining = remainingMB / currentSpeedMBps;

        return TimeSpan.FromSeconds(Math.Max(0, secondsRemaining));
    }

    // Get average speed over the session
    private double GetAverageSpeedMBps()
    {
        if (_performanceStopwatch.Elapsed.TotalSeconds <= 0)
            return 0;
            
        var totalMB = _totalBytesCopied / (1024.0 * 1024.0);
        return totalMB / _performanceStopwatch.Elapsed.TotalSeconds;
    }

    // Get peak speed recorded
    private double GetPeakSpeedMBps()
    {
        if (_speedHistory.Count == 0)
            return 0;
        return _speedHistory.Max();
    }
}