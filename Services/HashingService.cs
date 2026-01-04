using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk file integrity checking dengan hashing
/// </summary>
public class HashingService
{
    /// <summary>
    /// Calculate hash untuk file
    /// </summary>
    public async Task<string> ComputeFileHashAsync(string filePath, HashAlgorithmType algorithmType)
    {
        if (algorithmType == HashAlgorithmType.None)
        {
            return string.Empty;
        }

        using var stream = File.OpenRead(filePath);
        
        byte[] hash = algorithmType switch
        {
            HashAlgorithmType.MD5 => await ComputeMD5Async(stream),
            HashAlgorithmType.SHA256 => await ComputeSHA256Async(stream),
            _ => Array.Empty<byte>()
        };

        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Verify file integrity by comparing hashes
    /// </summary>
    public async Task<bool> VerifyFileIntegrityAsync(
        string sourceFile, 
        string destFile, 
        HashAlgorithmType algorithmType)
    {
        if (algorithmType == HashAlgorithmType.None)
        {
            return true; // Skip verification
        }

        if (!File.Exists(sourceFile) || !File.Exists(destFile))
        {
            return false;
        }

        var sourceHash = await ComputeFileHashAsync(sourceFile, algorithmType);
        var destHash = await ComputeFileHashAsync(destFile, algorithmType);

        return sourceHash.Equals(destHash, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<byte[]> ComputeMD5Async(Stream stream)
    {
        using var md5 = MD5.Create();
        return await Task.Run(() => md5.ComputeHash(stream));
    }

    private async Task<byte[]> ComputeSHA256Async(Stream stream)
    {
        using var sha256 = SHA256.Create();
        return await Task.Run(() => sha256.ComputeHash(stream));
    }

    /// <summary>
    /// Get hash algorithm name for display
    /// </summary>
    public static string GetAlgorithmName(HashAlgorithmType type)
    {
        return type switch
        {
            HashAlgorithmType.MD5 => "MD5",
            HashAlgorithmType.SHA256 => "SHA-256",
            _ => "None"
        };
    }
}
