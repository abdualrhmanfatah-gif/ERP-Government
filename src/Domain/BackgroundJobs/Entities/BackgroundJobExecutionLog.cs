using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.BackgroundJobs.Entities;

/// <summary>
/// Per-attempt audit trail. Multiple rows per BackgroundJobInstance (one per retry attempt).
/// </summary>
public class BackgroundJobExecutionLog : BaseAuditableEntity
{
    public int JobInstanceId { get; set; }

    /// <summary>1-based attempt number.</summary>
    public int AttemptNumber { get; set; }

    /// <summary>When this attempt started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>When this attempt finished.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Status of this specific attempt.</summary>
    public BackgroundJobExecutionStatus Status { get; set; } = BackgroundJobExecutionStatus.Running;

    /// <summary>Error message on this attempt.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Stack trace on failure.</summary>
    public string? StackTrace { get; set; }

    public byte[] RowVersion { get; set; } = [];

    /// <summary>Navigation property.</summary>
    public BackgroundJobInstance JobInstance { get; set; } = null!;
}
