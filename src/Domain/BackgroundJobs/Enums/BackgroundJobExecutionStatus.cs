namespace ERP_Government.Domain.BackgroundJobs.Enums;

/// <summary>
/// Per-attempt execution status (different from instance status).
/// </summary>
public enum BackgroundJobExecutionStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2
}
