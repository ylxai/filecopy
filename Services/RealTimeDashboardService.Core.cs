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
/// Partial class for RealTimeDashboardService - Core functionality
/// </summary>
public partial class RealTimeDashboardService
{
    private readonly DispatcherTimer _updateTimer;
    private readonly Queue<DataPoint> _speedHistory;
    private readonly Queue<DataPoint> _throughputHistory;
    private readonly int _maxDataPoints = 60; // 1 minute of history at 1-second intervals

    public event EventHandler<DashboardUpdateEventArgs>? DashboardUpdated;

    public RealTimeDashboardService()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += UpdateTimer_Tick;

        _speedHistory = new Queue<DataPoint>();
        _throughputHistory = new Queue<DataPoint>();
    }

    /// <summary>
    /// Start real-time updates
    /// </summary>
    public void StartRealTimeUpdates()
    {
        _updateTimer.Start();
    }

    /// <summary>
    /// Stop real-time updates
    /// </summary>
    public void StopRealTimeUpdates()
    {
        _updateTimer.Stop();
    }

    /// <summary>
    /// Get current dashboard statistics
    /// </summary>
    public DashboardStats GetCurrentStats()
    {
        var recentSpeed = _speedHistory.LastOrDefault()?.Value ?? 0;
        var avgSpeed = _speedHistory.Count > 0 ? _speedHistory.Average(d => d.Value) : 0;
        var peakSpeed = _speedHistory.Count > 0 ? _speedHistory.Max(d => d.Value) : 0;

        return new DashboardStats
        {
            CurrentSpeed = recentSpeed,
            AverageSpeed = avgSpeed,
            PeakSpeed = peakSpeed,
            DataPoints = _speedHistory.Count,
            Uptime = _speedHistory.Count > 0 ?
                DateTime.Now - _speedHistory.First().Timestamp :
                TimeSpan.Zero
        };
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        var stats = GetCurrentStats();
        OnDashboardUpdated(new DashboardUpdateEventArgs
        {
            Stats = stats,
            Timestamp = DateTime.Now
        });
    }

    protected virtual void OnDashboardUpdated(DashboardUpdateEventArgs e)
    {
        DashboardUpdated?.Invoke(this, e);
    }
}