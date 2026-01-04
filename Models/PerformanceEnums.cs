using System;

namespace FileCopyUtility.Models;

/// <summary>
/// Performance related enums and configurations
/// </summary>

public enum PerformanceDuplicateHandling
{
    Overwrite,
    Skip,
    Rename
}

public enum PerformanceHashAlgorithmType
{
    None,
    MD5,
    SHA1,
    SHA256
}

public enum CopyMode
{
    Standard,
    HighPerformance,
    UltraFast
}

public enum OptimizationLevel
{
    Balanced,
    Speed,
    Memory,
    Maximum
}

/// <summary>
/// Performance settings configuration
/// </summary>
public class PerformanceSettings
{
    public CopyMode Mode { get; set; } = CopyMode.UltraFast;
    public OptimizationLevel Optimization { get; set; } = OptimizationLevel.Speed;
    public int MaxParallelism { get; set; } = Environment.ProcessorCount * 2;
    public int BufferSize { get; set; } = 1_048_576; // 1MB default
    public bool UseMemoryMapping { get; set; } = true;
    public bool UseDirectIO { get; set; } = true;
    public bool PreAllocateFiles { get; set; } = true;
    public bool FlushToDisk { get; set; } = true;
    
    /// <summary>
    /// Auto-configure settings based on system capabilities
    /// </summary>
    public static PerformanceSettings AutoConfigure()
    {
        var settings = new PerformanceSettings();
        
        // Configure based on processor count
        if (Environment.ProcessorCount >= 8)
        {
            settings.MaxParallelism = Environment.ProcessorCount * 3;
            settings.BufferSize = 2_097_152; // 2MB
        }
        else if (Environment.ProcessorCount >= 4)
        {
            settings.MaxParallelism = Environment.ProcessorCount * 2;
            settings.BufferSize = 1_048_576; // 1MB
        }
        else
        {
            settings.MaxParallelism = Environment.ProcessorCount;
            settings.BufferSize = 524_288; // 512KB
        }
        
        // Configure based on available memory
        var totalMemory = GC.GetTotalMemory(false);
        if (totalMemory > 8_000_000_000) // 8GB+
        {
            settings.UseMemoryMapping = true;
            settings.BufferSize = Math.Max(settings.BufferSize, 4_194_304); // 4MB minimum
        }
        
        return settings;
    }
}