using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Application.Accounting.Reports.IncomeStatement;

public class IncomeStatementDto
{
    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public string Currency { get; init; } = string.Empty;

    public IncomeStatementGroup Revenue { get; init; } = new();

    public IncomeStatementGroup Expenses { get; init; } = new();

    public decimal NetIncome { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}

public class IncomeStatementGroup
{
    public List<ReportSection> Sections { get; init; } = [];

    public decimal Total { get; init; }
}
