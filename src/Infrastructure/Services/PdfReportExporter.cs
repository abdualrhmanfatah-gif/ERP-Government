
using ERP_Government.Application.Accounting.Reports.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public class PdfReportExporter : IReportExporter
{
    private static bool _licenseConfigured;

    public static void Configure(string? licenseName)
    {
        if (!Enum.TryParse<LicenseType>(licenseName, ignoreCase: true, out var license))
        {
            throw new InvalidOperationException("QuestPdf:License must be configured as Community, Professional, or Enterprise.");
        }

        QuestPDF.Settings.License = license;
        _licenseConfigured = true;
    }

    private static void EnsureLicenseConfigured()
    {
        if (!_licenseConfigured)
        {
            throw new InvalidOperationException("QuestPdf:License must be configured as Community, Professional, or Enterprise.");
        }
    }

    public Task ExportExcelAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ExcelReportExporter for Excel export.");
    }

    public Task ExportPdfAsync(ReportResult result, string reportName, Stream outputStream, CancellationToken cancellationToken = default)
    {
        EnsureLicenseConfigured();

        var pageSize = ResolvePageSize(result.PaperSize, result.IsLandscape);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.MarginHorizontal(15);
                page.MarginVertical(15);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(style => style.FontFamily("Calibri").FontSize(10));

                page.Header().Element(header => OfficialPdfTemplate.ComposeOfficialHeader(
                    header,
                    reportName,
                    result.Currency,
                    result.GeneratedAt,
                    result.DataWarning));

                page.Content().PaddingVertical(10).Column(content =>
                {
                    foreach (var section in result.Sections)
                    {
                        content.Item().PaddingBottom(6).Text(section.Title)
                            .Bold().FontSize(12).FontColor(OfficialPdfTemplate.PrimaryAccentColor);

                        if (!string.IsNullOrWhiteSpace(section.Description))
                        {
                            content.Item().PaddingBottom(4).Text(section.Description)
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        }

                        if (section.ColumnHeaders is not null)
                        {
                            WriteExtendedSection(content, section);
                        }
                        else
                        {
                            WriteLegacySection(content, section);
                        }

                        content.Item().PaddingBottom(16);
                    }
                });

                page.Footer().Element(OfficialPdfTemplate.ComposeExternalFooter);
            });
        });

        document.GeneratePdf(outputStream);
        return Task.CompletedTask;
    }

    private static void WriteExtendedSection(ColumnDescriptor column, ReportSection section)
    {
        var headers = section.ColumnHeaders!;

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(3f);
                for (int i = 2; i < headers.Count; i++)
                {
                    columns.RelativeColumn(2f);
                }
            });

            table.Header(header =>
            {
                foreach (var headerText in headers)
                {
                    header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text(headerText).Bold();
                }
            });

            foreach (var line in section.Lines)
            {
                table.Cell().Element(OfficialPdfTemplate.CellStyle).Text(line.AccountCode);
                table.Cell().Element(OfficialPdfTemplate.CellStyle).Text(line.AccountName);

                var values = line.Values ?? [];
                foreach (var value in values)
                {
                    table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text(value.ToString("N2"));
                }
            }

            if (section.ColumnTotals is not null)
            {
                table.Footer(footer =>
                {
                    footer.Cell().ColumnSpan(2).Element(OfficialPdfTemplate.FooterCellStyle).Text("الإجماليات").Bold();

                    foreach (var total in section.ColumnTotals)
                    {
                        footer.Cell().Element(OfficialPdfTemplate.FooterCellStyle).Text(total.ToString("N2")).Bold();
                    }
                });
            }
        });
    }

    private static void WriteLegacySection(ColumnDescriptor column, ReportSection section)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(3.5f);
                columns.RelativeColumn(2f);
                columns.RelativeColumn(2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("رقم الحساب").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("اسم الحساب").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("مدين").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("دائن").Bold();
            });

            foreach (var line in section.Lines)
            {
                table.Cell().Element(OfficialPdfTemplate.CellStyle).Text(line.AccountCode);
                table.Cell().Element(OfficialPdfTemplate.CellStyle).Text(line.AccountName);
                table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text(line.Debit.ToString("N2"));
                table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text(line.Credit.ToString("N2"));
            }

            var totalDebit = section.Lines.Sum(l => l.Debit);
            var totalCredit = section.Lines.Sum(l => l.Credit);

            table.Footer(footer =>
            {
                footer.Cell().ColumnSpan(2).Element(OfficialPdfTemplate.FooterCellStyle).Text("الإجمالي").Bold();
                footer.Cell().Element(OfficialPdfTemplate.FooterCellStyle).Text(totalDebit.ToString("N2")).Bold();
                footer.Cell().Element(OfficialPdfTemplate.FooterCellStyle).Text(totalCredit.ToString("N2")).Bold();
            });
        });
    }

    private static PageSize ResolvePageSize(string? pageSizeName, bool isLandscape)
    {
        var baseSize = pageSizeName?.Trim().ToUpperInvariant() switch
        {
            "A3" => PageSizes.A3,
            "A5" => PageSizes.A5,
            "LETTER" => PageSizes.Letter,
            "LEGAL" => PageSizes.Legal,
            _ => PageSizes.A4
        };

        return isLandscape ? baseSize.Landscape() : baseSize.Portrait();
    }
}
