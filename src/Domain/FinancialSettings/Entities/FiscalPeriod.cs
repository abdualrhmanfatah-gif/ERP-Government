using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class FiscalPeriod : BaseAuditableEntity
{
    public int FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsLockedForPosting { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
