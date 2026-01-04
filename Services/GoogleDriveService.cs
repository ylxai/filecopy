using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileCopyUtility.Services
{
    public class GoogleDriveService
    {
        private static string[] Scopes = { DriveService.Scope.DriveFile };
        private static string ApplicationName = "FileCopy Utility Gallery Generator";
        private DriveService? _service;

        public bool IsAuthenticated => _service != null;

        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                UserCredential credential;
                string credPath = "token.json";
                string secretsPath = "credentials.json";

                if (!File.Exists(secretsPath))
                {
                    throw new FileNotFoundException("credentials.json not found. Please place it in the application directory.");
                }

                using (var stream = new FileStream(secretsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                _service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> FindFolderAsync(string folderName, string? parentId = null)
        {
            if (_service == null) throw new InvalidOperationException("Not authenticated");

            string query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
            if (!string.IsNullOrEmpty(parentId))
            {
                query += $" and '{parentId}' in parents";
            }

            var request = _service.Files.List();
            request.Q = query;
            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var result = await request.ExecuteAsync();
            return result.Files.FirstOrDefault()?.Id;
        }

        public async Task<string> CreateFolderAsync(string folderName, string? parentId = null)
        {
            if (_service == null) throw new InvalidOperationException("Not authenticated");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            if (!string.IsNullOrEmpty(parentId))
            {
                fileMetadata.Parents = new List<string> { parentId };
            }

            var request = _service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = await request.ExecuteAsync();
            return file.Id;
        }

        public async Task<string> UploadFileAsync(string filePath, string parentId, string? displayName = null)
        {
            if (_service == null) throw new InvalidOperationException("Not authenticated");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = displayName ?? Path.GetFileName(filePath),
                Parents = new List<string> { parentId }
            };

            string mimeType = GetMimeType(filePath);

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var request = _service.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";
                var progress = await request.UploadAsync();

                if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    throw new Exception($"Upload failed: {progress.Exception.Message}");
                }

                var file = request.ResponseBody;
                return file.Id;
            }
        }

        public async Task MakeFolderPublicAsync(string fileId)
        {
            if (_service == null) throw new InvalidOperationException("Not authenticated");

            var permission = new Google.Apis.Drive.v3.Data.Permission()
            {
                Type = "anyone",
                Role = "reader"
            };

            var request = _service.Permissions.Create(permission, fileId);
            await request.ExecuteAsync();
        }

        private string GetMimeType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }
}
