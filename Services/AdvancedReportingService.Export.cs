using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for AdvancedReportingService - Export functionality
/// </summary>
public partial class AdvancedReportingService
{
    /// <summary>
    /// Export report to various formats
    /// </summary>
    public async Task<string> ExportReportAsync(CopyReport report, ExportFormat format, string outputPath)
    {
        var fileName = $"FileCopy_Report_{report.GeneratedAt:yyyyMMdd_HHmmss}";
        var fullPath = format switch
        {
            ExportFormat.JSON => await ExportToJsonAsync(report, Path.Combine(outputPath, $"{fileName}.json")),
            ExportFormat.HTML => await ExportToHtmlAsync(report, Path.Combine(outputPath, $"{fileName}.html")),
            ExportFormat.CSV => await ExportToCsvAsync(report, Path.Combine(outputPath, $"{fileName}.csv")),
            ExportFormat.TXT => await ExportToTextAsync(report, Path.Combine(outputPath, $"{fileName}.txt")),
            _ => throw new ArgumentException("Unsupported export format")
        };

        OnReportGenerated($"📊 Report exported: {fullPath}");
        return fullPath;
    }

    /// <summary>
    /// Export to JSON format
    /// </summary>
    private async Task<string> ExportToJsonAsync(CopyReport report, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(report, options);
        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }

