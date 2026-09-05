using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.BackgroundJobs.Entities;

/// <summary>
/// A specific scheduled execution of a job. One row per scheduled run.
/// </summary>
public class BackgroundJobInstance : BaseAuditableEntity
{
    public int JobDefinitionId { get; set; }

    /// <summary>When this run was scheduled for.</summary>
    public DateTimeOffset ScheduledAt { get; set; }

    /// <summary>When execution actually started.</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>When execution finished.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Current status of this instance.</summary>
    public BackgroundJobStatus Status { get; set; } = BackgroundJobStatus.Pending;

    /// <summary>Number of retry attempts so far.</summary>
    public int RetryCount { get; set; }

    /// <summary>Deterministic key for idempotency (nullable).</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Error details on failure.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Who/what triggered this execution: "Scheduler", "Manual", "Startup".</summary>
    public string TriggeredBy { get; set; } = "Scheduler";

    public byte[] RowVersion { get; set; } = [];

    /// <summary>Navigation property.</summary>
    public BackgroundJobDefinition JobDefinition { get; set; } = null!;

    /// <summary>Navigation property.</summary>
    public ICollection<BackgroundJobExecutionLog> ExecutionLogs { get; set; } = [];
}
