using ERP_Government.Application.Accounting.Reports.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public class PdfReportExporter : IReportExporter
{
    public Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ExcelReportExporter for Excel export.");
    }

    public Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header()
                    .Text(reportName)
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Black);

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Item().Text($"Currency: {result.Currency} | Generated: {result.GeneratedAt:yyyy-MM-dd HH:mm}");
                        column.Item().PaddingBottom(10);

                        foreach (var section in result.Sections)
                        {
                            column.Item().Text(section.Title).Bold().FontSize(14);
                            column.Item().PaddingBottom(5);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").Bold();
                                    header.Cell().Text("Name").Bold();
                                    header.Cell().Text("Debit").Bold();
                                    header.Cell().Text("Credit").Bold();
                                });

                                foreach (var line in section.Lines)
                                {
                                    table.Cell().Text(line.AccountCode);
                                    table.Cell().Text(line.AccountName);
                                    table.Cell().Text(line.Debit.ToString("N2"));
                                    table.Cell().Text(line.Credit.ToString("N2"));
                                }

                                table.Footer(footer =>
                                {
                                    footer.Cell().Text("Total").Bold();
                                    footer.Cell();
                                    footer.Cell().Text(section.Total.ToString("N2")).Bold();
                                });
                            });

                            column.Item().PaddingBottom(15);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        });

        document.GeneratePdf(outputStream);
        return Task.CompletedTask;
    }
}
