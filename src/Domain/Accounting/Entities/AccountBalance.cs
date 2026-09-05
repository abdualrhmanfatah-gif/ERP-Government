using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Period-based aggregated balance layer per spec FR-001.
/// JournalEntries + JournalEntryLines remain the source of truth; this is a materialized layer
/// that can always be rebuilt from posted journal entries.
/// </summary>
public class AccountBalance : BaseAuditableEntity
{
    public int AccountId { get; set; }
    public int FiscalYearId { get; set; }
    public int FiscalPeriodId { get; set; }
    public int CurrencyId { get; set; }

    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }

    public bool IsFinalized { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    // Navigation properties
    public Account Account { get; set; } = null!;
    public FiscalYear FiscalYear { get; set; } = null!;
    public FiscalPeriod FiscalPeriod { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
