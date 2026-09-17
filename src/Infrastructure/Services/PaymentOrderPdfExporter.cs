using ERP_Government.Application.Payments.Common.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public class PaymentOrderPdfExporter
{
    private const float BaseFontSize = 11f;
    private const float SmallFontSize = 9.5f;

    public Task ExportAsync(PaymentOrderPrintDto dto, Stream outputStream, CancellationToken ct = default)
    {
        var amountInWords = ArabicNumberToWords.Convert(dto.NetAmount);
        var beneficiary = Display(dto.BeneficiaryName);
        var purpose = Display(dto.Purpose);
        var budgetItem = dto.BudgetItemCode is not null || dto.BudgetItemName is not null
            ? $"{Display(dto.BudgetItemCode)} / {Display(dto.BudgetItemName)}"
            : "—";
        var accountText = dto.AccountCode is not null || dto.AccountName is not null
            ? $"{Display(dto.AccountCode)} / {Display(dto.AccountName)}"
            : "—";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.MarginHorizontal(28);
                page.MarginVertical(20);
                page.PageColor("#F5F5F5");
                page.Background().Layers(layers =>
                {
                    layers.PrimaryLayer().Padding(5, Unit.Millimetre)
                        .Border(0.5f, Unit.Millimetre)
                        .BorderColor(Colors.Black);

                    if (ReportBranding.LogoPath is not null)
                    {
                        var watermark = WatermarkHelper.LoadWithTransparency(ReportBranding.LogoPath, 0.08f);
                        layers.Layer().AlignCenter().AlignMiddle()
                            .Height(150).Image(watermark).FitArea();
                    }
                });
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(style => style
                    .FontFamily("Calibri")
                    .FontSize(BaseFontSize)
                    .FontColor(Colors.Black));

                page.Header().Element(h => OfficialPdfTemplate.ComposeOfficialHeader(
                    h,
                    reportName: "أمر صرف من الصندوق",
                    currency: $"{dto.CurrencyCode} — {dto.CurrencyName}",
                    generatedAt: dto.Created,
                    dataWarning: dto.IsUnapproved
                        ? $"نسخة غير معتمدة — حالة الأمر: {dto.StatusLabel}"
                        : null,
                    orderNumber: dto.OrderNumber));

                page.Content().PaddingTop(8).Column(content =>
                {
                    content.Spacing(10);

                    content.Item().Element(section => ComposeSummaryBoxes(section, dto, budgetItem));
                    content.Item().Element(section => ComposeNarrative(section, dto, amountInWords, beneficiary, purpose, accountText));
                    content.Item().PaddingTop(12).Element(section => ComposeSignatures(section, dto));
                    content.Item().PaddingTop(8).Element(section => ComposeReceipt(section, dto, amountInWords));
                });

                page.Footer().Element(OfficialPdfTemplate.ComposeExternalFooter);
            });
        });

        document.GeneratePdf(outputStream);
        return Task.CompletedTask;
    }

    private static void ComposeSummaryBoxes(IContainer container, PaymentOrderPrintDto dto, string budgetItem)
    {
        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Element(item => ComposeLabeledBox(item, "المبلغ", $"{dto.AmountGross:N2}"));
            row.RelativeItem().Element(item => ComposeLabeledBox(item, "الضريبة", $"{dto.TaxDeductions:N2}"));
            row.RelativeItem().Element(item => ComposeLabeledBox(item, "صافي المبلغ", $"{dto.NetAmount:N2}"));
            row.RelativeItem().Element(item => ComposeLabeledBox(item, "بند الميزانية", budgetItem));
        });
    }

    private static void ComposeLabeledBox(IContainer container, string label, string value)
    {
        container.Column(column =>
        {
            column.Spacing(3);

            column.Item()
                .Border(1.2f)
                .BorderColor(Colors.Black)
                .PaddingVertical(4)
                .PaddingHorizontal(4)
                .AlignCenter()
                .MinHeight(18)
                .Text(label)
                .Bold()
                .FontSize(9f);

            column.Item()
                .Border(1.2f)
                .BorderColor(Colors.Black)
                .PaddingVertical(5)
                .PaddingHorizontal(4)
                .AlignCenter()
                .AlignMiddle()
                .MinHeight(22)
                .Text(Display(value, 60))
                .FontSize(9f);
        });
    }

    private static void ComposeNarrative(
        IContainer container,
        PaymentOrderPrintDto dto,
        string amountInWords,
        string beneficiary,
        string purpose,
        string accountText)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().AlignRight().Text("الأخ / أمين الصندوق").Bold();
                row.ConstantItem(90).AlignLeft().Text("المحترم").Bold();
            });

            column.Item().PaddingTop(8).AlignCenter().Text(text =>
            {
                text.Span("يتم صرف صافي المبلغ وقدره ").FontSize(BaseFontSize);
                text.Span($"{dto.NetAmount:N2}").FontSize(BaseFontSize).Bold();
                text.Span(" ( ").FontSize(BaseFontSize);
                text.Span(amountInWords).FontSize(BaseFontSize).Bold();
                text.Span(" ) ").FontSize(BaseFontSize);
                text.Span(dto.CurrencyName).FontSize(BaseFontSize).Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("للأخ/الأخت / ").FontSize(BaseFontSize);
                text.Span(beneficiary).FontSize(BaseFontSize).Bold();
                text.Span("    مقابل / ").FontSize(BaseFontSize);
                text.Span(purpose).FontSize(BaseFontSize).Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("بموجب الأوليات المرفقة عدد ( ").FontSize(BaseFontSize);
                text.Span(dto.AttachmentsCount.ToString()).FontSize(BaseFontSize).Bold();
                text.Span(" )، مع أخذ استلام بذلك بحسب النظام.").FontSize(BaseFontSize);
            });

            column.Item().Text(text =>
            {
                text.Span("على أن يوجه صرفه من ح / ").FontSize(BaseFontSize);
                text.Span(accountText).FontSize(BaseFontSize).Bold();
            });

            if (dto.AccrualJournalEntryId.HasValue)
            {
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("قيد الاستحقاق / ").FontSize(SmallFontSize);
                    text.Span(dto.AccrualEntryNumber ?? dto.AccrualJournalEntryId.ToString()!).FontSize(SmallFontSize).Bold();
                });
                if (dto.AccrualExpenseAccountCode is not null)
                {
                    column.Item().Text(text =>
                    {
                        text.Span("الحساب المدين / ").FontSize(SmallFontSize);
                        text.Span($"{dto.AccrualExpenseAccountCode} — {dto.AccrualExpenseAccountName}").FontSize(SmallFontSize);
                    });
                }
                if (dto.AccrualLiabilityAccountCode is not null)
                {
                    column.Item().Text(text =>
                    {
                        text.Span("الحساب الدائن / ").FontSize(SmallFontSize);
                        text.Span($"{dto.AccrualLiabilityAccountCode} — {dto.AccrualLiabilityAccountName}").FontSize(SmallFontSize);
                    });
                }
                if (dto.AccrualAmount.HasValue)
                {
                    column.Item().Text(text =>
                    {
                        text.Span("مبلغ القيد / ").FontSize(SmallFontSize);
                        text.Span($"{dto.AccrualAmount:N2} {dto.CurrencyName}").FontSize(SmallFontSize).Bold();
                    });
                }
            }

            column.Item().PaddingTop(4)
                .AlignCenter()
                .Text("مرسل للتنفيذ.")
                .Bold()
                .FontSize(10.5f);
        });
    }

    private static void ComposeSignatures(IContainer container, PaymentOrderPrintDto dto)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.Spacing(10);

                row.RelativeItem().Element(item => ComposeSignatureRole(item,
                    "المختص",
                    Display(dto.CreatedByName),
                    null));

                row.RelativeItem().Element(item => ComposeSignatureRole(item,
                    "المراجع",
                    "........................",
                    null));

                row.RelativeItem().Element(item => ComposeSignatureRole(item,
                    "المعتمد",
                    Display(dto.ApproverName),
                    dto.RequiredRole));

                row.RelativeItem().Element(item => ComposeSignatureRole(item,
                    "أمين الصندوق",
                    Display(dto.PaidByName),
                    dto.PaidAt?.ToString("yyyy/MM/dd")));
            });
        });
    }

    private static void ComposeSignatureRole(IContainer container, string role, string name, string? subtitle)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            column.Item().AlignCenter().Text(role).Bold().FontSize(8.6f);
            column.Item().AlignCenter().Text(name).FontSize(8f);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                column.Item().AlignCenter().Text(subtitle).FontSize(7.4f);
            }
        });
    }

    private static void ComposeReceipt(IContainer container, PaymentOrderPrintDto dto, string amountInWords)
    {
        container.Column(column =>
        {
            column.Spacing(6);

            column.Item().BorderTop(1f).BorderColor(Colors.Black).PaddingTop(8)
                .Text(text =>
                {
                    text.Span("استلمت من الأخ/ أمين الصندوق مبلغًا وقدره: ").FontSize(10).Bold();
                    text.Span($"{dto.NetAmount:N2} {dto.CurrencyName}").FontSize(10).Bold();
                    text.Span(" ( ").FontSize(10);
                    text.Span(amountInWords).FontSize(10).Bold();
                    text.Span(" )").FontSize(10);
                });

            column.Item().Text("اسم المستلم: __________________________________________").Bold();
            column.Item().Text("جهة القبض: _____________________________________________").Bold();
            column.Item().Text("التوقيع / البصمة: ______________________________________").Bold();
        });
    }

    private static string Display(string? value, int maxLength = 40)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "—";

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength] + "...";
    }
}
