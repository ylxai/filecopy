using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for FileOperationService - File handling helpers
/// </summary>
public partial class FileOperationService
{
    /// <summary>
    /// Handle duplicate files based on policy
    /// </summary>
    private string HandleDuplicateFile(string destPath, DuplicateHandling handling)
    {
        if (!File.Exists(destPath))
        {
            return destPath;
        }

        return handling switch
        {
            DuplicateHandling.Overwrite => destPath,
            DuplicateHandling.Skip => string.Empty, // Empty means skip
            DuplicateHandling.Rename => GenerateUniqueFileName(destPath),
            _ => destPath
        };
    }

    /// <summary>
    /// Generate unique filename by adding number suffix
    /// </summary>
    private string GenerateUniqueFileName(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        int counter = 1;
        string newPath;

        do
        {
            newPath = Path.Combine(directory, $"{fileNameWithoutExt} ({counter}){extension}");
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }

    /// <summary>
    /// Copy single file dengan progress callback
    /// </summary>
    private async Task CopyFileWithProgressAsync(
        string sourcePath,
        string destPath,
        Action<int> progressCallback,
        CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80KB buffer

        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
        using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        var buffer = new byte[bufferSize];
        long totalBytes = sourceStream.Length;
        long copiedBytes = 0;
        int bytesRead;

        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            copiedBytes += bytesRead;

            var progress = (int)((copiedBytes * 100) / totalBytes);
            progressCallback?.Invoke(progress);
        }
    }

    /// <summary>
    /// Generate error log
    /// </summary>
    public async Task SaveErrorLogAsync(string logPath, List<FailedFileItem> failedFiles)
    {
        var logLines = new List<string>
        {
            $"FileCopy Utility - Error Log",
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"Total Errors: {failedFiles.Count}",
            "",
            "Failed Files:",
            "============="
        };

        foreach (var item in failedFiles)
        {
            logLines.Add($"\nFile: {item.File.Path}");
            logLines.Add($"Error: {item.Error}");
        }

        await File.WriteAllLinesAsync(logPath, logLines);
    }
}