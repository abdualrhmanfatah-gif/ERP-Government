namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportSection
{
    public string Title { get; init; } = string.Empty;

    public string TitleEn { get; init; } = string.Empty;

    public string? Description { get; init; }

    public List<ReportLine> Lines { get; init; } = [];

    public decimal Total { get; init; }

    /// <summary>
    /// Extended column layout: when set, exporters render these headers with
    /// per-row Values and per-column ColumnTotals instead of the legacy
    /// Code/Name/Debit/Credit/Balance layout. Additive — legacy mappers leave it null.
    /// </summary>
    public List<string>? ColumnHeaders { get; init; }

    /// <summary>Totals per extended column (same length as ColumnHeaders' numeric columns).</summary>
    public List<decimal>? ColumnTotals { get; init; }
}
