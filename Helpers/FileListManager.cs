using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileCopyUtility.Helpers
{
    /// <summary>
    /// Helper class for managing large file lists efficiently to prevent memory leaks
    /// </summary>
    public class FileListManager
    {
        private readonly string _tempFilePath;
        private readonly object _lockObject = new();

        public FileListManager()
        {
            _tempFilePath = Path.GetTempFileName();
        }

        public void AddFile(string filePath)
        {
            lock (_lockObject)
            {
                File.AppendAllLines(_tempFilePath, new[] { filePath });
            }
        }

        public void AddFiles(IEnumerable<string> filePaths)
        {
            lock (_lockObject)
            {
                File.AppendAllLines(_tempFilePath, filePaths);
            }
        }

        public List<string> GetAllFiles()
        {
            lock (_lockObject)
            {
                if (!File.Exists(_tempFilePath))
                    return new List<string>();
                    
                return File.ReadAllLines(_tempFilePath).ToList();
            }
        }

        public bool Any()
        {
            lock (_lockObject)
            {
                if (!File.Exists(_tempFilePath))
                    return false;
                    
                using var fs = new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return fs.Length > 0;
            }
        }

        public int Count()
        {
            lock (_lockObject)
            {
                if (!File.Exists(_tempFilePath))
                    return 0;
                    
                return File.ReadAllLines(_tempFilePath).Length;
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }
            }
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }
            }
        }

        public string GetTempFilePath() => _tempFilePath;

        public List<string> GetPagedFiles(int startIndex, int count)
        {
            lock (_lockObject)
            {
                if (!File.Exists(_tempFilePath))
                    return new List<string>();
                    
                var allLines = File.ReadAllLines(_tempFilePath);
                var result = new List<string>();
                
                for (int i = startIndex; i < Math.Min(startIndex + count, allLines.Length); i++)
                {
                    result.Add(allLines[i]);
                }
                
                return result;
            }
        }
    }
}