using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Advanced reporting service with detailed analytics and export capabilities
/// </summary>
public partial class AdvancedReportingService
{
    // This partial class contains only the declaration
    // All functionality has been moved to partial classes:
    // - AdvancedReportingService.Core.cs (core functionality)
    // - AdvancedReportingService.Export.cs (export functionality)
    // - AdvancedReportingService.Helpers.cs (helper methods)
}

#region Report Models

public class CopyReport
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string SourceFolder { get; set; } = string.Empty;
    public TimeSpan OperationDuration { get; set; }
    public int TotalFilesProcessed { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public long TotalBytesTransferred { get; set; }
    public double AverageSpeedMBps { get; set; }
    public double PeakSpeedMBps { get; set; }
    public string PerformanceGrade { get; set; } = string.Empty;
    public double PerformanceRatio { get; set; }
    public PerformanceMetrics? Metrics { get; set; }
    public List<FileTypeStats> FileTypeDistribution { get; set; } = new();
    public Dictionary<string, int> SizeDistribution { get; set; } = new();
    public ErrorAnalysis? ErrorAnalysis { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class PerformanceMetrics
{
    public double ThroughputMBps { get; set; }
    public double EfficiencyRating { get; set; }
    public double TimePerFile { get; set; }
    public double BytesPerSecond { get; set; }
    public double FilesPerSecond { get; set; }
    public double MemoryEfficiency { get; set; }
    public double SystemUtilization { get; set; }
}

public class FileTypeStats
{
    public string Extension { get; set; } = string.Empty;
    public int Count { get; set; }
    public long TotalSize { get; set; }
    public long AverageSize { get; set; }
}

public class ErrorAnalysis
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorCategories { get; set; } = new();
    public string MostCommonError { get; set; } = string.Empty;
    public List<FailedFileItem> FailedFiles { get; set; } = new();
}

public enum ExportFormat
{
    JSON,
    HTML,
    CSV,
    TXT
}

#endregion