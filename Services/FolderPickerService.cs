using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace FileCopyUtility.Services
{
    public class FolderPickerService
    {
        /// <summary>
        /// Shows a modern folder picker dialog with improved UX
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialPath">Initial folder path (optional)</param>
        /// <returns>Selected folder path or null if cancelled</returns>
        public static string? ShowFolderDialog(string title = "Select Folder", string? initialPath = null)
        {
            try
            {
                // Use Windows Vista+ folder browser dialog for better UX
                using var dialog = new FolderBrowserDialog();
                
                dialog.Description = title;
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = true;
                
                // Set initial path if provided
                if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
                {
                    dialog.SelectedPath = initialPath;
                }
                else
                {
                    // Default to user's Documents folder
                    dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                // Show dialog and return result
                var result = dialog.ShowDialog();
                return result == DialogResult.OK ? dialog.SelectedPath : null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error opening folder dialog: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return null;
            }
        }

        /// <summary>
        /// Shows folder dialog specifically for source folder selection
        /// </summary>
        public static string? ShowSourceFolderDialog(string? currentPath = null)
        {
            return ShowFolderDialog(
                "📁 Select Source Folder (where your photos are stored)", 
                currentPath);
        }

        /// <summary>
        /// Shows folder dialog specifically for destination folder selection  
        /// </summary>
        public static string? ShowDestinationFolderDialog(string? currentPath = null)
        {
            return ShowFolderDialog(
                "📂 Select Destination Folder (where files will be copied)", 
                currentPath);
        }

        /// <summary>
        /// Shows folder dialog for scanning/importing files
        /// </summary>
        public static string? ShowScanFolderDialog(string? currentPath = null)
        {
            return ShowFolderDialog(
                "🔍 Select Folder to Scan (includes all subfolders)", 
                currentPath);
        }

        /// <summary>
        /// Validates if the selected folder is accessible and contains files
        /// </summary>
        public static (bool isValid, string message, int fileCount) ValidateFolder(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                    return (false, "No folder selected", 0);

                if (!Directory.Exists(folderPath))
                    return (false, "Folder does not exist", 0);

                // Check if folder is accessible
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                var photoExtensions = new[] { ".nef", ".jpg", ".jpeg", ".png", ".raw", ".cr2", ".arw", ".dng", ".raf", ".orf" };
                var photoCount = 0;

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file).ToLower();
                    if (Array.Exists(photoExtensions, e => e == ext))
                        photoCount++;
                }

                if (files.Length == 0)
                    return (true, "Folder is empty", 0);

                return (true, $"Folder contains {files.Length} files ({photoCount} photos)", files.Length);
            }
            catch (UnauthorizedAccessException)
            {
                return (false, "Access denied to this folder", 0);
            }
            catch (Exception ex)
            {
                return (false, $"Error accessing folder: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Gets a user-friendly display name for a folder path
        /// </summary>
        public static string GetFolderDisplayName(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return "No folder selected";

            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                var parentName = dirInfo.Parent?.Name ?? "";
                
                // Show parent folder name for context
                return string.IsNullOrEmpty(parentName) 
                    ? dirInfo.Name 
                    : $"{parentName}\\{dirInfo.Name}";
            }
            catch
            {
                return Path.GetFileName(folderPath) ?? folderPath;
            }
        }

        /// <summary>
        /// Validates if a path is a valid folder for drag-drop operations
        /// </summary>
        public static bool IsValidDroppedFolder(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets recent folder suggestions based on common photo locations
        /// </summary>
        public static List<string> GetRecentFolderSuggestions()
        {
            var suggestions = new List<string>();
            
            try
            {
                // Common photo locations
                var commonPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    "C:\\Users\\Public\\Pictures"
                };

                foreach (var path in commonPaths)
                {
                    if (Directory.Exists(path))
                    {
                        suggestions.Add(path);
                    }
                }
            }
            catch
            {
                // Ignore errors when building suggestions
            }

            return suggestions;
        }

        /// <summary>
        /// Gets a detailed folder summary for tooltip or status display
        /// </summary>
        public static string GetFolderSummary(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return "No folder selected";

            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                if (!dirInfo.Exists)
                    return "Folder does not exist";

                var files = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                var subDirs = dirInfo.GetDirectories();
                var photoExtensions = new[] { ".nef", ".jpg", ".jpeg", ".png", ".raw", ".cr2", ".arw", ".dng", ".raf", ".orf" };
                var photoCount = files.Count(f => photoExtensions.Contains(f.Extension.ToLower()));

                return $"📁 {GetFolderDisplayName(folderPath)}\n" +
                       $"📄 {files.Length} files, 📂 {subDirs.Length} folders\n" +
                       $"📸 {photoCount} photo files";
            }
            catch
            {
                return "Unable to access folder information";
            }
        }
    }
}