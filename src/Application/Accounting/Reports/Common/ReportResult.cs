namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportResult
{
    public string Currency { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }

    public int TotalLines { get; init; }

    /// <summary>
    /// Optional partial-data warning rendered in the export header (RC-4),
    /// e.g. "بيانات جزئية" when the report range includes the currently open fiscal period.
    /// </summary>
    public string? DataWarning { get; init; }

    /// <summary>
    /// Optional paper size format for export e.g. "A4", "A3", "A5", "Letter", "Legal".
    /// Defaults to "A4" if null.
    /// </summary>
    public string? PaperSize { get; init; }

    /// <summary>
    /// Optional page orientation. Defaults to true (Landscape).
    /// </summary>
    public bool IsLandscape { get; init; } = true;

    public List<ReportSection> Sections { get; init; } = [];
}
