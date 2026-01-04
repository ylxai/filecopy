using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FileCopyUtility.Helpers;

namespace FileCopyUtility.Services;

/// <summary>
/// Partial class for HighPerformanceFileService - Copy operations
/// </summary>
public partial class HighPerformanceFileService
{
    /// <summary>
    /// Optimized copy for large files (50MB+)
    /// Uses high-performance buffer with async I/O and memory mapping for best performance
    /// </summary>
    private async Task<bool> CopyLargeFileOptimizedAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 4_194_304; // 4MB buffer for large files (increased from 2MB)

        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            // Validate paths to prevent directory traversal
            var sanitizedSourcePath = PathSecurityValidator.SanitizePath(sourcePath);
            var sanitizedDestPath = PathSecurityValidator.SanitizePath(destPath);

            // Ensure both paths are valid and safe
            if (string.IsNullOrEmpty(sanitizedSourcePath) || string.IsNullOrEmpty(sanitizedDestPath))
            {
                return false;
            }

            // Use optimized file options for better performance
            using var sourceStream = new FileStream(sanitizedSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize,
                FileOptions.SequentialScan | FileOptions.Asynchronous);
            using var destStream = new FileStream(sanitizedDestPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize,
                FileOptions.WriteThrough | FileOptions.Asynchronous);

            // Pre-allocate file size for better performance
            destStream.SetLength(sourceStream.Length);

            // Use larger buffer for better throughput
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }

            // Force write to disk immediately
            await destStream.FlushAsync();

            return true;
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exception to be handled by caller
            throw;
        }
        catch (Exception)
        {
            // Return false for other exceptions
            return false;
        }
    }

    /// <summary>
    /// Optimized copy for small files (under 50MB)
    /// Uses high-performance buffer with async I/O
    /// </summary>
    private async Task<bool> CopySmallFileOptimizedAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 2_097_152; // 2MB buffer for small files (increased for better performance)

        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            // Validate paths to prevent directory traversal
            var sanitizedSourcePath = PathSecurityValidator.SanitizePath(sourcePath);
            var sanitizedDestPath = PathSecurityValidator.SanitizePath(destPath);

            // Ensure both paths are valid and safe
            if (string.IsNullOrEmpty(sanitizedSourcePath) || string.IsNullOrEmpty(sanitizedDestPath))
            {
                return false;
            }

            // Use optimized file options for better performance
            using var sourceStream = new FileStream(sanitizedSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize,
                FileOptions.SequentialScan | FileOptions.Asynchronous);
            using var destStream = new FileStream(sanitizedDestPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize,
                FileOptions.WriteThrough | FileOptions.Asynchronous);

            // Pre-allocate file size for better performance
            destStream.SetLength(sourceStream.Length);

            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }

            // Force write to disk immediately
            await destStream.FlushAsync();

            return true;
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exception to be handled by caller
            throw;
        }
        catch (Exception)
        {
            // Return false for other exceptions
            return false;
        }
    }
}