using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileCopyUtility.Models;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk scheduled copy operations
/// </summary>
public class SchedulerService
{
    private readonly string _schedulesFolder;
    private readonly string _schedulesFile;
    private readonly ProfileService _profileService;
    private readonly FileOperationService _fileService;
    private System.Timers.Timer? _checkTimer;
    private bool _isRunning;

    public event EventHandler<string>? StatusChanged;

    public SchedulerService(ProfileService profileService, FileOperationService fileService)
    {
        _profileService = profileService;
        _fileService = fileService;
        
        _schedulesFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileCopyUtility",
            "Schedules");
        
        _schedulesFile = Path.Combine(_schedulesFolder, "schedules.json");
        
        Directory.CreateDirectory(_schedulesFolder);
    }

    /// <summary>
    /// Start scheduler service
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _checkTimer = new System.Timers.Timer(60000); // Check every minute
        _checkTimer.Elapsed += async (s, e) => await CheckAndExecuteSchedulesAsync();
        _checkTimer.Start();

        OnStatusChanged("✅ Scheduler started");
    }

    /// <summary>
    /// Stop scheduler service
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _checkTimer?.Stop();
        _checkTimer?.Dispose();
        OnStatusChanged("⏹️ Scheduler stopped");
    }

    /// <summary>
    /// Get all scheduled tasks
    /// </summary>
    public async Task<List<ScheduledTask>> GetAllSchedulesAsync()
    {
        if (!File.Exists(_schedulesFile))
        {
            return new List<ScheduledTask>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_schedulesFile);
            var schedules = JsonSerializer.Deserialize<List<ScheduledTask>>(json);
            return schedules ?? new List<ScheduledTask>();
        }
        catch
        {
            return new List<ScheduledTask>();
        }
    }

    /// <summary>
    /// Save schedule
    /// </summary>
    public async Task SaveScheduleAsync(ScheduledTask task)
    {
        var schedules = await GetAllSchedulesAsync();
        
        var existingIndex = schedules.FindIndex(s => s.Id == task.Id);
        if (existingIndex >= 0)
        {
            schedules[existingIndex] = task;
        }
        else
        {
            schedules.Add(task);
        }

        // Calculate next run
        task.NextRun = CalculateNextRun(task);

        var json = JsonSerializer.Serialize(schedules, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_schedulesFile, json);
    }

    /// <summary>
    /// Delete schedule
    /// </summary>
    public async Task DeleteScheduleAsync(string scheduleId)
    {
        var schedules = await GetAllSchedulesAsync();
        schedules.RemoveAll(s => s.Id == scheduleId);
        
        var json = JsonSerializer.Serialize(schedules, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_schedulesFile, json);
    }

    /// <summary>
    /// Check and execute due schedules
    /// </summary>
    private async Task CheckAndExecuteSchedulesAsync()
    {
        if (!_isRunning) return;

        var schedules = await GetAllSchedulesAsync();
        var now = DateTime.Now;

        foreach (var schedule in schedules.Where(s => s.IsEnabled))
        {
            if (schedule.NextRun.HasValue && schedule.NextRun.Value <= now)
            {
                await ExecuteScheduledTaskAsync(schedule);
            }
        }
    }

    /// <summary>
    /// Execute scheduled task
    /// </summary>
    private async Task ExecuteScheduledTaskAsync(ScheduledTask task)
    {
        try
        {
            OnStatusChanged($"⏰ Executing scheduled task: {task.Name}");

            // Load profile
            var profile = await _profileService.GetProfileAsync(task.ProfileName);
            if (profile == null)
            {
                OnStatusChanged($"❌ Profile not found: {task.ProfileName}");
                task.FailureCount++;
                return;
            }

            // Load files from source list
            if (!File.Exists(profile.SourceListPath))
            {
                OnStatusChanged($"❌ Source list not found: {profile.SourceListPath}");
                task.FailureCount++;
                return;
            }

            var lines = await File.ReadAllLinesAsync(profile.SourceListPath);
            var fileItems = new List<FileItem>();

            foreach (var line in lines)
            {
                if (File.Exists(line))
                {
                    var fileInfo = new FileInfo(line);
                    fileItems.Add(new FileItem
                    {
                        Path = line,
                        Name = fileInfo.Name,
                        Size = fileInfo.Length
                    });
                }
            }

            // Execute copy
            var result = await _fileService.CopyFilesParallelAsync(
                fileItems,
                profile.DestinationFolder,
                profile.ThreadCount,
                CancellationToken.None,
                null);

            // Update task status
            task.LastRun = DateTime.Now;
            task.NextRun = CalculateNextRun(task);
            
            if (result.FailedFiles.Any())
            {
                task.FailureCount++;
                OnStatusChanged($"⚠️ Task completed with errors: {result.FailedFiles.Count} failures");
            }
            else
            {
                task.SuccessCount++;
                OnStatusChanged($"✅ Task completed successfully: {result.SuccessfulFiles.Count} files copied");
            }

            await SaveScheduleAsync(task);
        }
        catch (Exception ex)
        {
            OnStatusChanged($"❌ Error executing task: {ex.Message}");
            task.FailureCount++;
            await SaveScheduleAsync(task);
        }
    }

    /// <summary>
    /// Calculate next run time based on schedule type
    /// </summary>
    private DateTime CalculateNextRun(ScheduledTask task)
    {
        var now = DateTime.Now;

        return task.ScheduleType switch
        {
            ScheduleType.Once => now.AddYears(100), // Far future (won't run again)
            ScheduleType.Hourly => now.AddHours(task.IntervalHours),
            ScheduleType.Daily => now.Date.AddDays(1).Add(task.ExecutionTime),
            ScheduleType.Weekly => CalculateNextWeeklyRun(task, now),
            ScheduleType.Custom => now.AddHours(task.IntervalHours),
            _ => now.AddDays(1)
        };
    }

    private DateTime CalculateNextWeeklyRun(ScheduledTask task, DateTime now)
    {
        if (task.SelectedDays == null || task.SelectedDays.Length == 0)
        {
            return now.AddDays(7);
        }

        var nextRun = now.Date.Add(task.ExecutionTime);
        
        for (int i = 0; i < 7; i++)
        {
            nextRun = nextRun.AddDays(1);
            if (task.SelectedDays.Contains(nextRun.DayOfWeek) && nextRun > now)
            {
                return nextRun;
            }
        }

        return now.AddDays(7);
    }

    protected virtual void OnStatusChanged(string status)
    {
        StatusChanged?.Invoke(this, status);
    }
}
