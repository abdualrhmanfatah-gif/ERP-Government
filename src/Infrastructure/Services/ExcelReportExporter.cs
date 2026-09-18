using ClosedXML.Excel;
using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Infrastructure.Services;

public class ExcelReportExporter : IReportExporter
{
    private static readonly XLColor HeaderBgColor = XLColor.FromHtml("#F3F4F6");
    private static readonly XLColor FooterBgColor = XLColor.FromHtml("#E5E7EB");
    private static readonly XLColor BorderColorBlack = XLColor.Black;
    private static readonly XLColor BorderColorLight = XLColor.FromHtml("#CCCCCC");

    public Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportName);
        worksheet.RightToLeft = true;

        var row = ExcelHeaderWriter.Compose(worksheet, result, reportName, 1);

        foreach (var section in result.Sections)
        {
            row = WriteSection(worksheet, section, row);
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(outputStream);
        return Task.CompletedTask;
    }

    private static int WriteSection(IXLWorksheet worksheet, ReportSection section, int row)
    {
        var titleCell = worksheet.Cell(row, 1);
        titleCell.Value = section.Title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 12;
        row++;

        if (!string.IsNullOrWhiteSpace(section.Description))
        {
            worksheet.Cell(row, 1).Value = section.Description;
            worksheet.Cell(row, 1).Style.Font.FontSize = 10;
            worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml("#666666");
            row++;
        }

        row = section.ColumnHeaders is not null
            ? WriteExtendedSection(worksheet, section, row)
            : WriteLegacySection(worksheet, section, row);

        row++;
        return row;
    }

    private static int WriteExtendedSection(IXLWorksheet worksheet, ReportSection section, int row)
    {
        var headers = section.ColumnHeaders!;
        var totalCols = headers.Count;

        for (var c = 0; c < totalCols; c++)
        {
            ApplyHeaderCell(worksheet.Cell(row, c + 1), headers[c]);
        }
        row++;

        foreach (var line in section.Lines)
        {
            ApplyDataCell(worksheet.Cell(row, 1), line.AccountCode);
            ApplyDataCell(worksheet.Cell(row, 2), line.AccountName);

            var values = line.Values ?? [];
            for (var c = 0; c < values.Count; c++)
            {
                ApplyNumberCell(worksheet.Cell(row, c + 3), values[c]);
            }
            row++;
        }

        if (section.ColumnTotals is not null)
        {
            var labelCell = worksheet.Cell(row, 1);
            labelCell.Value = "الإجماليات";
            ApplyFooterCell(labelCell, bold: true);

            worksheet.Range(row, 1, row, 2).Merge();
            ApplyFooterBg(worksheet.Range(row, 1, row, 2));

            for (var c = 0; c < section.ColumnTotals.Count; c++)
            {
                ApplyFooterNumberCell(worksheet.Cell(row, c + 3), section.ColumnTotals[c]);
            }
            row++;
        }

        row++;
        return row;
    }

    private static int WriteLegacySection(IXLWorksheet worksheet, ReportSection section, int row)
    {
        var arabicHeaders = new[] { "رقم الحساب", "اسم الحساب", "مدين", "دائن", "الرصيد" };

        for (var c = 0; c < arabicHeaders.Length; c++)
        {
            ApplyHeaderCell(worksheet.Cell(row, c + 1), arabicHeaders[c]);
        }
        row++;

        foreach (var line in section.Lines)
        {
            ApplyDataCell(worksheet.Cell(row, 1), line.AccountCode);
            ApplyDataCell(worksheet.Cell(row, 2), line.AccountName);
            ApplyNumberCell(worksheet.Cell(row, 3), line.Debit);
            ApplyNumberCell(worksheet.Cell(row, 4), line.Credit);
            ApplyNumberCell(worksheet.Cell(row, 5), line.Balance);
            row++;
        }

        var totalDebit = section.Lines.Sum(l => l.Debit);
        var totalCredit = section.Lines.Sum(l => l.Credit);
        var totalBalance = section.Lines.Sum(l => l.Balance);

        worksheet.Range(row, 1, row, 2).Merge();
        var footerLabel = worksheet.Cell(row, 1);
        footerLabel.Value = "الإجمالي";
        ApplyFooterCell(footerLabel, bold: true);
        ApplyFooterBg(worksheet.Range(row, 1, row, 2));
        SetFooterBorders(worksheet.Range(row, 1, row, 2));

        ApplyFooterNumberCell(worksheet.Cell(row, 3), totalDebit);
        ApplyFooterNumberCell(worksheet.Cell(row, 4), totalCredit);
        ApplyFooterNumberCell(worksheet.Cell(row, 5), totalBalance);
        row++;

        row++;
        return row;
    }

    private static void ApplyHeaderCell(IXLCell cell, string value)
    {
        cell.Value = value;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 10;
        cell.Style.Font.FontColor = XLColor.Black;
        cell.Style.Fill.BackgroundColor = HeaderBgColor;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.BottomBorderColor = BorderColorBlack;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private static void ApplyDataCell(IXLCell cell, string value)
    {
        cell.Value = value;
        cell.Style.Font.FontSize = 10;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        cell.Style.Border.BottomBorderColor = BorderColorLight;
    }

    private static void ApplyNumberCell(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.Font.FontSize = 10;
        cell.Style.NumberFormat.Format = "#,##0.00";
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        cell.Style.Border.BottomBorderColor = BorderColorLight;
    }

    private static void ApplyFooterCell(IXLCell cell, bool bold)
    {
        cell.Style.Font.Bold = bold;
        cell.Style.Font.FontSize = 10;
        cell.Style.Fill.BackgroundColor = FooterBgColor;
    }

    private static void ApplyFooterNumberCell(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 10;
        cell.Style.NumberFormat.Format = "#,##0.00";
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        cell.Style.Fill.BackgroundColor = FooterBgColor;
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.TopBorderColor = BorderColorBlack;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Double;
        cell.Style.Border.BottomBorderColor = BorderColorBlack;
    }

    private static void ApplyFooterBg(IXLRange range)
    {
        range.Style.Fill.BackgroundColor = FooterBgColor;
    }

    private static void SetFooterBorders(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.TopBorderColor = BorderColorBlack;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Double;
        range.Style.Border.BottomBorderColor = BorderColorBlack;
    }

    public Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use PdfReportExporter for PDF export.");
    }
}
