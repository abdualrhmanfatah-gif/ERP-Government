using ClosedXML;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Infrastructure.Services;

public static class ExcelHeaderWriter
{
    private const int LogoMaxHeightPx = 80;
    private const int HeaderImageMaxHeightPx = 30;

    public static int Compose(IXLWorksheet worksheet, ReportResult result, string reportName, int startRow)
    {
        var row = startRow;

        row = ComposeImageRow(worksheet, row);

        if (!string.IsNullOrWhiteSpace(ReportBranding.OrganizationName))
        {
            var cell = worksheet.Cell(row, 1);
            cell.Value = ReportBranding.OrganizationName;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 11;
        }

        if (!string.IsNullOrWhiteSpace(ReportBranding.DepartmentName))
        {
            row++;
            var cell = worksheet.Cell(row, 1);
            cell.Value = ReportBranding.DepartmentName;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 10;
        }

        row += 2;

        var dateCell = worksheet.Cell(row, 1);
        dateCell.Value = $"التاريخ: {result.GeneratedAt:yyyy/MM/dd}م";
        dateCell.Style.Font.Bold = true;
        dateCell.Style.Font.FontSize = 9;

        var currencyCell = worksheet.Cell(row, 3);
        currencyCell.Value = $"العملة: {result.Currency}";
        currencyCell.Style.Font.Bold = true;
        currencyCell.Style.Font.FontSize = 9;

        row++;

        var createdCell = worksheet.Cell(row, 1);
        createdCell.Value = $"تاريخ الإنشاء: {result.GeneratedAt:yyyy-MM-dd HH:mm}";
        createdCell.Style.Font.FontSize = 9;

        if (!string.IsNullOrEmpty(result.DataWarning))
        {
            row++;
            var warningCell = worksheet.Cell(row, 1);
            warningCell.Value = result.DataWarning;
            warningCell.Style.Font.FontColor = XLColor.Orange;
            warningCell.Style.Font.Bold = true;
            warningCell.Style.Font.FontSize = 9;
        }

        row += 2;

        var titleCell = worksheet.Cell(row, 1);
        titleCell.Value = reportName;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var lastCol = worksheet.RangeUsed()?.LastColumn()?.ColumnNumber() ?? 8;
        worksheet.Range(row, 1, row, lastCol).Merge();
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        row += 2;

        return row;
    }

    private static int ComposeImageRow(IXLWorksheet worksheet, int row)
    {
        var imageRow = row;
        var hasImages = false;

        if (TryAddImage(worksheet, ReportBranding.RepublicHeaderPath, "republic_header", imageRow, 1, HeaderImageMaxHeightPx))
            hasImages = true;

        if (TryAddImage(worksheet, ReportBranding.MinistryHeaderPath, "ministry_header", imageRow + 1, 1, HeaderImageMaxHeightPx))
            hasImages = true;

        if (TryAddImage(worksheet, ReportBranding.LogoPath, "logo", imageRow, 5, LogoMaxHeightPx))
            hasImages = true;

        return hasImages ? row + 3 : row;
    }

    private static bool TryAddImage(
        IXLWorksheet worksheet,
        string? filePath,
        string imageId,
        int row,
        int col,
        int maxHeightPx)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            using var stream = new MemoryStream(bytes);
            var pic = worksheet.AddPicture(stream, XLPictureFormat.Png)
                .MoveTo(worksheet.Cell(row, col));

            if (pic.Height > maxHeightPx)
            {
                var ratio = (double)maxHeightPx / pic.Height;
                pic.Width = (int)(pic.Width * ratio);
                pic.Height = maxHeightPx;
            }

            return true;
        }
        catch
        {
            // Image loading is non-fatal — report renders without logos gracefully.
            return false;
        }
    }
}
