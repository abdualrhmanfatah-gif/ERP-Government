using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Application.Accounting.Reports.TrialBalance;

public class TrialBalanceDto
{
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public int FiscalPeriodId { get; init; }
    public string PeriodName { get; init; } = string.Empty;
    public List<ReportSection> Sections { get; init; } = [];
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public bool IsBalanced { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
}
