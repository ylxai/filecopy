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
/// Real-time dashboard service with live analytics and visualizations
/// </summary>
public partial class RealTimeDashboardService
{
    // This partial class contains only the declaration
    // All functionality has been moved to partial classes:
    // - RealTimeDashboardService.Core.cs (core functionality)
    // - RealTimeDashboardService.Charts.cs (chart creation and updates)
    // - RealTimeDashboardService.Resources.cs (resource monitoring)
    // - RealTimeDashboardService.Timeline.cs (timeline and heatmap)
}

#region Dashboard Models

public class DataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

public class DashboardStats
{
    public double CurrentSpeed { get; set; }
    public double AverageSpeed { get; set; }
    public double PeakSpeed { get; set; }
    public int DataPoints { get; set; }
    public TimeSpan Uptime { get; set; }
}

public class DashboardUpdateEventArgs : EventArgs
{
    public DashboardStats Stats { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

#endregion