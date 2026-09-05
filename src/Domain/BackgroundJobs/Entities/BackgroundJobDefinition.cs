using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.BackgroundJobs.Entities;

/// <summary>
/// Registered job type metadata. One row per job type. Seeded at startup from DI-registered IBackgroundJob implementations.
/// </summary>
public class BackgroundJobDefinition : BaseAuditableEntity
{
    /// <summary>CLR full type name (unique).</summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Schedule type (Interval or Cron).</summary>
    public BackgroundJobScheduleType ScheduleType { get; set; }

    /// <summary>Schedule value — seconds for Interval, cron expression for Cron.</summary>
    public string ScheduleValue { get; set; } = string.Empty;

    /// <summary>Maximum retry attempts on failure.</summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>Maximum execution timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>Whether this job type is active.</summary>
    public bool IsActive { get; set; } = true;

    public byte[] RowVersion { get; set; } = [];
}
