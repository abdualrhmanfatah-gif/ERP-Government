namespace ERP_Government.Application.Accounting.Reports.CashFlowStatement;

public class CashFlowStatementDto
{
    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public string Currency { get; init; } = string.Empty;

    public CashFlowSection Operating { get; init; } = new();

    public CashFlowSection Investing { get; init; } = new();

    public CashFlowSection Financing { get; init; } = new();

    public decimal NetChange { get; init; }

    public decimal OpeningCash { get; init; }

    public decimal ClosingCash { get; init; }

    public bool Reconciled { get; init; }

    public string? Warning { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}

public class CashFlowSection
{
    public string Title { get; init; } = string.Empty;

    public string TitleAr { get; init; } = string.Empty;

    public List<CashFlowLineItem> Items { get; init; } = [];

    public decimal Total { get; init; }
}

public class CashFlowLineItem
{
    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }
}
