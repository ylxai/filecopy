using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for FileOperationService - Core functionality
/// </summary>
public partial class FileOperationService
{
    public event EventHandler<ProgressEventArgs>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// Validate file list dan return valid/invalid files
    /// </summary>
    public ValidationResult ValidateFiles(List<string> filePaths)
    {
        var result = new ValidationResult();

        // Remove duplicates
        var uniquePaths = filePaths.Distinct().ToList();
        result.DuplicatesRemoved = filePaths.Count - uniquePaths.Count;

        foreach (var path in uniquePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            if (File.Exists(path))
            {
                try
                {
                    var fileInfo = new FileInfo(path);
                    result.ValidFiles.Add(new FileItem
                    {
                        Path = path,
                        Name = fileInfo.Name,
                        Size = fileInfo.Length,
                        CreatedDate = fileInfo.CreationTime,
                        ModifiedDate = fileInfo.LastWriteTime
                    });
                }
                catch (Exception ex)
                {
                    result.InvalidFiles.Add(new InvalidFileItem
                    {
                        Path = path,
                        Reason = $"Cannot access file: {ex.Message}"
                    });
                }
            }
            else
            {
                result.InvalidFiles.Add(new InvalidFileItem
                {
                    Path = path,
                    Reason = "File not found"
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Copy files dengan progress tracking (Sequential - v1.0)
    /// </summary>
    public async Task<CopyResult> CopyFilesAsync(
        List<FileItem> files,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        var result = new CopyResult();
        result.StartTime = DateTime.Now;
        int totalFiles = files.Count;
        int processedFiles = 0;

        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                result.Cancelled = true;
                break;
            }

            var destPath = Path.Combine(destinationFolder, file.Name);

            try
            {
                OnStatusChanged($"Copying: {file.Name}");

                await CopyFileWithProgressAsync(
                    file.Path,
                    destPath,
                    (progress) =>
                    {
                        OnProgressChanged(new ProgressEventArgs
                        {
                            TotalFiles = totalFiles,
                            ProcessedFiles = processedFiles,
                            CurrentFileName = file.Name,
                            CurrentFileProgress = progress
                        });
                    },
                    cancellationToken);

                result.SuccessfulFiles.Add(file);
                OnStatusChanged($"✅ {file.Name}");
            }
            catch (Exception ex)
            {
                result.FailedFiles.Add(new FailedFileItem
                {
                    File = file,
                    Error = ex.Message
                });
                OnStatusChanged($"❌ {file.Name}: {ex.Message}");
            }

            processedFiles++;

            // Update total progress
            OnProgressChanged(new ProgressEventArgs
            {
                TotalFiles = totalFiles,
                ProcessedFiles = processedFiles,
                CurrentFileName = file.Name,
                CurrentFileProgress = 100
            });
        }

        result.EndTime = DateTime.Now;
        return result;
    }

    protected virtual void OnProgressChanged(ProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }

    protected virtual void OnStatusChanged(string status)
    {
        StatusChanged?.Invoke(this, status);
    }
}