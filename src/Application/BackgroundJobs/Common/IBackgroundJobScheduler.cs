using ERP_Government.Domain.BackgroundJobs.Enums;

namespace ERP_Government.Application.BackgroundJobs.Common;

/// <summary>
/// Scheduler abstraction for background jobs.
/// </summary>
public interface IBackgroundJobScheduler
{
    /// <summary>
    /// Schedule a new job instance for execution.
    /// </summary>
    Task<int> ScheduleAsync(
        int jobDefinitionId,
        DateTimeOffset scheduledAt,
        string? idempotencyKey = null,
        string triggeredBy = "Scheduler",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a pending or running job instance.
    /// </summary>
    Task CancelAsync(int instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current status of a job instance.
    /// </summary>
    Task<BackgroundJobStatus?> GetStatusAsync(int instanceId, CancellationToken cancellationToken = default);
}
