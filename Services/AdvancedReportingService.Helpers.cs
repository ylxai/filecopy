using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for AdvancedReportingService - Helper methods
/// </summary>
public partial class AdvancedReportingService
{
    /// <summary>
    /// Helper methods for report generation
    /// </summary>
    private double CalculateEfficiencyRating(PerformanceCopyResult result)
    {
        // Calculate efficiency based on theoretical maximum vs actual performance
        var theoreticalMax = 1000.0; // MB/s theoretical max for modern SSDs
        return Math.Min(100, (result.AverageSpeedMBps / theoreticalMax) * 100);
    }

    private double EstimateMemoryEfficiency(PerformanceCopyResult result)
    {
        // Estimate based on file count and average speed
        return Math.Min(100, 85 + (result.AverageSpeedMBps / 100.0) * 15);
    }

    private double EstimateSystemUtilization(PerformanceCopyResult result)
    {
        // Estimate system resource utilization
        return Math.Min(100, 60 + (result.PerformanceRatio * 10));
    }

    private void GeneratePerformanceRecommendations(CopyReport report)
    {
        var recommendations = new List<string>();

        if (report.AverageSpeedMBps < 50)
        {
            recommendations.Add("🔧 Consider using SSD drives for better performance");
            recommendations.Add("⚡ Increase thread count for parallel processing");
        }

        if (report.PerformanceRatio < 2.0)
        {
            recommendations.Add("💾 Enable memory optimization settings");
            recommendations.Add("🚀 Use ultra-fast mode for large file operations");
        }

        if (report.FailedFiles > 0)
        {
            recommendations.Add("🛡️ Enable auto-retry for better reliability");
            recommendations.Add("📝 Review failed files and check file permissions");
        }

        if (report.FileTypeDistribution.Any(ft => ft.Extension == ".raw" || ft.Extension == ".nef"))
        {
            recommendations.Add("📸 RAW files detected - consider enabling integrity verification");
            recommendations.Add("💾 Use memory mapping for better large file handling");
        }

        recommendations.Add("📊 Regular cleanup of temp files improves performance");
        recommendations.Add("🔄 Schedule regular maintenance for optimal speed");

        report.Recommendations = recommendations;
    }

    private string CategorizeError(string error)
    {
        if (error.Contains("access", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("permission", StringComparison.OrdinalIgnoreCase))
            return "Access/Permission";

        if (error.Contains("space", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("disk", StringComparison.OrdinalIgnoreCase))
            return "Disk Space";

        if (error.Contains("network", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("path", StringComparison.OrdinalIgnoreCase))
            return "Network/Path";

        return "Other";
    }

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

    private string GetGradeClass(string grade)
    {
        return grade.ToLower().Replace(" ", "-");
    }
}