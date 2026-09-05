using ERP_Government.Domain.BackgroundJobs.Enums;

namespace ERP_Government.Domain.BackgroundJobs.Common;

/// <summary>
/// Schedule configuration for a background job.
/// </summary>
public record BackgroundJobSchedule
{
    public BackgroundJobScheduleType ScheduleType { get; init; }

    /// <summary>
    /// For Interval: seconds between executions.
    /// For Cron: cron expression (e.g. "0 0 * * *").
    /// </summary>
    public string ScheduleValue { get; init; } = string.Empty;

    public static BackgroundJobSchedule Interval(int seconds) => new()
    {
        ScheduleType = BackgroundJobScheduleType.Interval,
        ScheduleValue = seconds.ToString()
    };

    public static BackgroundJobSchedule Cron(string expression) => new()
    {
        ScheduleType = BackgroundJobScheduleType.Cron,
        ScheduleValue = expression
    };

    public TimeSpan GetInterval()
    {
        return ScheduleType switch
        {
            BackgroundJobScheduleType.Interval => TimeSpan.FromSeconds(int.Parse(ScheduleValue)),
            _ => throw new InvalidOperationException("GetInterval() only valid for Interval schedule type")
        };
    }
}
