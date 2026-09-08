using ClosedXML.Excel;
using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Infrastructure.Services;

public class ExcelReportExporter : IReportExporter
{
    public Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportName);
        worksheet.RightToLeft = true;

        // Header
        worksheet.Cell(1, 1).Value = reportName;
        worksheet.Cell(2, 1).Value = $"العملة: {result.Currency}";
        worksheet.Cell(3, 1).Value = $"تاريخ الإنشاء: {result.GeneratedAt:yyyy-MM-dd HH:mm}";
        if (!string.IsNullOrEmpty(result.DataWarning))
        {
            var warningCell = worksheet.Cell(4, 1);
            warningCell.Value = result.DataWarning;
            warningCell.Style.Font.FontColor = XLColor.Orange;
            warningCell.Style.Font.Bold = true;
        }

        var row = 6;
        foreach (var section in result.Sections)
        {
            worksheet.Cell(row, 1).Value = section.Title;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            if (section.ColumnHeaders is not null)
            {
                WriteExtendedSection(worksheet, section, ref row);
                continue;
            }

            WriteLegacySection(worksheet, section, ref row);
        }

        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(outputStream);
        return Task.CompletedTask;
    }

    private static void WriteExtendedSection(IXLWorksheet worksheet, ReportSection section, ref int row)
    {
        var headers = section.ColumnHeaders!;
        for (var c = 0; c < headers.Count; c++)
        {
            worksheet.Cell(row, c + 1).Value = headers[c];
        }
        worksheet.Range(row, 1, row, headers.Count).Style.Font.Bold = true;
        row++;

        foreach (var line in section.Lines)
        {
            var values = line.Values ?? [];
            worksheet.Cell(row, 1).Value = line.AccountCode;
            worksheet.Cell(row, 2).Value = line.AccountName;
            for (var c = 0; c < values.Count; c++)
            {
                var cell = worksheet.Cell(row, c + 3);
                cell.Value = values[c];
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.NumberFormat.Format = "#,##0.00";
            }
            row++;
        }

        if (section.ColumnTotals is not null)
        {
            var totals = section.ColumnTotals;
            var totalsLabelCell = worksheet.Cell(row, 1);
            totalsLabelCell.Value = "الإجماليات";
            totalsLabelCell.Style.Font.Bold = true;
            for (var c = 0; c < totals.Count; c++)
            {
                var cell = worksheet.Cell(row, c + 3);
                cell.Value = totals[c];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.NumberFormat.Format = "#,##0.00";
            }
            row++;
        }

        row++;
    }

    private static void WriteLegacySection(IXLWorksheet worksheet, ReportSection section, ref int row)
    {
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

    public Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use PdfReportExporter for PDF export.");
    }
}
