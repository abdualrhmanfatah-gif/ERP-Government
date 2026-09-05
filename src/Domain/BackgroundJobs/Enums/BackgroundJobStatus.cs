namespace ERP_Government.Domain.BackgroundJobs.Enums;

/// <summary>
/// Background job instance status.
/// </summary>
public enum BackgroundJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
