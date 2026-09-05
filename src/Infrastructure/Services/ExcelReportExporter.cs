using ClosedXML.Excel;
using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Infrastructure.Services;

public class ExcelReportExporter : IReportExporter
{
    public Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportName);

        // Header
        worksheet.Cell(1, 1).Value = reportName;
        worksheet.Cell(2, 1).Value = $"Currency: {result.Currency}";
        worksheet.Cell(3, 1).Value = $"Generated: {result.GeneratedAt:yyyy-MM-dd HH:mm}";

        var row = 5;
        foreach (var section in result.Sections)
        {
            worksheet.Cell(row, 1).Value = section.Title;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            worksheet.Cell(row, 1).Value = "Account Code";
            worksheet.Cell(row, 2).Value = "Account Name";
            worksheet.Cell(row, 3).Value = "Debit";
            worksheet.Cell(row, 4).Value = "Credit";
            worksheet.Cell(row, 5).Value = "Balance";
            worksheet.Range(row, 1, row, 5).Style.Font.Bold = true;
            row++;

            foreach (var line in section.Lines)
            {
                worksheet.Cell(row, 1).Value = line.AccountCode;
                worksheet.Cell(row, 2).Value = line.AccountName;
                worksheet.Cell(row, 3).Value = line.Debit;
                worksheet.Cell(row, 4).Value = line.Credit;
                worksheet.Cell(row, 5).Value = line.Balance;
                row++;
            }

            worksheet.Cell(row, 2).Value = "Total";
            worksheet.Cell(row, 5).Value = section.Total;
            worksheet.Range(row, 2, row, 5).Style.Font.Bold = true;
            row += 2;
        }

        workbook.SaveAs(outputStream);
        return Task.CompletedTask;
    }

    public Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use PdfReportExporter for PDF export.");
    }
}
