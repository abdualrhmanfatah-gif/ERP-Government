namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportResult
{
    public string Currency { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }

    public int TotalLines { get; init; }

    public List<ReportSection> Sections { get; init; } = [];
}
