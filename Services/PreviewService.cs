using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Preview service for showing file information before copy operations
/// </summary>
public class PreviewService
{
    public event EventHandler<PreviewUpdateEventArgs>? PreviewUpdated;

    /// <summary>
    /// Generate comprehensive preview data for files
    /// </summary>
    public async Task<PreviewData> GeneratePreviewAsync(List<FileItem> files, string sourceFolder)
    {
        var preview = new PreviewData
        {
            TotalFiles = files.Count,
            SourceFolder = sourceFolder,
            GeneratedAt = DateTime.Now
        };

        await Task.Run(() =>
        {
            // Analyze file types
            AnalyzeFileTypes(preview, files);
            
            // Calculate sizes
            CalculateSizes(preview, files);
            
            // Generate thumbnails for images (sample)
            GenerateThumbnails(preview, files.Take(5).ToList());
            
            // Estimate copy time
            EstimateCopyTime(preview);
        });

        OnPreviewUpdated(new PreviewUpdateEventArgs { Preview = preview });
        return preview;
    }

    /// <summary>
    /// Analyze file type distribution
    /// </summary>
    private void AnalyzeFileTypes(PreviewData preview, List<FileItem> files)
    {
        var typeGroups = files.GroupBy(f => Path.GetExtension(f.Name).ToLower())
                              .OrderByDescending(g => g.Count())
                              .ToList();

        foreach (var group in typeGroups)
        {
            var extension = string.IsNullOrEmpty(group.Key) ? "No Extension" : group.Key;
            var count = group.Count();
            var totalSize = group.Sum(f => f.Size);

            preview.FileTypes.Add(new FileTypeInfo
            {
                Extension = extension,
                Count = count,
                TotalSize = totalSize,
                AverageSize = totalSize / count,
                Icon = GetFileTypeIcon(extension)
            });
        }

        // Special analysis for photographer workflow
        AnalyzePhotographyFiles(preview, files);
    }

    /// <summary>
    /// Special analysis for RAW + JPG pairs (photographer workflow)
    /// </summary>
    private void AnalyzePhotographyFiles(PreviewData preview, List<FileItem> files)
    {
        var rawExtensions = new[] { ".nef", ".raw", ".cr2", ".arw", ".dng", ".orf" };
        var jpgExtensions = new[] { ".jpg", ".jpeg" };

        var rawFiles = files.Where(f => rawExtensions.Contains(Path.GetExtension(f.Name).ToLower())).ToList();
        var jpgFiles = files.Where(f => jpgExtensions.Contains(Path.GetExtension(f.Name).ToLower())).ToList();

        if (rawFiles.Any() || jpgFiles.Any())
        {
            preview.IsPhotographyWorkflow = true;
            preview.RawCount = rawFiles.Count;
            preview.JpgCount = jpgFiles.Count;

            // Find matching pairs
            var rawNames = rawFiles.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToHashSet();
            var jpgNames = jpgFiles.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToHashSet();
            
            preview.MatchingPairs = rawNames.Intersect(jpgNames).Count();
            preview.OrphanRaw = rawFiles.Count(f => !jpgNames.Contains(Path.GetFileNameWithoutExtension(f.Name)));
            preview.OrphanJpg = jpgFiles.Count(f => !rawNames.Contains(Path.GetFileNameWithoutExtension(f.Name)));
        }
    }

    /// <summary>
    /// Calculate size statistics
    /// </summary>
    private void CalculateSizes(PreviewData preview, List<FileItem> files)
    {
        if (!files.Any()) return;

        preview.TotalSize = files.Sum(f => f.Size);
        preview.AverageSize = preview.TotalSize / files.Count;
        preview.LargestFile = files.MaxBy(f => f.Size);
        preview.SmallestFile = files.MinBy(f => f.Size);

        // Size distribution
        preview.SizeDistribution = new Dictionary<string, int>
        {
            ["< 1 MB"] = files.Count(f => f.Size < 1_048_576),
            ["1-10 MB"] = files.Count(f => f.Size >= 1_048_576 && f.Size < 10_485_760),
            ["10-100 MB"] = files.Count(f => f.Size >= 10_485_760 && f.Size < 104_857_600),
            ["100 MB-1 GB"] = files.Count(f => f.Size >= 104_857_600 && f.Size < 1_073_741_824),
            ["> 1 GB"] = files.Count(f => f.Size >= 1_073_741_824)
        };
    }

