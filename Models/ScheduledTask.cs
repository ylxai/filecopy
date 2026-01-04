using System;

namespace FileCopyUtility.Models;

/// <summary>
/// Scheduled task untuk automatic copy operations
/// </summary>
public class ScheduledTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Daily;
    public TimeSpan ExecutionTime { get; set; } = TimeSpan.FromHours(2); // 2 AM default
    public int IntervalHours { get; set; } = 24;
    public DayOfWeek[] SelectedDays { get; set; } = Array.Empty<DayOfWeek>();
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public enum ScheduleType
{
    Once,
    Hourly,
    Daily,
    Weekly,
    Custom
}
