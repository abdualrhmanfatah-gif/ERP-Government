using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Entities;
using ERP_Government.Domain.BackgroundJobs.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Persistence operations for background job state.
/// </summary>
public class BackgroundJobStore
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<BackgroundJobStore> _logger;

    public BackgroundJobStore(IApplicationDbContext context, ILogger<BackgroundJobStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get job instances that are due for execution (Pending with ScheduledAt in the past, or ReadyForRetry).
    /// </summary>
    public async Task<List<BackgroundJobInstance>> GetDueJobsAsync(int batchSize, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.BackgroundJobInstances
            .Where(i => i.Status == BackgroundJobStatus.Pending
                     && i.ScheduledAt <= now)
            .OrderBy(i => i.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create a new job instance for a scheduled execution.
    /// </summary>
    public async Task<BackgroundJobInstance> CreateInstanceAsync(
        int jobDefinitionId,
        DateTimeOffset scheduledAt,
        string? idempotencyKey,
        string triggeredBy,
        CancellationToken cancellationToken)
    {
        // Idempotency check: if same key exists, return existing
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _context.BackgroundJobInstances
                .FirstOrDefaultAsync(i =>
                    i.JobDefinitionId == jobDefinitionId
                    && i.IdempotencyKey == idempotencyKey
                    && i.Status != BackgroundJobStatus.Failed
                    && i.Status != BackgroundJobStatus.Cancelled,
                    cancellationToken);

            if (existing != null)
            {
                _logger.LogDebug("Idempotent hit: returning existing instance {InstanceId} for key {Key}",
                    existing.Id, idempotencyKey);
                return existing;
            }
        }

        var instance = new BackgroundJobInstance
        {
            JobDefinitionId = jobDefinitionId,
            ScheduledAt = scheduledAt,
            Status = BackgroundJobStatus.Pending,
            IdempotencyKey = idempotencyKey,
            TriggeredBy = triggeredBy,
            RetryCount = 0
        };

        _context.BackgroundJobInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Created job instance {InstanceId} for definition {DefinitionId} at {ScheduledAt}",
            instance.Id, jobDefinitionId, scheduledAt);

        return instance;
    }

    /// <summary>
    /// Transition a job instance to a new status. Enforces valid state transitions.
    /// </summary>
    public async Task UpdateStatusAsync(
        BackgroundJobInstance instance,
        BackgroundJobStatus newStatus,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var validTransition = (instance.Status, newStatus) switch
        {
            (BackgroundJobStatus.Pending, BackgroundJobStatus.Running) => true,
            (BackgroundJobStatus.Running, BackgroundJobStatus.Completed) => true,
            (BackgroundJobStatus.Running, BackgroundJobStatus.Failed) => true,
            (BackgroundJobStatus.Running, BackgroundJobStatus.Cancelled) => true,
            (BackgroundJobStatus.Pending, BackgroundJobStatus.Cancelled) => true,
            _ => false
        };

        if (!validTransition)
        {
            _logger.LogWarning("Invalid state transition from {Old} to {New} for instance {InstanceId}",
                instance.Status, newStatus, instance.Id);
            throw new InvalidOperationException(
                $"Invalid state transition from {instance.Status} to {newStatus} for instance {instance.Id}");
        }

        instance.Status = newStatus;
        instance.ErrorMessage = errorMessage;

        if (newStatus == BackgroundJobStatus.Running)
            instance.StartedAt = DateTimeOffset.UtcNow;
        else if (newStatus is BackgroundJobStatus.Completed or BackgroundJobStatus.Failed or BackgroundJobStatus.Cancelled)
            instance.CompletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Instance {InstanceId} transitioned from {Old} to {New}",
            instance.Id, instance.Status, newStatus);
    }

    /// <summary>
    /// Increment retry count and schedule next retry.
    /// </summary>
    public async Task ScheduleRetryAsync(
        BackgroundJobInstance instance,
        TimeSpan delay,
        CancellationToken cancellationToken)
    {
        instance.RetryCount++;
        instance.Status = BackgroundJobStatus.Pending;
        instance.ScheduledAt = DateTimeOffset.UtcNow.Add(delay);
        instance.ErrorMessage = null;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Scheduled retry for instance {InstanceId} at {NextRun} (attempt {RetryCount})",
            instance.Id, instance.ScheduledAt, instance.RetryCount);
    }

    /// <summary>
    /// Find orphaned Running jobs (started long ago and never completed).
    /// </summary>
    public async Task<List<BackgroundJobInstance>> GetOrphanedRunningJobsAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow - timeout;

        return await _context.BackgroundJobInstances
            .Where(i => i.Status == BackgroundJobStatus.Running
                     && i.StartedAt != null
                     && i.StartedAt < cutoff)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create an execution log entry for a specific attempt.
    /// </summary>
    public async Task<BackgroundJobExecutionLog> CreateExecutionLogAsync(
        int jobInstanceId,
        int attemptNumber,
        CancellationToken cancellationToken)
    {
        var log = new BackgroundJobExecutionLog
        {
            JobInstanceId = jobInstanceId,
            AttemptNumber = attemptNumber,
            StartedAt = DateTimeOffset.UtcNow,
            Status = BackgroundJobExecutionStatus.Running
        };

        _context.BackgroundJobExecutionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return log;
    }

    /// <summary>
    /// Update an execution log entry when attempt completes.
    /// </summary>
    public async Task CompleteExecutionLogAsync(
        BackgroundJobExecutionLog log,
        BackgroundJobExecutionStatus status,
        string? errorMessage = null,
        string? stackTrace = null,
        CancellationToken cancellationToken = default)
    {
        log.Status = status;
        log.CompletedAt = DateTimeOffset.UtcNow;
        log.ErrorMessage = errorMessage;
        log.StackTrace = stackTrace;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Purge old execution logs beyond retention period.
    /// </summary>
    public async Task<int> PurgeOldLogsAsync(int retentionDays, CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var staleLogs = await _context.BackgroundJobExecutionLogs
            .Where(l => l.CompletedAt != null && l.CompletedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (staleLogs.Count == 0) return 0;

        _context.BackgroundJobExecutionLogs.RemoveRange(staleLogs);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purged {Count} old execution logs older than {Days} days",
            staleLogs.Count, retentionDays);

        return staleLogs.Count;
    }
}
