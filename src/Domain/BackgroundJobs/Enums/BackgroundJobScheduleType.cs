namespace ERP_Government.Domain.BackgroundJobs.Enums;

/// <summary>
/// Background job schedule type.
/// </summary>
public enum BackgroundJobScheduleType
{
    /// <summary>Interval in seconds.</summary>
    Interval = 0,

    /// <summary>Cron expression.</summary>
    Cron = 1
}
