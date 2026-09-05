namespace ERP_Government.Application.Accounting.Reports.GeneralLedger;

public class GeneralLedgerDto
{
    public string Currency { get; init; } = string.Empty;

    public int TotalLines { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public List<GeneralLedgerLine> Lines { get; init; } = [];

    public GeneralLedgerTotals Totals { get; init; } = new();

    public DateTimeOffset GeneratedAt { get; init; }
}

public class GeneralLedgerLine
{
    public DateOnly DocumentDate { get; init; }

    public string EntryNumber { get; init; } = string.Empty;

    public string Reference { get; init; } = string.Empty;

    public string Narration { get; init; } = string.Empty;

    public string AccountCode { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public decimal Debit { get; init; }

    public decimal Credit { get; init; }

    public decimal RunningBalance { get; init; }
}

public class GeneralLedgerTotals
{
    public decimal Debit { get; init; }

    public decimal Credit { get; init; }
}
