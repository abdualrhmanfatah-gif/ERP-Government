using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class YearClosingRun : BaseAuditableEntity
{
    public int FiscalYearId { get; set; }
    public DateTimeOffset RunAt { get; set; }
    public int RunById { get; set; }
    public Enums.YearClosingRunType RunType { get; set; }
    public decimal LapsedAppropriationTotal { get; set; }
    public decimal LapsedEncumbranceTotal { get; set; }
    public Enums.YearClosingRunStatus Status { get; set; } = Enums.YearClosingRunStatus.Completed;
    public int? ReversedById { get; set; }
    public DateTimeOffset? ReversedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FinancialSettings.Entities.FiscalYear FiscalYear { get; set; } = null!;
}
