namespace ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;

public record TrialBalanceReportDto
{
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public int? FiscalPeriodId { get; init; }
    public string? FiscalPeriodName { get; init; }
    public List<TrialBalanceLineDto> Lines { get; init; } = [];
    public TrialBalanceTotalDto Totals { get; init; } = new();
}

public record TrialBalanceLineDto
{
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal OpeningDebit { get; init; }
    public decimal OpeningCredit { get; init; }
    public decimal DebitTotal { get; init; }
    public decimal CreditTotal { get; init; }
    public decimal ClosingBalance { get; init; }
}

public record TrialBalanceTotalDto
{
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal TotalOpeningBalance { get; init; }
    public decimal TotalClosingBalance { get; init; }
}
