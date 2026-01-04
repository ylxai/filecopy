using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk manage copy profiles
/// </summary>
public class ProfileService
{
    private readonly string _profilesFolder;
    private readonly string _profilesFile;

    public ProfileService()
    {
        _profilesFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileCopyUtility",
            "Profiles");
        
        _profilesFile = Path.Combine(_profilesFolder, "profiles.json");
        
        Directory.CreateDirectory(_profilesFolder);
    }

    /// <summary>
    /// Get all saved profiles
    /// </summary>
    public async Task<List<CopyProfile>> GetAllProfilesAsync()
    {
        if (!File.Exists(_profilesFile))
        {
            return new List<CopyProfile>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_profilesFile);
            var profiles = JsonSerializer.Deserialize<List<CopyProfile>>(json);
            return profiles ?? new List<CopyProfile>();
        }
        catch
        {
            return new List<CopyProfile>();
        }
    }

    /// <summary>
    /// Save profile
    /// </summary>
    public async Task SaveProfileAsync(CopyProfile profile)
    {
        var profiles = await GetAllProfilesAsync();
        
        // Update existing or add new
        var existingIndex = profiles.FindIndex(p => p.Name == profile.Name);
        if (existingIndex >= 0)
        {
            profiles[existingIndex] = profile;
        }
        else
        {
            profiles.Add(profile);
        }

        var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_profilesFile, json);
    }

    /// <summary>
    /// Delete profile
    /// </summary>
    public async Task DeleteProfileAsync(string profileName)
    {
        var profiles = await GetAllProfilesAsync();
        profiles.RemoveAll(p => p.Name == profileName);
        
        var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_profilesFile, json);
    }

    /// <summary>
    /// Get specific profile
    /// </summary>
    public async Task<CopyProfile?> GetProfileAsync(string profileName)
    {
        var profiles = await GetAllProfilesAsync();
        return profiles.Find(p => p.Name == profileName);
    }

    /// <summary>
    /// Update last used timestamp
    /// </summary>
    public async Task UpdateLastUsedAsync(string profileName)
    {
        var profile = await GetProfileAsync(profileName);
        if (profile != null)
        {
            profile.LastUsed = DateTime.Now;
            await SaveProfileAsync(profile);
        }
    }
}
