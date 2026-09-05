using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class FiscalYear : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int YearNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public FiscalYearStatus Status { get; set; }
    public bool IsClosed { get; set; }
    public int? ClosingJournalEntryId { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    private readonly List<FiscalPeriod> _periods = [];
    public IReadOnlyCollection<FiscalPeriod> Periods => _periods.AsReadOnly();

    public void AddPeriod(FiscalPeriod period)
    {
        _periods.Add(period);
    }

    public void RemovePeriod(FiscalPeriod period)
    {
        _periods.Remove(period);
    }
}
