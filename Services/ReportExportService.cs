using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk export reports ke berbagai format
/// </summary>
public class ReportExportService
{
    /// <summary>
    /// Export report to JSON format
    /// </summary>
    public async Task<string> ExportToJsonAsync(CopyResult result, string outputPath)
    {
        var report = new
        {
            GeneratedDate = DateTime.Now,
            Duration = result.Duration.ToString(@"hh\:mm\:ss"),
            Summary = new
            {
                TotalFiles = result.SuccessfulFiles.Count + result.FailedFiles.Count,
                Successful = result.SuccessfulFiles.Count,
                Failed = result.FailedFiles.Count,
                Cancelled = result.Cancelled
            },
            SuccessfulFiles = result.SuccessfulFiles.Select(f => new
            {
                f.Name,
                f.Path,
                Size = f.Size,
                SizeFormatted = FormatFileSize(f.Size)
            }),
            FailedFiles = result.FailedFiles.Select(f => new
            {
                f.File.Name,
                f.File.Path,
                f.Error
            })
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        var filePath = Path.Combine(outputPath, $"copy_report_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        await File.WriteAllTextAsync(filePath, json);
        
        return filePath;
    }

    /// <summary>
    /// Export report to CSV format
    /// </summary>
    public async Task<string> ExportToCsvAsync(CopyResult result, string outputPath)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("FileCopy Utility - Copy Report");
        csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Duration,{result.Duration:mm\\:ss}");
        csv.AppendLine($"Total Files,{result.SuccessfulFiles.Count + result.FailedFiles.Count}");
        csv.AppendLine($"Successful,{result.SuccessfulFiles.Count}");
        csv.AppendLine($"Failed,{result.FailedFiles.Count}");
        csv.AppendLine($"Cancelled,{result.Cancelled}");
        csv.AppendLine();
        
        // Successful files
        csv.AppendLine("Successful Files");
        csv.AppendLine("FileName,FilePath,Size,SizeFormatted");
        foreach (var file in result.SuccessfulFiles)
        {
            csv.AppendLine($"\"{file.Name}\",\"{file.Path}\",{file.Size},\"{FormatFileSize(file.Size)}\"");
        }
        
        csv.AppendLine();
        
        // Failed files
        if (result.FailedFiles.Any())
        {
            csv.AppendLine("Failed Files");
            csv.AppendLine("FileName,FilePath,Error");
            foreach (var file in result.FailedFiles)
            {
                csv.AppendLine($"\"{file.File.Name}\",\"{file.File.Path}\",\"{file.Error}\"");
            }
        }

        var filePath = Path.Combine(outputPath, $"copy_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await File.WriteAllTextAsync(filePath, csv.ToString());
        
        return filePath;
    }

    /// <summary>
    /// Export report to simple HTML (can be converted to PDF)
    /// </summary>
    public async Task<string> ExportToHtmlAsync(CopyResult result, string outputPath)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<title>FileCopy Utility - Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; }");
        html.AppendLine("h1 { color: #0078D4; }");
        html.AppendLine(".summary { background: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0; }");
        html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
        html.AppendLine("th { background: #0078D4; color: white; padding: 12px; text-align: left; }");
        html.AppendLine("td { padding: 10px; border-bottom: 1px solid #ddd; }");
        html.AppendLine(".success { color: #107C10; }");
        html.AppendLine(".error { color: #D13438; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        html.AppendLine("<h1>📁 FileCopy Utility - Copy Report</h1>");
        
        html.AppendLine("<div class='summary'>");
        html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine($"<p><strong>Duration:</strong> {result.Duration:mm\\:ss}</p>");
        html.AppendLine($"<p><strong>Total Files:</strong> {result.SuccessfulFiles.Count + result.FailedFiles.Count}</p>");
        html.AppendLine($"<p class='success'><strong>✅ Successful:</strong> {result.SuccessfulFiles.Count}</p>");
        html.AppendLine($"<p class='error'><strong>❌ Failed:</strong> {result.FailedFiles.Count}</p>");
        html.AppendLine($"<p><strong>Cancelled:</strong> {(result.Cancelled ? "Yes" : "No")}</p>");
        html.AppendLine("</div>");
        
        // Successful files table
        html.AppendLine("<h2>✅ Successful Files</h2>");
        html.AppendLine("<table>");
        html.AppendLine("<tr><th>File Name</th><th>Size</th><th>Path</th></tr>");
        foreach (var file in result.SuccessfulFiles)
        {
            html.AppendLine($"<tr><td>{file.Name}</td><td>{FormatFileSize(file.Size)}</td><td>{file.Path}</td></tr>");
        }
        html.AppendLine("</table>");
        
        // Failed files table
        if (result.FailedFiles.Any())
        {
            html.AppendLine("<h2>❌ Failed Files</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>File Name</th><th>Error</th><th>Path</th></tr>");
            foreach (var file in result.FailedFiles)
            {
                html.AppendLine($"<tr><td>{file.File.Name}</td><td class='error'>{file.Error}</td><td>{file.File.Path}</td></tr>");
            }
            html.AppendLine("</table>");
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        var filePath = Path.Combine(outputPath, $"copy_report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        await File.WriteAllTextAsync(filePath, html.ToString());
        
        return filePath;
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
