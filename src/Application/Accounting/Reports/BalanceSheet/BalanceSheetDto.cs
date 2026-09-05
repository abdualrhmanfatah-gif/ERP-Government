using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Application.Accounting.Reports.BalanceSheet;

public class BalanceSheetDto
{
    public DateOnly AsOfDate { get; init; }

    public string Currency { get; init; } = string.Empty;

    public BalanceSheetGroup Assets { get; init; } = new();

    public BalanceSheetGroup Liabilities { get; init; } = new();

    public BalanceSheetGroup Equity { get; init; } = new();

    public decimal LiabilitiesAndEquity { get; init; }

    public bool Balanced { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}

public class BalanceSheetGroup
{
    public List<ReportSection> Sections { get; init; } = [];

    public decimal Total { get; init; }
}
