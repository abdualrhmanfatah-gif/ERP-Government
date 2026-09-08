
using ERP_Government.Application.Accounting.Reports.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public class PdfReportExporter : IReportExporter
{
    private static bool _licenseConfigured;
    private static readonly Color PrimaryAccentColor = Color.FromHex("#1E5B3C"); // الأخضر الرسمي

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

                // ── Header (ترويسة الصفحة الرسمية) ──
                page.Header().Element(header => ComposeOfficialHeader(header, reportName, result));

                // ── Content (محتوى التقرير والجداول) ──
                page.Content().PaddingVertical(10).Column(content =>
                {
                    foreach (var section in result.Sections)
                    {
                        content.Item().PaddingBottom(6).Text(section.Title)
                            .Bold().FontSize(12).FontColor(PrimaryAccentColor);

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

                // ── Footer (تذييل الصفحة - صادر عن + رقم الصفحة) ──
                page.Footer().Element(ComposeExternalFooter);
            });
        });

        document.GeneratePdf(outputStream);
        return Task.CompletedTask;
    }

    private static void ComposeOfficialHeader(IContainer header, string reportName, ReportResult result)
    {
        header.Column(column =>
        {
            // ── الهيكل العلوي: اليمين (الوزارة)، الوسط (اللوجو)، اليسار (التاريخ والرقم) ──
            column.Item().Row(row =>
            {
                // 1. العمود الأيمن (اسم الجهة والوزارة)
                row.RelativeItem().Column(rightCol =>
                {
                    rightCol.Spacing(2);
                    rightCol.Item().AlignCenter().Text(ReportBranding.GovernmentLine)
                        .FontSize(13).Bold().FontColor(Colors.Black);
                    
                    rightCol.Item().AlignCenter().Text("وزارة الداخلية")
                        .FontSize(13).Bold().FontColor(Colors.Black);

                    if (!string.IsNullOrWhiteSpace(ReportBranding.OrganizationName))
                    {
                        rightCol.Item().AlignCenter().Text(ReportBranding.OrganizationName)
                            .FontSize(13).Bold().FontColor(Colors.Black);
                    }

                    if (!string.IsNullOrWhiteSpace(ReportBranding.DepartmentName))
                    {
                        rightCol.Item().AlignCenter().Text(ReportBranding.DepartmentName)
                            .FontSize(11).Bold().FontColor(Colors.Black);
                    }
                });

                // 2. العمود الأوسط (الشعار)
                row.ConstantItem(200).Column(centerCol =>
                {
                    if (ReportBranding.LogoPath is not null)
                    {
                        centerCol.Item().AlignCenter().PaddingTop(2).Height(70).Image(ReportBranding.LogoPath).FitArea();
                    }
                });

                // 3. العمود الأيسر (بيانات التوثيق)
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().AlignLeft().Width(180).Column(info =>
                    {
                        info.Spacing(6);

                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("التاريخ : ").FontSize(10).Bold();
                            r.RelativeItem().AlignRight().Text($"{DateTime.Now:yyyy/MM/dd}م").FontSize(10).Bold();
                        });

                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("الرقم : ").FontSize(10).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("المرجع : ").FontSize(10).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2).BorderBottom(1).BorderColor(Colors.Black);
                        });
                    });
                });
            });

            // ── خطوط الفصل المزدوجة ──
            column.Item().PaddingTop(8).Element(e => e.BorderBottom(1.5f).BorderColor(PrimaryAccentColor));
            column.Item().PaddingTop(2).Element(e => e.BorderBottom(0.5f).BorderColor(PrimaryAccentColor));

            // ── عنوان التقرير والملاحظات ──
            column.Item().PaddingTop(8).AlignCenter().Text(reportName)
                .FontSize(15).Bold().FontColor(Colors.Black);

            column.Item().PaddingTop(2).AlignCenter()
                .Text($"العملة: {result.Currency}  |  تاريخ الإنشاء: {result.GeneratedAt:yyyy-MM-dd HH:mm}")
                .FontSize(9).FontColor(Colors.Grey.Darken2);

            if (!string.IsNullOrEmpty(result.DataWarning))
            {
                column.Item().PaddingTop(2).AlignCenter()
                    .Text(result.DataWarning).FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
            }
        });
    }

    private static void ComposeExternalFooter(IContainer footer)
    {
        footer.PaddingTop(5).Row(row =>
        {
            // صادر عن (يمين)
            row.RelativeItem().AlignRight()
                .Text($"صادر عن: {ReportBranding.OrganizationName}")
                .FontSize(8.5f).FontColor(Colors.Grey.Darken1);

            // رقم الصفحة (وسط)
            row.ConstantItem(120).AlignCenter()
                .Text(text =>
                {
                    text.Span("صفحة ").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    text.Span(" من ").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    text.TotalPages().FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                });

            // مساحة متوازنة (يسار)
            row.RelativeItem();
        });
    }

    private static void WriteExtendedSection(ColumnDescriptor column, ReportSection section)
    {
        var headers = section.ColumnHeaders!;

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f); // رمز الحساب
                columns.RelativeColumn(3f);   // اسم الحساب
                for (int i = 2; i < headers.Count; i++)
                {
                    columns.RelativeColumn(2f); // القيم المالية
                }
            });

            table.Header(header =>
            {
                foreach (var headerText in headers)
                {
                    header.Cell().Element(HeaderCellStyle).Text(headerText).Bold();
                }
            });

            foreach (var line in section.Lines)
            {
                table.Cell().Element(CellStyle).Text(line.AccountCode);
                table.Cell().Element(CellStyle).Text(line.AccountName);

                var values = line.Values ?? [];
                foreach (var value in values)
                {
                    table.Cell().Element(NumberCellStyle).Text(value.ToString("N2"));
                }
            }

            if (section.ColumnTotals is not null)
            {
                table.Footer(footer =>
                {
                    footer.Cell().ColumnSpan(2).Element(FooterCellStyle).Text("الإجماليات").Bold();

                    foreach (var total in section.ColumnTotals)
                    {
                        footer.Cell().Element(FooterCellStyle).Text(total.ToString("N2")).Bold();
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
                header.Cell().Element(HeaderCellStyle).Text("رقم الحساب").Bold();
                header.Cell().Element(HeaderCellStyle).Text("اسم الحساب").Bold();
                header.Cell().Element(HeaderCellStyle).Text("مدين").Bold();
                header.Cell().Element(HeaderCellStyle).Text("دائن").Bold();
            });

            foreach (var line in section.Lines)
            {
                table.Cell().Element(CellStyle).Text(line.AccountCode);
                table.Cell().Element(CellStyle).Text(line.AccountName);
                table.Cell().Element(NumberCellStyle).Text(line.Debit.ToString("N2"));
                table.Cell().Element(NumberCellStyle).Text(line.Credit.ToString("N2"));
            }

            var totalDebit = section.Lines.Sum(l => l.Debit);
            var totalCredit = section.Lines.Sum(l => l.Credit);

            table.Footer(footer =>
            {
                footer.Cell().ColumnSpan(2).Element(FooterCellStyle).Text("الإجمالي").Bold();
                footer.Cell().Element(FooterCellStyle).Text(totalDebit.ToString("N2")).Bold();
                footer.Cell().Element(FooterCellStyle).Text(totalCredit.ToString("N2")).Bold();
            });
        });
    }

    // ── تنسيقات خلايا الجدول الموحدة (Style Helpers) ──

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container.Background("#F3F4F6")
            .BorderBottom(1.5f).BorderColor(Colors.Grey.Darken2)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle();
    }

    private static IContainer NumberCellStyle(IContainer container)
    {
        return container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    private static IContainer FooterCellStyle(IContainer container)
    {
        return container.Background("#E5E7EB")
            .BorderTop(1.5f).BorderColor(Colors.Grey.Darken2)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
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

