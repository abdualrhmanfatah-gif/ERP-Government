namespace ERP_Government.Domain.BackgroundJobs.Common;

/// <summary>
/// Interface that background job types implement to define their execution logic, schedule, and retry policy.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Execute the job logic.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get the schedule for this job type.
    /// </summary>
    BackgroundJobSchedule GetSchedule();

    /// <summary>
    /// Get the retry policy for this job type.
    /// </summary>
    BackgroundJobRetryPolicy GetRetryPolicy() => new();

    /// <summary>
    /// Get a deterministic idempotency key for the current execution.
    /// Return null if idempotency is not needed for this execution.
    /// </summary>
    string? GetIdempotencyKey() => null;

    /// <summary>
    /// Display name for this job type.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Maximum execution timeout in seconds.
    /// </summary>
    int TimeoutSeconds => 300;
}