    /// <summary>
    /// Generate thumbnail previews for sample files
    /// </summary>
    private void GenerateThumbnails(PreviewData preview, List<FileItem> sampleFiles)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };

        foreach (var file in sampleFiles.Where(f => imageExtensions.Contains(Path.GetExtension(f.Name).ToLower())))
        {
            try
            {
                if (File.Exists(file.Path))
                {
                    // Create thumbnail (simplified - in real app you'd want proper thumbnail generation)
                    var thumbnail = new ThumbnailInfo
                    {
                        FileName = file.Name,
                        FilePath = file.Path,
                        FileSize = file.Size,
                        HasThumbnail = true
                    };

                    preview.Thumbnails.Add(thumbnail);
                }
            }
            catch
            {
                // Ignore thumbnail generation errors
            }
        }
    }

    /// <summary>
    /// Estimate copy time based on file sizes and typical speeds
    /// </summary>
    private void EstimateCopyTime(PreviewData preview)
    {
        // Estimate based on different scenarios
        var totalGB = preview.TotalSize / (1024.0 * 1024.0 * 1024.0);

        // Conservative estimate (50 MB/s average)
        preview.EstimatedCopyTime = TimeSpan.FromSeconds(totalGB * 1024 / 50);

        // Fast estimate with our optimized engine (150 MB/s average)
        preview.OptimizedCopyTime = TimeSpan.FromSeconds(totalGB * 1024 / 150);

        preview.SpeedupRatio = preview.EstimatedCopyTime.TotalSeconds / preview.OptimizedCopyTime.TotalSeconds;
    }

    /// <summary>
    /// Get appropriate icon for file type
    /// </summary>
    private string GetFileTypeIcon(string extension)
    {
        return extension.ToLower() switch
        {
            ".nef" or ".raw" or ".cr2" or ".arw" or ".dng" or ".orf" => "📷",
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".tiff" => "🖼️",
            ".mp4" or ".mov" or ".avi" or ".mkv" => "🎥",
            ".pdf" => "📄",
            ".txt" or ".md" => "📝",
            ".zip" or ".rar" or ".7z" => "📦",
            ".exe" or ".msi" => "⚙️",
            _ => "📁"
        };
    }

    protected virtual void OnPreviewUpdated(PreviewUpdateEventArgs e)
    {
        PreviewUpdated?.Invoke(this, e);
    }
}

#region Preview Models

public class PreviewData
{
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
    public long AverageSize { get; set; }
    public string SourceFolder { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    public List<FileTypeInfo> FileTypes { get; set; } = new();
    public Dictionary<string, int> SizeDistribution { get; set; } = new();
    public List<ThumbnailInfo> Thumbnails { get; set; } = new();

    public FileItem? LargestFile { get; set; }
    public FileItem? SmallestFile { get; set; }

    // Photography workflow specific
    public bool IsPhotographyWorkflow { get; set; }
    public int RawCount { get; set; }
    public int JpgCount { get; set; }
    public int MatchingPairs { get; set; }
    public int OrphanRaw { get; set; }
    public int OrphanJpg { get; set; }

    // Time estimates
    public TimeSpan EstimatedCopyTime { get; set; }
    public TimeSpan OptimizedCopyTime { get; set; }
    public double SpeedupRatio { get; set; }

    public string FormattedTotalSize => FormatFileSize(TotalSize);
    public string FormattedAverageSize => FormatFileSize(AverageSize);

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
}

public class FileTypeInfo
{
    public string Extension { get; set; } = string.Empty;
    public int Count { get; set; }
    public long TotalSize { get; set; }
    public long AverageSize { get; set; }
    public string Icon { get; set; } = "📁";
    
    public string FormattedTotalSize => FormatFileSize(TotalSize);
    public string FormattedAverageSize => FormatFileSize(AverageSize);
    public double Percentage { get; set; }

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
}

public class ThumbnailInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool HasThumbnail { get; set; }
    public BitmapImage? Thumbnail { get; set; }
}

public class PreviewUpdateEventArgs : EventArgs
{
    public PreviewData Preview { get; set; } = new();
}

#endregion