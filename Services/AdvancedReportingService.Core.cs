using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for AdvancedReportingService - Core functionality
/// </summary>
public partial class AdvancedReportingService
{
    public event EventHandler<string>? ReportGenerated;

    /// <summary>
    /// Generate comprehensive copy operation report
    /// </summary>
    public async Task<CopyReport> GenerateDetailedReportAsync(PerformanceCopyResult result, string sourceFolder)
    {
        var report = new CopyReport
        {
            ReportId = Guid.NewGuid().ToString(),
            GeneratedAt = DateTime.Now,
            SourceFolder = sourceFolder,
            OperationDuration = result.Duration,
            TotalFilesProcessed = result.SuccessfulFiles.Count + result.FailedFiles.Count,
            SuccessfulFiles = result.SuccessfulFiles.Count,
            FailedFiles = result.FailedFiles.Count,
            TotalBytesTransferred = result.TotalBytesTransferred,
            AverageSpeedMBps = result.AverageSpeedMBps,
            PeakSpeedMBps = result.PeakSpeedMBps,
            PerformanceGrade = result.PerformanceGrade,
            PerformanceRatio = result.PerformanceRatio
        };

        // Analyze file types and sizes
        await AnalyzeFileDistribution(report, result);

        // Calculate performance metrics
        CalculateDetailedMetrics(report, result);

        // Analyze errors if any
        if (result.FailedFiles.Any())
        {
            AnalyzeErrors(report, result.FailedFiles);
        }

        return report;
    }

    /// <summary>
    /// Analyze file distribution by type and size
    /// </summary>
    private async Task AnalyzeFileDistribution(CopyReport report, PerformanceCopyResult result)
    {
        await Task.Run(() =>
        {
            var fileAnalysis = new Dictionary<string, FileTypeStats>();
            var sizeRanges = new Dictionary<string, int>
            {
                ["< 1MB"] = 0,
                ["1-10MB"] = 0,
                ["10-100MB"] = 0,
                ["100MB-1GB"] = 0,
                ["> 1GB"] = 0
            };

            foreach (var file in result.SuccessfulFiles)
            {
                // File type analysis
                var extension = Path.GetExtension(file.Name).ToLower();
                if (!fileAnalysis.ContainsKey(extension))
                {
                    fileAnalysis[extension] = new FileTypeStats { Extension = extension };
                }

                var stats = fileAnalysis[extension];
                stats.Count++;
                stats.TotalSize += file.Size;
                stats.AverageSize = stats.TotalSize / stats.Count;

                // Size range analysis
                var sizeMB = file.Size / (1024.0 * 1024.0);
                if (sizeMB < 1) sizeRanges["< 1MB"]++;
                else if (sizeMB < 10) sizeRanges["1-10MB"]++;
                else if (sizeMB < 100) sizeRanges["10-100MB"]++;
                else if (sizeMB < 1024) sizeRanges["100MB-1GB"]++;
                else sizeRanges["> 1GB"]++;
            }

            report.FileTypeDistribution = fileAnalysis.Values.OrderByDescending(f => f.TotalSize).ToList();
            report.SizeDistribution = sizeRanges;
        });
    }

    /// <summary>
    /// Calculate detailed performance metrics
    /// </summary>
    private void CalculateDetailedMetrics(CopyReport report, PerformanceCopyResult result)
    {
        report.Metrics = new PerformanceMetrics
        {
            ThroughputMBps = result.AverageSpeedMBps,
            EfficiencyRating = CalculateEfficiencyRating(result),
            TimePerFile = result.Duration.TotalSeconds / Math.Max(1, result.SuccessfulFiles.Count),
            BytesPerSecond = result.TotalBytesTransferred / Math.Max(1, result.Duration.TotalSeconds),
            FilesPerSecond = result.SuccessfulFiles.Count / Math.Max(1, result.Duration.TotalSeconds),
            MemoryEfficiency = EstimateMemoryEfficiency(result),
            SystemUtilization = EstimateSystemUtilization(result)
        };

        // Performance recommendations
        GeneratePerformanceRecommendations(report);
    }

    /// <summary>
    /// Analyze errors and categorize them
    /// </summary>
    private void AnalyzeErrors(CopyReport report, List<FailedFileItem> failedFiles)
    {
        var errorCategories = new Dictionary<string, int>();

        foreach (var failed in failedFiles)
        {
            var category = CategorizeError(failed.Error);
            errorCategories[category] = errorCategories.GetValueOrDefault(category, 0) + 1;
        }

        report.ErrorAnalysis = new ErrorAnalysis
        {
            TotalErrors = failedFiles.Count,
            ErrorCategories = errorCategories,
            MostCommonError = errorCategories.OrderByDescending(kv => kv.Value).FirstOrDefault().Key ?? "Unknown",
            FailedFiles = failedFiles.Take(10).ToList() // Top 10 for report
        };
    }

    protected virtual void OnReportGenerated(string message)
    {
        ReportGenerated?.Invoke(this, message);
    }
}