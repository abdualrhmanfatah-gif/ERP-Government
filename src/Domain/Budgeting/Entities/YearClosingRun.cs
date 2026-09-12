using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class YearClosingRun : BaseAuditableEntity
{
    public int FiscalYearId { get; set; }
    public int? FundId { get; set; }
    public Enums.YearClosingRunType RunType { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int RunById { get; set; }
    public Enums.YearClosingRunStatus Status { get; set; } = Enums.YearClosingRunStatus.Pending;
    public decimal LapsedAppropriationTotal { get; set; }
    public decimal LapsedEncumbranceTotal { get; set; }
    public int? ReversalRunId { get; set; }
    public string? ReversalReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FinancialSettings.Entities.FiscalYear FiscalYear { get; set; } = null!;
    public YearClosingRun? ReversalRun { get; set; }
}
