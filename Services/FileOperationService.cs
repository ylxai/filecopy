using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk handle operasi file (validation, copy, dll)
/// </summary>
public partial class FileOperationService
{
    // This partial class contains only the declaration
    // All functionality has been moved to partial classes:
    // - FileOperationService.Core.cs (core functionality)
    // - FileOperationService.Parallel.cs (parallel functionality)
    // - FileOperationService.Helpers.cs (file handling helpers)
}

#region Models

public class FileItem
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class InvalidFileItem
{
    public string Path { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class FailedFileItem
{
    public FileItem File { get; set; } = new();
    public string Error { get; set; } = string.Empty;
}

public class ValidationResult
{
    public List<FileItem> ValidFiles { get; set; } = new();
    public List<InvalidFileItem> InvalidFiles { get; set; } = new();
    public int DuplicatesRemoved { get; set; }

    public long TotalSize => ValidFiles.Sum(f => f.Size);
}

public class CopyResult
{
    public List<FileItem> SuccessfulFiles { get; set; } = new();
    public List<FailedFileItem> FailedFiles { get; set; } = new();
    public bool Cancelled { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }

    public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;
}

public class ProgressEventArgs : EventArgs
{
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public string CurrentFileName { get; set; } = string.Empty;
    public int CurrentFileProgress { get; set; }
    public long BytesCopied { get; set; }
    public long TotalBytes { get; set; }
    public double CopySpeedMBps { get; set; }

    public int TotalProgress => TotalFiles > 0 ? (ProcessedFiles * 100) / TotalFiles : 0;
}

/// <summary>
/// Pause token untuk Pause/Resume functionality
/// </summary>
public class PauseTokenSource
{
    private TaskCompletionSource<bool> _paused = new();
    private bool _isPaused;

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (_isPaused == value) return;

            _isPaused = value;
            if (!_isPaused)
            {
                _paused.TrySetResult(true);
                _paused = new TaskCompletionSource<bool>();
            }
        }
    }

    public async Task WaitWhilePausedAsync()
    {
        if (_isPaused)
        {
            await _paused.Task;
        }
    }
}

#endregion