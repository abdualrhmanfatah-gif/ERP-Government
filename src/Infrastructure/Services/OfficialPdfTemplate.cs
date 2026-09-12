using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public static class OfficialPdfTemplate
{
    public static readonly Color PrimaryAccentColor = Color.FromHex("#1E5B3C");

    public static void ComposeOfficialHeader(
        IContainer header,
        string reportName,
        string currency,
        DateTimeOffset generatedAt,
        string? dataWarning = null,
        string? orderNumber = null)
    {
        header.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(1.2f).Column(rightCol =>
                {
                    rightCol.Spacing(2);
                    rightCol.Item().AlignCenter().Text(ReportBranding.GovernmentLine)
                        .FontSize(11).Bold().FontColor(Colors.Black);
                    rightCol.Item().AlignCenter().Text("وزارة الداخلية")
                        .FontSize(11).Bold().FontColor(Colors.Black);
                    if (!string.IsNullOrWhiteSpace(ReportBranding.OrganizationName))
                        rightCol.Item().AlignCenter().Text(ReportBranding.OrganizationName)
                            .FontSize(10).Bold().FontColor(Colors.Black);
                    if (!string.IsNullOrWhiteSpace(ReportBranding.DepartmentName))
                        rightCol.Item().AlignCenter().Text(ReportBranding.DepartmentName)
                            .FontSize(9).Bold().FontColor(Colors.Black);
                });

                row.RelativeItem(0.8f).Column(centerCol =>
                {
                    if (ReportBranding.LogoPath is not null)
                        centerCol.Item().AlignCenter().Height(50)
                            .Image(ReportBranding.LogoPath).FitArea();
                });

                row.RelativeItem(1f).Column(leftCol =>
                {
                    leftCol.Item().AlignLeft().Column(info =>
                    {
                        info.Spacing(4);
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("التاريخ : ").FontSize(9).Bold();
                            r.RelativeItem().AlignRight()
                                .Text($"{DateTime.Now:yyyy/MM/dd}م").FontSize(9).Bold();
                        });
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("الرقم : ").FontSize(9).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2)
                                .BorderBottom(1).BorderColor(Colors.Black)
                                .Text(orderNumber ?? string.Empty);
                        });
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("المرجع : ").FontSize(9).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2)
                                .BorderBottom(1).BorderColor(Colors.Black);
                        });
                    });
                });
            });

            column.Item().PaddingTop(6)
                .Element(e => e.BorderBottom(1.5f).BorderColor(PrimaryAccentColor));
            column.Item().PaddingTop(2)
                .Element(e => e.BorderBottom(0.5f).BorderColor(PrimaryAccentColor));

            column.Item().PaddingTop(6).AlignCenter()
                .Text(reportName).FontSize(13).Bold().FontColor(Colors.Black);

            column.Item().PaddingTop(2).AlignCenter()
                .Text($"العملة: {currency}  |  تاريخ الإنشاء: {generatedAt:yyyy-MM-dd HH:mm}")
                .FontSize(8.5f).FontColor(Colors.Grey.Darken2);

            if (!string.IsNullOrEmpty(dataWarning))
            {
                column.Item().PaddingTop(2).AlignCenter()
                    .Text(dataWarning).FontSize(8.5f).Bold().FontColor(Colors.Orange.Darken2);
            }
        });
    }

    public static void ComposeExternalFooter(IContainer footer)
    {
        footer.PaddingTop(5).Row(row =>
        {
            row.RelativeItem().AlignRight()
                .Text($"صادر عن: {ReportBranding.OrganizationName}")
                .FontSize(8f).FontColor(Colors.Grey.Darken1);
            row.AutoItem().PaddingHorizontal(10).AlignCenter()
                .Text(text =>
                {
                    text.Span("صفحة ").FontSize(8f).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(8f).FontColor(Colors.Grey.Darken1);
                    text.Span(" من ").FontSize(8f).FontColor(Colors.Grey.Darken1);
                    text.TotalPages().FontSize(8f).FontColor(Colors.Grey.Darken1);
                });
            row.RelativeItem();
        });
    }

    public static IContainer HeaderCellStyle(IContainer container)
    {
        return container.Background("#F3F4F6")
            .BorderBottom(1.5f).BorderColor(Colors.Grey.Darken2)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    public static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle();
    }

    public static IContainer NumberCellStyle(IContainer container)
    {
        return container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    public static IContainer FooterCellStyle(IContainer container)
    {
        return container.Background("#E5E7EB")
            .BorderTop(1.5f).BorderColor(Colors.Grey.Darken2)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }
}
