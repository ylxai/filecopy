using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileCopyUtility.Services
{
    public class GalleryHistoryItem
    {
        public string FolderId { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string SourcePath { get; set; } = string.Empty;
    }

    public class GalleryHistoryService
    {
        private readonly string _historyFilePath;
        private List<GalleryHistoryItem> _history;

        public GalleryHistoryService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "FileCopyUtility");
            Directory.CreateDirectory(appFolder);
            _historyFilePath = Path.Combine(appFolder, "gallery_history.json");
            _history = new List<GalleryHistoryItem>();
        }

        public async Task LoadHistoryAsync()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string json = await File.ReadAllTextAsync(_historyFilePath);
                    _history = JsonSerializer.Deserialize<List<GalleryHistoryItem>>(json) ?? new List<GalleryHistoryItem>();
                    
                    // Sort by date descending
                    _history = _history.OrderByDescending(x => x.CreatedAt).ToList();
                }
            }
            catch (Exception)
            {
                // Ignore errors, start with empty history
                _history = new List<GalleryHistoryItem>();
            }
        }

        public async Task AddItemAsync(GalleryHistoryItem item)
        {
            _history.Insert(0, item);
            await SaveHistoryAsync();
        }

        public List<GalleryHistoryItem> GetHistory()
        {
            return _history;
        }

        public async Task DeleteItemAsync(string folderId)
        {
            var item = _history.FirstOrDefault(x => x.FolderId == folderId);
            if (item != null)
            {
                _history.Remove(item);
                await SaveHistoryAsync();
            }
        }

        private async Task SaveHistoryAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_historyFilePath, json);
            }
            catch (Exception)
            {
                // Handle save error if needed
            }
        }
    }
}
