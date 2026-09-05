namespace ERP_Government.Application.Accounting.Reports.Common;

public interface IReportExporter
{
    Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default);

    Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default);
}
