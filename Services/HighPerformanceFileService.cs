using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FileCopyUtility.Services;

/// <summary>
/// High-Performance File Copy Service - Faster than Windows Explorer
/// Menggunakan advanced techniques untuk speed maksimal
/// </summary>
public partial class HighPerformanceFileService
{
    // This partial class contains only the declaration
    // All functionality has been moved to partial classes:
    // - HighPerformanceFileService.Core.cs (core functionality and Win32 API declarations)
    // - HighPerformanceFileService.Copy.cs (copy operations)
    // - HighPerformanceFileService.Helpers.cs (helper methods)
}

#region Performance Models

public class PerformanceCopyResult : CopyResult
{
    public int TotalFiles { get; set; }
    public long TotalBytes { get; set; }
    public long TotalBytesTransferred { get; set; }
    public double AverageSpeedMBps { get; set; }
    public double PeakSpeedMBps { get; set; }
    public double CurrentSpeedMBps { get; set; }
    public List<FileItem> SkippedFiles { get; set; } = new List<FileItem>();
    public List<FileItem> VerifiedFiles { get; set; } = new List<FileItem>();

    // Calculated properties for performance metrics
    public double PerformanceRatio => CalculatePerformanceRatio(AverageSpeedMBps);
    public string PerformanceGrade => CalculatePerformanceGrade(AverageSpeedMBps);

    // Helper methods for calculating performance metrics
    private double CalculatePerformanceRatio(double speedMBps)
    {
        const double baselineSpeed = 150.0; // Typical high-performance baseline
        return Math.Round(speedMBps / baselineSpeed, 3);
    }

    private string CalculatePerformanceGrade(double speedMBps)
    {
        if (speedMBps > 200) return "🚀 ULTRA FAST";
        if (speedMBps > 100) return "⚡ VERY FAST";
        if (speedMBps > 50) return "🔥 FAST";
        if (speedMBps > 20) return "✅ GOOD";
        if (speedMBps > 5) return "🟡 MODERATE";
        return "⚠️ SLOW";
    }
}

public class PerformanceProgressEventArgs : ProgressEventArgs
{
    public new long TotalBytes { get; set; }
    public long BytesTransferred { get; set; }
    public new double CopySpeedMBps { get; set; }
    public double PeakSpeedMBps { get; set; }
    public int SkippedCount { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }

    public int BytesProgress => TotalBytes > 0 ? (int)((BytesTransferred * 100) / TotalBytes) : 0;
    public string SpeedDisplay => CopySpeedMBps >= 1024 ? $"{CopySpeedMBps / 1024:F1} GB/s" : $"{CopySpeedMBps:F1} MB/s";
    public string PeakSpeedDisplay => PeakSpeedMBps >= 1024 ? $"{PeakSpeedMBps / 1024:F1} GB/s" : $"{PeakSpeedMBps:F1} MB/s";
    public string ETADisplay => EstimatedTimeRemaining.TotalSeconds > 0 ?
        EstimatedTimeRemaining.ToString(@"mm\:ss") + " remaining" : "Calculating...";
}

#endregion