    /// <summary>
    /// Export to beautiful HTML format with charts
    /// </summary>
    private async Task<string> ExportToHtmlAsync(CopyReport report, string filePath)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>FileCopy Utility - Operation Report</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f8f9fa; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; color: #b85493; margin-bottom: 30px; }}
        .header h1 {{ margin: 0; font-size: 2.5em; }}
        .header p {{ color: #6b6570; font-size: 1.2em; margin: 10px 0; }}
        .metrics {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0; }}
        .metric-card {{ background: linear-gradient(135deg, #b85493, #a04682); color: white; padding: 20px; border-radius: 12px; text-align: center; }}
        .metric-value {{ font-size: 2em; font-weight: bold; }}
        .metric-label {{ font-size: 0.9em; opacity: 0.9; }}
        .section {{ margin: 30px 0; }}
        .section h2 {{ color: #6b6570; border-bottom: 2px solid #e8e5f0; padding-bottom: 10px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        th, td {{ padding: 12px; text-align: left; border-bottom: 1px solid #e8e5f0; }}
        th {{ background-color: #f7f6f8; color: #6b6570; font-weight: 600; }}
        .performance-grade {{ padding: 5px 15px; border-radius: 20px; font-weight: bold; }}
        .ultra-fast {{ background: #22c55e; color: white; }}
        .fast {{ background: #f59e0b; color: white; }}
        .good {{ background: #3b82f6; color: white; }}
        .slow {{ background: #ef4444; color: white; }}
        .chart-container {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 15px 0; }}
        .progress-bar {{ background: #e8e5f0; border-radius: 10px; overflow: hidden; }}
        .progress-fill {{ background: linear-gradient(90deg, #b85493, #22c55e); height: 20px; }}
        .timestamp {{ text-align: center; color: #9ca3af; margin-top: 30px; font-style: italic; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚀 FileCopy Utility Report</h1>
            <p>Ultra-Fast Copy Operation Analysis</p>
            <p>{report.GeneratedAt:dddd, MMMM dd, yyyy 'at' HH:mm:ss}</p>
        </div>

        <div class='metrics'>
            <div class='metric-card'>
                <div class='metric-value'>{report.SuccessfulFiles:N0}</div>
                <div class='metric-label'>Files Copied Successfully</div>
            </div>
            <div class='metric-card'>
                <div class='metric-value'>{FormatFileSize(report.TotalBytesTransferred)}</div>
                <div class='metric-label'>Total Data Transferred</div>
            </div>
            <div class='metric-card'>
                <div class='metric-value'>{report.AverageSpeedMBps:F1} MB/s</div>
                <div class='metric-label'>Average Copy Speed</div>
            </div>
            <div class='metric-card'>
                <div class='metric-value'>{report.OperationDuration:mm\\:ss}</div>
                <div class='metric-label'>Total Duration</div>
            </div>
        </div>

        <div class='section'>
            <h2>📊 Performance Summary</h2>
            <table>
                <tr><td><strong>Performance Grade:</strong></td><td><span class='performance-grade {GetGradeClass(report.PerformanceGrade)}'>{report.PerformanceGrade}</span></td></tr>
                <tr><td><strong>Speed vs Explorer:</strong></td><td>{report.PerformanceRatio:F1}x faster</td></tr>
                <tr><td><strong>Peak Speed:</strong></td><td>{report.PeakSpeedMBps:F1} MB/s</td></tr>
                <tr><td><strong>Efficiency Rating:</strong></td><td>{report.Metrics?.EfficiencyRating:F1}%</td></tr>
                <tr><td><strong>Files per Second:</strong></td><td>{report.Metrics?.FilesPerSecond:F1}</td></tr>
            </table>
        </div>

        <div class='section'>
            <h2>📁 File Type Distribution</h2>
            <table>
                <tr><th>File Type</th><th>Count</th><th>Total Size</th><th>Avg Size</th><th>% of Total</th></tr>
                {string.Join("", report.FileTypeDistribution.Select(ft =>
                    $"<tr><td><strong>{ft.Extension}</strong></td><td>{ft.Count:N0}</td><td>{FormatFileSize(ft.TotalSize)}</td><td>{FormatFileSize(ft.AverageSize)}</td><td>{(ft.TotalSize * 100.0 / report.TotalBytesTransferred):F1}%</td></tr>"))}
            </table>
        </div>

        {(report.ErrorAnalysis != null ? $@"
        <div class='section'>
            <h2>⚠️ Error Analysis</h2>
            <p><strong>Total Errors:</strong> {report.ErrorAnalysis.TotalErrors}</p>
            <p><strong>Most Common:</strong> {report.ErrorAnalysis.MostCommonError}</p>
            <table>
                <tr><th>Error Category</th><th>Count</th></tr>
                {string.Join("", report.ErrorAnalysis.ErrorCategories.Select(ec =>
                    $"<tr><td>{ec.Key}</td><td>{ec.Value}</td></tr>"))}
            </table>
        </div>" : "")}

        <div class='section'>
            <h2>💡 Performance Recommendations</h2>
            <ul>
                {string.Join("", report.Recommendations.Select(rec => $"<li>{rec}</li>"))}
            </ul>
        </div>

        <div class='timestamp'>
            Report generated by FileCopy Utility v2.0 - Ultra-Fast Edition
        </div>
    </div>
</body>
</html>";

        await File.WriteAllTextAsync(filePath, html);
        return filePath;
    }

    /// <summary>
    /// Export to CSV format for data analysis
    /// </summary>
    private async Task<string> ExportToCsvAsync(CopyReport report, string filePath)
    {
        var csv = "Metric,Value\n" +
                 $"Report ID,{report.ReportId}\n" +
                 $"Generated At,{report.GeneratedAt:yyyy-MM-dd HH:mm:ss}\n" +
                 $"Source Folder,{report.SourceFolder}\n" +
                 $"Duration (seconds),{report.OperationDuration.TotalSeconds:F2}\n" +
                 $"Total Files,{report.TotalFilesProcessed}\n" +
                 $"Successful Files,{report.SuccessfulFiles}\n" +
                 $"Failed Files,{report.FailedFiles}\n" +
                 $"Total Bytes,{report.TotalBytesTransferred}\n" +
                 $"Average Speed (MB/s),{report.AverageSpeedMBps:F2}\n" +
                 $"Peak Speed (MB/s),{report.PeakSpeedMBps:F2}\n" +
                 $"Performance Grade,{report.PerformanceGrade}\n" +
                 $"Performance Ratio,{report.PerformanceRatio:F2}\n";

        await File.WriteAllTextAsync(filePath, csv);
        return filePath;
    }

    /// <summary>
    /// Export to detailed text format
    /// </summary>
    private async Task<string> ExportToTextAsync(CopyReport report, string filePath)
    {
        var text = $@"
╔══════════════════════════════════════════════════════════════════════════════════════╗
║                           🚀 FILECOPY UTILITY REPORT                                ║
║                              Ultra-Fast Copy Analysis                                ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

📅 Generated: {report.GeneratedAt:dddd, MMMM dd, yyyy 'at' HH:mm:ss}
🆔 Report ID: {report.ReportId}

╔══════════════════════════════════════════════════════════════════════════════════════╗
║                               📊 OPERATION SUMMARY                                  ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

📁 Source Folder: {report.SourceFolder}
⏱️  Total Duration: {report.OperationDuration:hh\\:mm\\:ss}
📄 Total Files Processed: {report.TotalFilesProcessed:N0}

✅ Successful: {report.SuccessfulFiles:N0} files
❌ Failed: {report.FailedFiles:N0} files
📊 Success Rate: {(report.SuccessfulFiles * 100.0 / Math.Max(1, report.TotalFilesProcessed)):F1}%

💾 Total Data Transferred: {FormatFileSize(report.TotalBytesTransferred)}
⚡ Average Speed: {report.AverageSpeedMBps:F1} MB/s
🚀 Peak Speed: {report.PeakSpeedMBps:F1} MB/s
🏆 Performance Grade: {report.PerformanceGrade}
🔥 Speedup vs Explorer: {report.PerformanceRatio:F1}x faster!

╔══════════════════════════════════════════════════════════════════════════════════════╗
║                              🎯 PERFORMANCE METRICS                                 ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

📈 Throughput: {report.Metrics?.ThroughputMBps:F1} MB/s
⚡ Efficiency Rating: {report.Metrics?.EfficiencyRating:F1}%
⏱️  Time per File: {report.Metrics?.TimePerFile:F2} seconds
📊 Files per Second: {report.Metrics?.FilesPerSecond:F1}
💾 Memory Efficiency: {report.Metrics?.MemoryEfficiency:F1}%
🖥️  System Utilization: {report.Metrics?.SystemUtilization:F1}%

╔══════════════════════════════════════════════════════════════════════════════════════╗
║                              📁 FILE TYPE ANALYSIS                                  ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

{string.Join("\n", report.FileTypeDistribution.Select(ft =>
    $"{ft.Extension.PadRight(8)} │ {ft.Count.ToString("N0").PadLeft(8)} files │ {FormatFileSize(ft.TotalSize).PadLeft(12)} │ {(ft.TotalSize * 100.0 / report.TotalBytesTransferred):F1}%"))}

{(report.ErrorAnalysis != null ? $@"
╔══════════════════════════════════════════════════════════════════════════════════════╗
║                                ⚠️  ERROR ANALYSIS                                   ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

Total Errors: {report.ErrorAnalysis.TotalErrors}
Most Common Error: {report.ErrorAnalysis.MostCommonError}

Error Categories:
{string.Join("\n", report.ErrorAnalysis.ErrorCategories.Select(ec => $"  • {ec.Key}: {ec.Value} occurrences"))}" : "")}

╔══════════════════════════════════════════════════════════════════════════════════════╗
║                            💡 PERFORMANCE RECOMMENDATIONS                           ║
╚══════════════════════════════════════════════════════════════════════════════════════╝

{string.Join("\n", report.Recommendations.Select(rec => $"  • {rec}"))}

═══════════════════════════════════════════════════════════════════════════════════════
Generated by FileCopy Utility v2.0 - Ultra-Fast Edition with OKLCH Design
═══════════════════════════════════════════════════════════════════════════════════════
";

        await File.WriteAllTextAsync(filePath, text);
        return filePath;
    }
}