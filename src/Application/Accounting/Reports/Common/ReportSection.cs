namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportSection
{
    public string Title { get; init; } = string.Empty;

    public string TitleEn { get; init; } = string.Empty;

    public List<ReportLine> Lines { get; init; } = [];

    public decimal Total { get; init; }
}
