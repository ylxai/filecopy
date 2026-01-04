using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for HighPerformanceFileService - Helper methods
/// </summary>
public partial class HighPerformanceFileService
{
    /// <summary>
    /// Group files by size for optimal batch processing
    /// Small files together, large files get dedicated threads
    /// </summary>
    private List<List<FileItem>> GroupFilesByOptimalSize(List<FileItem> files)
    {
        var groups = new List<List<FileItem>>();
        var smallFiles = new List<FileItem>();
        var largeFiles = new List<FileItem>();

        foreach (var file in files)
        {
            if (file.Size > 50_000_000) // 50MB+
                largeFiles.Add(file);
            else
                smallFiles.Add(file);
        }

        // Large files get individual processing
        foreach (var largeFile in largeFiles)
        {
            groups.Add(new List<FileItem> { largeFile });
        }

        // Small files are batched together
        if (smallFiles.Any())
        {
            // Split small files into optimal batches
            var batchSize = Math.Max(1, smallFiles.Count / Environment.ProcessorCount);
            for (int i = 0; i < smallFiles.Count; i += batchSize)
            {
                groups.Add(smallFiles.Skip(i).Take(batchSize).ToList());
            }
        }

        return groups;
    }

    /// <summary>
    /// Ensure destination directory structure exists
    /// Pre-creating reduces overhead during copy
    /// </summary>
    private void EnsureDestinationStructure(string destinationFolder)
    {
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }
    }

    /// <summary>
    /// Calculate estimated time remaining
    /// </summary>
    private TimeSpan CalculateETA(double speedMBps, long remainingBytes)
    {
        if (speedMBps <= 0) return TimeSpan.Zero;

        var remainingMB = remainingBytes / (1024.0 * 1024.0);
        var secondsRemaining = remainingMB / speedMBps;

        return TimeSpan.FromSeconds(Math.Max(0, secondsRemaining));
    }

    /// <summary>
    /// Format file size untuk display
    /// </summary>
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

    /// <summary>
    /// Format speed untuk display
    /// </summary>
    private string FormatSpeed(double mbps)
    {
        if (mbps >= 1024)
            return $"{mbps / 1024:F1} GB/s";
        return $"{mbps:F1} MB/s";
    }
}