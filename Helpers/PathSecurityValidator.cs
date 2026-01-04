using System;
using System.IO;

namespace FileCopyUtility.Helpers
{
    /// <summary>
    /// Helper class for path security validation to prevent path traversal attacks
    /// </summary>
    public static class PathSecurityValidator
    {
        /// <summary>
        /// Validates if the target path is within the allowed base directory
        /// </summary>
        public static bool IsPathWithinBaseDirectory(string basePath, string targetPath)
        {
            try
            {
                var baseDir = new DirectoryInfo(basePath).FullName;
                var targetDir = new FileInfo(targetPath).DirectoryName;

                if (targetDir == null) return false;

                var targetInfo = new DirectoryInfo(targetDir).FullName;
                
                // Check if target directory starts with base directory
                return targetInfo.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sanitizes a path to prevent path traversal attempts
        /// </summary>
        public static string SanitizePath(string inputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
                return string.Empty;

            // Normalize the path (resolve .. and .)
            string normalizedPath;
            try
            {
                normalizedPath = Path.GetFullPath(inputPath);
            }
            catch
            {
                // If GetFullPath fails, return a safe empty string or original path
                return string.Empty;
            }

            // Check for suspicious patterns
            if (normalizedPath.Contains("..") || normalizedPath.Contains("%2e%2e") || normalizedPath.Contains("..%2f"))
            {
                // Remove dangerous patterns
                normalizedPath = normalizedPath.Replace("..", "").Replace("%2e%2e", "").Replace("..%2f", "");
            }

            return normalizedPath;
        }

        /// <summary>
        /// Validates if a file path is safe to use (prevents directory traversal)
        /// </summary>
        public static bool IsSafeFilePath(string basePath, string filePath)
        {
            try
            {
                // Sanitize the path first
                var sanitizedPath = SanitizePath(filePath);
                
                if (string.IsNullOrEmpty(sanitizedPath))
                    return false;

                // Check if the sanitized path is within the base directory
                return IsPathWithinBaseDirectory(basePath, sanitizedPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a file name is safe (doesn't contain path traversal sequences)
        /// </summary>
        public static bool IsSafeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            // Check for path traversal attempts
            if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            {
                return false;
            }

            // Check for invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in fileName)
            {
                if (invalidChars.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}