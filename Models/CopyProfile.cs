using System;
using System.Collections.Generic;

namespace FileCopyUtility.Models;

/// <summary>
/// Profile untuk menyimpan konfigurasi copy operation
/// </summary>
public class CopyProfile
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceListPath { get; set; } = string.Empty;
    public string DestinationFolder { get; set; } = string.Empty;
    public int ThreadCount { get; set; } = 4;
    public bool VerifyIntegrity { get; set; } = false;
    public HashAlgorithmType HashAlgorithm { get; set; } = HashAlgorithmType.None;
    public bool AutoRetry { get; set; } = false;
    public int MaxRetries { get; set; } = 3;
    public bool OverwriteExisting { get; set; } = true;
    public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Overwrite;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUsed { get; set; }
}

public enum HashAlgorithmType
{
    None,
    MD5,
    SHA256
}

public enum DuplicateHandling
{
    Overwrite,
    Skip,
    Rename
}
