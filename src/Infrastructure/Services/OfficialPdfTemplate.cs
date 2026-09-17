using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public static class OfficialPdfTemplate
{
    private const float RepublicImageHeight = 20;
    private const float MinistryImageHeight = 20;
    private const float LogoHeight = 70;
    private const float OrganizationFontSize = 10;
    private const float DepartmentFontSize = 9;
    private const float InfoFontSize = 9;
    private const float TitleFontSize = 13;
    private const float MetaFontSize = 8.5f;
    private const float FooterFontSize = 8f;
    private const float HeaderBorderWidth = 1.5f;
    private const float CellBorderWidth = 1f;

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
                row.RelativeItem(1.2f).PaddingRight(5).Column(rightCol =>
                {
                    rightCol.Spacing(2);

                    if (ReportBranding.RepublicHeaderPath is not null
                        && File.Exists(ReportBranding.RepublicHeaderPath))
                        rightCol.Item().AlignCenter().Height(RepublicImageHeight)
                            .Image(ReportBranding.RepublicHeaderPath).FitArea();

                    if (ReportBranding.MinistryHeaderPath is not null
                        && File.Exists(ReportBranding.MinistryHeaderPath))
                        rightCol.Item().AlignCenter().Height(MinistryImageHeight)
                            .Image(ReportBranding.MinistryHeaderPath).FitArea();

                    if (!string.IsNullOrWhiteSpace(ReportBranding.OrganizationName))
                        rightCol.Item().AlignCenter().Text(ReportBranding.OrganizationName)
                            .FontSize(OrganizationFontSize).Bold().FontColor(Colors.Black);

                    if (!string.IsNullOrWhiteSpace(ReportBranding.DepartmentName))
                        rightCol.Item().AlignCenter().Text(ReportBranding.DepartmentName)
                            .FontSize(DepartmentFontSize).Bold().FontColor(Colors.Black);
                });

                row.RelativeItem(0.8f).Column(centerCol =>
                {
                    if (ReportBranding.LogoPath is not null
                        && File.Exists(ReportBranding.LogoPath))
                        centerCol.Item().AlignCenter().Height(LogoHeight)
                            .Image(ReportBranding.LogoPath).FitArea();
                });

                row.RelativeItem(1f).Column(leftCol =>
                {
                    leftCol.Item().AlignLeft().Column(info =>
                    {
                        info.Spacing(4);
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("التاريخ : ").FontSize(InfoFontSize).Bold();
                            r.RelativeItem().AlignRight()
                                .Text($"{generatedAt:yyyy/MM/dd}م").FontSize(InfoFontSize).Bold();
                        });
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("الرقم : ").FontSize(InfoFontSize).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2)
                                .BorderBottom(1).BorderColor(Colors.Black)
                                .Text(orderNumber ?? string.Empty);
                        });
                        info.Item().Row(r =>
                        {
                            r.AutoItem().Text("المرجع : ").FontSize(InfoFontSize).Bold();
                            r.RelativeItem().AlignBottom().PaddingBottom(2)
                                .BorderBottom(1).BorderColor(Colors.Black);
                        });
                    });
                });
            });

            column.Item().PaddingTop(4)
                .LineHorizontal(1).LineColor(Colors.Black);

            column.Item().PaddingTop(6).AlignCenter()
                .Text(reportName).FontSize(TitleFontSize).Bold().FontColor(Colors.Black);

            column.Item().PaddingTop(2).AlignCenter()
                .Text($"العملة: {currency}  |  تاريخ الإنشاء: {generatedAt:yyyy-MM-dd HH:mm}")
                .FontSize(MetaFontSize).FontColor(Colors.Black);

            if (!string.IsNullOrEmpty(dataWarning))
            {
                column.Item().PaddingTop(2).AlignCenter()
                    .Text(dataWarning).FontSize(MetaFontSize).Bold().FontColor(Colors.Black);
            }
        });
    }

    public static void ComposeExternalFooter(IContainer footer)
    {
        footer.PaddingTop(5).Row(row =>
        {
            row.RelativeItem().AlignRight()
                .Text($"صادر عن: {ReportBranding.OrganizationName}")
                .FontSize(FooterFontSize).FontColor(Colors.Black);
            row.AutoItem().PaddingHorizontal(10).AlignCenter()
                .Text(text =>
                {
                    text.Span("صفحة ").FontSize(FooterFontSize).FontColor(Colors.Black);
                    text.CurrentPageNumber().FontSize(FooterFontSize).FontColor(Colors.Black);
                    text.Span(" من ").FontSize(FooterFontSize).FontColor(Colors.Black);
                    text.TotalPages().FontSize(FooterFontSize).FontColor(Colors.Black);
                });
            row.RelativeItem();
        });
    }

    public static IContainer HeaderCellStyle(IContainer container)
    {
        return container.Background("#F3F4F6")
            .BorderBottom(HeaderBorderWidth).BorderColor(Colors.Black)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    public static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(CellBorderWidth).BorderColor(Colors.Black)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle();
    }

    public static IContainer NumberCellStyle(IContainer container)
    {
        return container.BorderBottom(CellBorderWidth).BorderColor(Colors.Black)
            .PaddingVertical(4).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }

    public static IContainer FooterCellStyle(IContainer container)
    {
        return container.Background("#E5E7EB")
            .BorderTop(HeaderBorderWidth).BorderColor(Colors.Black)
            .PaddingVertical(5).PaddingHorizontal(5).AlignMiddle().AlignCenter();
    }
}
