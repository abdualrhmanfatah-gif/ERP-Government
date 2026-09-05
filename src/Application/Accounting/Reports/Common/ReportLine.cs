namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportLine
{
    public string AccountCode { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public decimal Debit { get; init; }

    public decimal Credit { get; init; }

    public decimal Balance { get; init; }
}
