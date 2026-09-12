# خطة التنفيذ: طباعة أمر صرف من الصندوق PDF (المُنقَّحة)

## المشكلة

لا يوجد تصدير PDF لأوامر الدفع. المطلوب وثيقة PDF رسمية تتبع القالب الرسمي الحالي (ترويسة يمنية + تذييل) مع محتوى مخصص لأمر الصرف.

## القرار المعماري

### إعادة استخدام الترويسة

`PdfReportExporter` يحتوي على `ComposeOfficialHeader` و`ComposeExternalFooter` كدوال private. لا يمكن استخدامها خارج الملف.

**القرار:** تعديل `PdfReportExporter.cs` refactor فقط — استخراج الترويسة/Tذييل/التنسيق إلى `OfficialPdfTemplate` مشترك. لا تغيير على المخرجات البصرية. الهدف: منع نسخ الترويسة والتعرض للاختلاف مستقبلي.

**الخطوات:**
1. إنشاء `OfficialPdfTemplate.cs` — الترويسة + التذييل + التنسيقات
2. تعديل `PdfReportExporter.cs` — delegates إلى `OfficialPdfTemplate` (نفس المخرجات، refactor فقط)
3. `PaymentOrderPdfExporter.cs` يستخدم `OfficialPdfTemplate` مباشرة

**النتيجة:** ترويسة متطابقة بصرياً، لا تكرار كود، لا اختلاف مستقبلي. `PdfReportExporter.cs` يخضع لـ refactor فقط — لا تغيير في المخرجات البصرية.

### اتجاه الاعتماد

```
Application (Query + DTO)  ←  لا يعتمد على Infrastructure
Infrastructure (Exporter + ArabicNumberToWords)  ←  يعتمد على Application
Web (Endpoint)  ←  يعتمد على Application + Infrastructure
```

- `ArabicNumberToWords` في Infrastructure (ليس Application)
- `NetAmountInWords` لا EXISTS في DTO — يحسب في Exporter مباشرة

---

## الملفات المطلوبة

### Backend (7 ملفات)

| # | الملف | العملية |
|---|---|---|
| 1 | `src/Infrastructure/Services/OfficialPdfTemplate.cs` | **إنشاء** — ترويسة + تذييل مشترك |
| 2 | `src/Infrastructure/Services/ArabicNumberToWords.cs` | **إنشاء** — تحويل أرقام عربي |
| 3 | `src/Application/Payments/Common/DTOs/PaymentOrderPrintDto.cs` | **إنشاء** — DTO للطباعة |
| 4 | `src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderPrint/GetPaymentOrderPrintQuery.cs` | **إنشاء** — Query + Handler |
| 5 | `src/Infrastructure/Services/PaymentOrderPdfExporter.cs` | **إنشاء** — مُصدِّر PDF |
| 6 | `src/Infrastructure/Services/PdfReportExporter.cs` | **refactor** — delegates إلى OfficialPdfTemplate (لا تغيير بصري) |
| 7 | `src/Web/Endpoints/Payments/PaymentOrders.cs` | **تعديل** — إضافة export endpoint |

### Frontend (2 ملفات تعديل)

| # | الملف | التعديل |
|---|---|---|
| 8 | `src/Web/ClientApp/src/features/payments/payment-orders/shared/client.ts` | إضافة export URL |
| 9 | `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx` | زر طباعة |

---

## 1. OfficialPdfTemplate.cs (جديد)

**المسار:** `src/Infrastructure/Services/OfficialPdfTemplate.cs`

قالب مشترك يستخدمه كل من PdfReportExporter و PaymentOrderPdfExporter.

```csharp
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
                // اليمين — الجهة والوزارة
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

                // الوسط — الشعار
                row.RelativeItem(0.8f).Column(centerCol =>
                {
                    if (ReportBranding.LogoPath is not null)
                        centerCol.Item().AlignCenter().Height(50)
                            .Image(ReportBranding.LogoPath).FitArea();
                });

                // اليسار — بيانات التوثيق
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

            // خطوط فصل مزدوجة
            column.Item().PaddingTop(6)
                .Element(e => e.BorderBottom(1.5f).BorderColor(PrimaryAccentColor));
            column.Item().PaddingTop(2)
                .Element(e => e.BorderBottom(0.5f).BorderColor(PrimaryAccentColor));

            // عنوان التقرير
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

    // ── تنسيقات خلايا الجدول المشتركة ──

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
```

---

## 2. ArabicNumberToWords.cs (جديد)

**المسار:** `src/Infrastructure/Services/ArabicNumberToWords.cs`

**مهم:** تُعيد الأرقام بالحروف فقط — **بدون** "فقط" أو اسم العملة أو "لا غير". القالب يتولى الإحاطة.

```csharp
namespace ERP_Government.Infrastructure.Services;

public static class ArabicNumberToWords
{
    private static readonly string[] Units = ["صفر", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"];
    private static readonly string[] Teens = ["عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"];
    private static readonly string[] Tens = ["", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"];
    private static readonly string[] Hundreds = ["", "مئة", "مئتان", "ثلاثمئة", "أربعمئة", "خمسمئة", "ستمئة", "سبعمئة", "ثمانمئة", "تسعمئة"];

    public static string Convert(decimal amount)
    {
        if (amount == 0) return "صفر";

        var integerPart = (long)Math.Abs(amount);
        var fractionPart = (int)(Math.Abs(amount) * 100) % 100;

        var result = ConvertInteger(integerPart);

        if (fractionPart > 0)
        {
            result += $" و{fractionPart}/100";
        }

        return result;
    }

    private static string ConvertInteger(long number)
    {
        if (number == 0) return "صفر";

        var parts = new List<string>();

        if (number >= 1_000_000_000)
        {
            var billions = number / 1_000_000_000;
            number %= 1_000_000_000;
            parts.Add(billions switch
            {
                1 => "مليار",
                2 => "ملياران",
                <= 10 => $"{ConvertBelow1000(billions)} ملايين",
                _ => $"{ConvertBelow1000(billions)} مليار"
            });
        }

        if (number >= 1_000_000)
        {
            var millions = number / 1_000_000;
            number %= 1_000_000;
            parts.Add(millions switch
            {
                1 => "مليون",
                2 => "مليونان",
                <= 10 => $"{ConvertBelow1000(millions)} ملايين",
                _ => $"{ConvertBelow1000(millions)} مليون"
            });
        }

        if (number >= 1000)
        {
            var thousands = number / 1000;
            number %= 1000;
            parts.Add(thousands switch
            {
                1 => "ألف",
                2 => "ألفان",
                <= 10 => $"{ConvertBelow1000(thousands)} آلاف",
                _ => $"{ConvertBelow1000(thousands)} ألف"
            });
        }

        if (number > 0)
        {
            parts.Add(ConvertBelow1000(number));
        }

        return string.Join(" و", parts);
    }

    private static string ConvertBelow1000(long number)
    {
        var parts = new List<string>();

        if (number >= 100)
        {
            var hundreds = number / 100;
            number %= 100;
            parts.Add(Hundreds[hundreds]);
        }

        if (number >= 20)
        {
            var tens = number / 10;
            var units = number % 10;
            parts.Add(units > 0 ? $"{Units[units]} و{Tens[tens]}" : Tens[tens]);
        }
        else if (number >= 10)
        {
            parts.Add(Teens[number - 10]);
        }
        else if (number > 0)
        {
            parts.Add(Units[number]);
        }

        return string.Join(" و", parts);
    }
}
```

**أمثلة:**
- `1234.50` → "ألف ومئتين وأربعة وثلاثون و50/100"
- `1000000` → "مليون"
- `500.25` → "خمسمئة و25/100"

---

## 3. PaymentOrderPrintDto.cs (جديد)

**المسار:** `src/Application/Payments/Common/DTOs/PaymentOrderPrintDto.cs`

```csharp
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Common.DTOs;

public class PaymentOrderPrintDto
{
    // بيانات أمر الصرف
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string OrderType { get; init; } = string.Empty;
    public PaymentOrderStatus Status { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string? DisbursementRequestNumber { get; init; }

    // السنة المالية
    public string FiscalYearName { get; init; } = string.Empty;
    public int FiscalYearNumber { get; init; }

    // ملخص المبالغ
    public decimal AmountGross { get; init; }
    public decimal TaxDeductions { get; init; }
    public decimal OtherDeductions { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;

    // المستفيد
    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }

    // الغرض
    public string Purpose { get; init; } = string.Empty;

    // المرفقات
    public int AttachmentsCount { get; init; }

    // التوجيه المحاسبي
    public string FundCode { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public string? ClassificationCode { get; init; }
    public string? ClassificationName { get; init; }
    public string? AccountCode { get; init; }
    public string? AccountName { get; init; }
    public string? CostCenterCode { get; init; }
    public string? CostCenterName { get; init; }

    // بيانات الدفع (تأخذ من Payment أولاً، ثم PaymentOrder كبديل)
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? PaymentReferenceNumber { get; init; }
    public string? PaymentNumber { get; init; }
    public DateTimeOffset? PaidAt { get; init; }

    // الاعتماد
    public string? CreatedByName { get; init; }
    public DateTimeOffset Created { get; init; }     // BaseEntity.Created (ليس CreatedAt)
    public string? ApproverName { get; init; }
    public string? RequiredRole { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? PaidByName { get; init; }

    // حالة
    public bool IsUnapproved { get; init; }
}
```

**ملاحظات:**
- `Created`来自 `BaseAuditableEntity.Created` (ليس `CreatedAt`)
- لا يوجد `NetAmountInWords` — يحسب في Exporter
- `Purpose`永远有 قيمة (fallback إلى "—" في Handler)

---

## 4. GetPaymentOrderPrintQuery.cs (جديد)

**المسار:** `src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderPrint/GetPaymentOrderPrintQuery.cs`

### Query

```csharp
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;
using MediatR;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderPrint;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public record GetPaymentOrderPrintQuery : IRequest<PaymentOrderPrintDto?>
{
    public int Id { get; init; }
}
```

### Handler

```csharp
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderPrint;

internal class GetPaymentOrderPrintQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetPaymentOrderPrintQuery, PaymentOrderPrintDto?>
{
    private static readonly Dictionary<PaymentOrderStatus, string> StatusLabels = new()
    {
        [PaymentOrderStatus.Draft] = "مسودة",
        [PaymentOrderStatus.Submitted] = "مرسلة",
        [PaymentOrderStatus.Approved] = "موافق عليها",
        [PaymentOrderStatus.SentToTreasury] = "مرسلة للخزينة",
        [PaymentOrderStatus.Paid] = "مدفوعة",
        [PaymentOrderStatus.Cancelled] = "ملغاة",
        [PaymentOrderStatus.Rejected] = "مرفوضة",
        [PaymentOrderStatus.Voided] = "ملغاة نهائياً"
    };

    public async Task<PaymentOrderPrintDto?> Handle(
        GetPaymentOrderPrintQuery request,
        CancellationToken cancellationToken)
    {
        // 1. جلب أمر الصرف
        var order = await context.PaymentOrders
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (order is null) return null;

        // 2. جلب الاستقطاعات
        var deductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == request.Id)
            .ToListAsync(cancellationToken);

        // 3. الكيانات المرتبطة (JOINs يدوية — لا nav properties)
        var fund = await context.Funds.FindAsync(new object[] { order.FundId }, cancellationToken);
        var currency = await context.Currencies.FindAsync(new object[] { order.CurrencyId }, cancellationToken);
        var classification = order.BudgetClassificationId.HasValue
            ? await context.BudgetClassifications.FindAsync(new object[] { order.BudgetClassificationId.Value }, cancellationToken)
            : null;
        var account = order.AccountId.HasValue
            ? await context.Accounts.FindAsync(new object[] { order.AccountId.Value }, cancellationToken)
            : null;
        var costCenter = order.CostCenterId.HasValue
            ? await context.CostCenters.FindAsync(new object[] { order.CostCenterId.Value }, cancellationToken)
            : null;

        // 4. السنة المالية
        var fiscalYear = await context.FiscalYears.FindAsync(new object[] { order.FiscalYearId }, cancellationToken);

        // 5. طلب الصرف المرتبط
        DisbursementRequest? dr = null;
        if (order.DisbursementRequestId.HasValue)
        {
            dr = await context.DisbursementRequests
                .FirstOrDefaultAsync(x => x.Id == order.DisbursementRequestId.Value,
                    cancellationToken);
        }

        // 6. أحدث دفع مسجل (يأخذ الأولوية في طريقة الدفع)
        Payment? payment = null;
        if (order.Status is PaymentOrderStatus.Paid or PaymentOrderStatus.SentToTreasury)
        {
            payment = await context.Payments
                .Where(p => p.PaymentOrderId == request.Id
                         && p.Status == PaymentStatus.Completed)
                .OrderByDescending(p => p.PaidAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // 7. أحدث اعتماد ناجح — projection مباشر للحصول على اسم المستخدم
        var latestApproval = await context.ApprovalHistory
            .Where(a => a.DocumentType == "PaymentOrder"
                     && a.DocumentId == request.Id
                     && a.Action == ApprovalAction.Approve)
            .OrderByDescending(a => a.DecisionAt)
            .Select(a => new
            {
                ApproverName = a.ApproverUser.Login,
                a.RequiredRole,
                a.DecisionAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 8. اسم المنشئ — resolution من Users
        string? createdByName = null;
        if (int.TryParse(order.CreatedBy, out var createdByUserId))
        {
            var creator = await context.Users.FindAsync(new object[] { createdByUserId }, cancellationToken);
            createdByName = creator?.Login;
        }

        // 9. عدد المرفقات
        var attachmentsCount = await context.Attachments
            .CountAsync(a => a.DocumentType == "PaymentOrder"
                          && a.DocumentId == request.Id,
                    cancellationToken);

        // 10. حساب المجاميع
        var taxDeductions = deductions.Where(d => d.IsTaxDeduction).Sum(d => d.Amount);
        var otherDeductions = deductions.Where(d => !d.IsTaxDeduction).Sum(d => d.Amount);
        var totalDeductions = deductions.Sum(d => d.Amount);
        var netAmount = order.AmountGross - totalDeductions;

        // 11. طريقة الدفع — Payment أولاً، ثم PaymentOrder كبديل
        var paymentMethod = payment?.PaymentMethod ?? order.PaymentMethod;
        var paymentMethodName = paymentMethod switch
        {
            PaymentMethod.Cash => "نقدي",
            PaymentMethod.Check => "شيك",
            _ => "—"
        };

        // 12. الغرض — DisbursementRequest.Purpose أولاً، ثم Notes
        var purpose = dr?.Purpose ?? order.Notes;
        if (string.IsNullOrWhiteSpace(purpose)) purpose = "—";

        // 13. حالة غير معتمدة
        var isUnapproved = order.Status is not
            (PaymentOrderStatus.Approved or PaymentOrderStatus.SentToTreasury or PaymentOrderStatus.Paid);

        return new PaymentOrderPrintDto
        {
            OrderNumber = order.PaymentOrderNumber,
            OrderDate = order.PaymentOrderDate,
            DueDate = order.DueDate,
            OrderType = order.PaymentOrderType,
            Status = order.Status,
            StatusLabel = StatusLabels.GetValueOrDefault(order.Status, order.Status.ToString()),
            DisbursementRequestNumber = dr?.RequestNumber,
            FiscalYearName = fiscalYear?.Name ?? "—",
            FiscalYearNumber = fiscalYear?.YearNumber ?? 0,
            AmountGross = order.AmountGross,
            TaxDeductions = taxDeductions,
            OtherDeductions = otherDeductions,
            TotalDeductions = totalDeductions,
            NetAmount = netAmount,
            CurrencyCode = currency?.Code ?? "YER",
            CurrencyName = currency?.Name ?? "ريال يمني",
            BeneficiaryName = order.BeneficiaryName,
            BeneficiaryAccountNumber = order.BeneficiaryAccountNumber,
            BeneficiaryBankName = order.BeneficiaryBankName,
            Purpose = purpose,
            AttachmentsCount = attachmentsCount,
            FundCode = fund?.FundNumber ?? "—",
            FundName = fund?.FundName ?? "—",
            ClassificationCode = classification?.Code,
            ClassificationName = classification?.Name,
            AccountCode = account?.Code,
            AccountName = account?.Name,
            CostCenterCode = costCenter?.Code,
            CostCenterName = costCenter?.Name,
            PaymentMethodName = paymentMethodName,
            PaymentReferenceNumber = payment?.ReferenceNumber,
            PaymentNumber = payment?.PaymentNumber,
            PaidAt = payment?.PaidAt,
            CreatedByName = createdByName,
            Created = order.Created,
            ApproverName = latestApproval?.ApproverName,
            RequiredRole = latestApproval?.RequiredRole,
            ApprovedAt = latestApproval?.DecisionAt,
            PaidByName = payment?.PaidByName,
            IsUnapproved = isUnapproved
        };
    }
}
```

### DbContext المطلوبة (كلها موجودة)

```csharp
// في IApplicationDbContext:
DbSet<PaymentOrder> PaymentOrders
DbSet<PaymentOrderDeduction> PaymentOrderDeductions
DbSet<Payment> Payments
DbSet<DisbursementRequest> DisbursementRequests
DbSet<Fund> Funds
DbSet<Currency> Currencies
DbSet<BudgetClassification> BudgetClassifications
DbSet<Account> Accounts
DbSet<CostCenter> CostCenters
DbSet<FiscalYear> FiscalYears
DbSet<ApprovalHistory> ApprovalHistory  // مع nav ApproverUser
DbSet<User> Users
DbSet<Attachment> Attachments
```

---

## 5. PaymentOrderPdfExporter.cs (جديد)

**المسار:** `src/Infrastructure/Services/PaymentOrderPdfExporter.cs`

```csharp
using ERP_Government.Application.Payments.Common.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP_Government.Infrastructure.Services;

public class PaymentOrderPdfExporter
{
    private const int MaxPurposeLength = 120;
    private const int MaxBeneficiaryLength = 40;

    public Task ExportAsync(PaymentOrderPrintDto dto, Stream outputStream, CancellationToken ct = default)
    {
        var amountInWords = ArabicNumberToWords.Convert(dto.NetAmount);

        // قص النصوص الطويلة لضمان صفحة واحدة
        var purpose = Truncate(dto.Purpose, MaxPurposeLength);
        var beneficiary = Truncate(dto.BeneficiaryName, MaxBeneficiaryLength);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.MarginHorizontal(15);
                page.MarginVertical(15);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(s => s.FontFamily("Calibri").FontSize(9));

                // ترويسة رسمية مشتركة
                page.Header().Element(h => OfficialPdfTemplate.ComposeOfficialHeader(
                    h,
                    reportName: "أمر صرف من الصندوق",
                    currency: $"{dto.CurrencyCode} — {dto.CurrencyName}",
                    generatedAt: dto.Created,
                    dataWarning: dto.IsUnapproved
                        ? $"نسخة غير معتمدة — حالة الأمر: {dto.StatusLabel}"
                        : null,
                    orderNumber: dto.OrderNumber));

                // محتوى أمر الصرف
                page.Content().PaddingVertical(8).Column(content =>
                {
                    ComposeOrderInfo(content, dto);
                    ComposeAmountSummary(content, dto, amountInWords);
                    ComposeNarrativeText(content, dto, amountInWords, beneficiary, purpose);
                    ComposeAccountingDirection(content, dto);
                    ComposePaymentData(content, dto);
                    ComposeSignatures(content, dto);
                    ComposeReceiptAcknowledge(content, dto, amountInWords, purpose);
                });

                // تذييل مشترك
                page.Footer().Element(OfficialPdfTemplate.ComposeExternalFooter);
            });
        });

        document.GeneratePdf(outputStream);
        return Task.CompletedTask;
    }

    /// <summary>
    /// قص النص إلىطول محدد مع "..." في النهاية
    /// </summary>
    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return "—";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    /// <summary>
    /// عرض قيمة في جدول — قص إذا كانت طويلة، "—" إذا فارغة
    /// </summary>
    private static string Display(string? value, int maxLength = 25)
    {
        if (string.IsNullOrWhiteSpace(value)) return "—";
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    // ══════════════════════════════════════════════
    // بيانات أمر الصرف — صفوف مضغوطة
    // ══════════════════════════════════════════════
    private static void ComposeOrderInfo(ColumnDescriptor content, PaymentOrderPrintDto dto)
    {
        content.Item().PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("رقم الأمر: ").FontSize(9).Bold();
                    t.Span(dto.OrderNumber).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("السنة المالية: ").FontSize(9).Bold();
                    t.Span($"{dto.FiscalYearName} ({dto.FiscalYearNumber})").FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("حالة الأمر: ").FontSize(9).Bold();
                    t.Span(dto.StatusLabel).FontSize(9);
                });
            });
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("تاريخ الأمر: ").FontSize(9).Bold();
                    t.Span($"{dto.OrderDate:yyyy/MM/dd}").FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("نوع الأمر: ").FontSize(9).Bold();
                    t.Span(dto.OrderType).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("طلب الصرف المرتبط: ").FontSize(9).Bold();
                    t.Span(dto.DisbursementRequestNumber ?? "—").FontSize(9);
                });
            });
        });
    }

    // ══════════════════════════════════════════════
    // ملخص المبالغ — جدول صف واحد
    // ══════════════════════════════════════════════
    private static void ComposeAmountSummary(ColumnDescriptor content, PaymentOrderPrintDto dto, string amountInWords)
    {
        content.Item().PaddingBottom(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2f);
            });

            table.Header(header =>
            {
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("إجمالي المبلغ").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("الضرائب").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("استقطاعات أخرى").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("إجمالي الاستقطاعات").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("صافي المبلغ").Bold();
            });

            table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text($"{dto.AmountGross:N2}");
            table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text($"{dto.TaxDeductions:N2}");
            table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text($"{dto.OtherDeductions:N2}");
            table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text($"{dto.TotalDeductions:N2}");
            table.Cell().Element(OfficialPdfTemplate.NumberCellStyle).Text($"{dto.NetAmount:N2}").Bold();
        });

        content.Item().PaddingTop(1).PaddingBottom(1)
            .Text($"صافي المبلغ كتابةً: فقط {amountInWords} {dto.CurrencyName} لا غير.")
            .FontSize(8.5f).Bold();
    }

    // ══════════════════════════════════════════════
    // نص أمر الصرف
    // ══════════════════════════════════════════════
    private static void ComposeNarrativeText(ColumnDescriptor content, PaymentOrderPrintDto dto, string amountInWords, string beneficiary, string purpose)
    {
        content.Item().PaddingBottom(4).Column(section =>
        {
            section.Item().Text("إلى الأخ/ أمين الصندوق المحترم،").FontSize(9).LineHeight(1.4f);
            section.Item().PaddingTop(2).Text(t =>
            {
                t.Span("يرجى صرف صافي مبلغ قدره ").FontSize(9).LineHeight(1.4f);
                t.Span($"{dto.NetAmount:N2}").FontSize(9).Bold().LineHeight(1.4f);
                t.Span($"، فقط {amountInWords} {dto.CurrencyName} لا غير،").FontSize(9).LineHeight(1.4f);
            });
            section.Item().Text(t =>
            {
                t.Span("للمستفيد/ ").FontSize(9).LineHeight(1.4f);
                t.Span(beneficiary).FontSize(9).Bold().LineHeight(1.4f);
                t.Span($"، وذلك مقابل/ {purpose}،").FontSize(9).LineHeight(1.4f);
            });
            section.Item().Text(t =>
            {
                t.Span($"بالمستندات المؤيدة المرفقة وعددها ({dto.AttachmentsCount})،").FontSize(9).LineHeight(1.4f);
            });
            section.Item().Text("على أن يُحمّل المبلغ على الحساب والتصنيف المالي الموضحين أدناه،")
                .FontSize(9).LineHeight(1.4f);
            section.Item().Text("مع أخذ توقيع المستفيد بما يفيد الاستلام.")
                .FontSize(9).LineHeight(1.4f);
        });
    }

    // ══════════════════════════════════════════════
    // التوجيه المحاسبي
    // ══════════════════════════════════════════════
    private static void ComposeAccountingDirection(ColumnDescriptor content, PaymentOrderPrintDto dto)
    {
        content.Item().PaddingBottom(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
            });

            table.Header(header =>
            {
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("الصندوق").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("التصنيف المالي").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("الحساب").Bold();
                header.Cell().Element(OfficialPdfTemplate.HeaderCellStyle).Text("مركز التكلفة").Bold();
            });

            table.Cell().Element(OfficialPdfTemplate.CellStyle)
                .Text($"{Display(dto.FundCode)} — {Display(dto.FundName, 20)}");
            table.Cell().Element(OfficialPdfTemplate.CellStyle)
                .Text(dto.ClassificationCode is not null ? $"{Display(dto.ClassificationCode)} — {Display(dto.ClassificationName, 20)}" : "—");
            table.Cell().Element(OfficialPdfTemplate.CellStyle)
                .Text(dto.AccountCode is not null ? $"{Display(dto.AccountCode)} — {Display(dto.AccountName, 20)}" : "—");
            table.Cell().Element(OfficialPdfTemplate.CellStyle)
                .Text(dto.CostCenterCode is not null ? $"{Display(dto.CostCenterCode)} — {Display(dto.CostCenterName, 20)}" : "—");
        });
    }

    // ══════════════════════════════════════════════
    // بيانات الدفع — صف أو اثنين فقط
    // ══════════════════════════════════════════════
    private static void ComposePaymentData(ColumnDescriptor content, PaymentOrderPrintDto dto)
    {
        content.Item().PaddingBottom(4).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("طريقة الدفع: ").FontSize(9).Bold();
                    t.Span(Display(dto.PaymentMethodName)).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("رقم حساب المستفيد: ").FontSize(9).Bold();
                    t.Span(Display(dto.BeneficiaryAccountNumber)).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("رقم الدفع: ").FontSize(9).Bold();
                    t.Span(Display(dto.PaymentNumber)).FontSize(9);
                });
            });
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("البنك: ").FontSize(9).Bold();
                    t.Span(Display(dto.BeneficiaryBankName)).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("رقم المرجع: ").FontSize(9).Bold();
                    t.Span(Display(dto.PaymentReferenceNumber)).FontSize(9);
                });
                col.Item().PaddingBottom(1).Text(t =>
                {
                    t.Span("تاريخ التنفيذ: ").FontSize(9).Bold();
                    t.Span(dto.PaidAt?.ToString("yyyy/MM/dd") ?? "—").FontSize(9);
                });
            });
        });
    }

    // ══════════════════════════════════════════════
    // الاعتماد والتنفيذ — 3 أعمدة توقيع
    // ══════════════════════════════════════════════
    private static void ComposeSignatures(ColumnDescriptor content, PaymentOrderPrintDto dto)
    {
        content.Item().PaddingBottom(4).Row(row =>
        {
            // مُعدّ الأمر
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text("مُعدّ الأمر").Bold().FontSize(9);
                col.Item().PaddingBottom(1).Text(dto.CreatedByName ?? "—").FontSize(8);
                col.Item().PaddingBottom(1).Text($"{dto.Created:yyyy/MM/dd}").FontSize(8);
                col.Item().PaddingBottom(1).Text("التوقيع: ................").FontSize(8);
            });

            // المعتمد
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text("المعتمد").Bold().FontSize(9);
                col.Item().PaddingBottom(1).Text(dto.ApproverName ?? "—").FontSize(8);
                col.Item().PaddingBottom(1).Text(
                    dto.RequiredRole is not null ? $"الدور: {dto.RequiredRole}" : "—").FontSize(8);
                col.Item().PaddingBottom(1).Text(
                    dto.ApprovedAt?.ToString("yyyy/MM/dd") ?? "—").FontSize(8);
                col.Item().PaddingBottom(1).Text("التوقيع: ................").FontSize(8);
            });

            // أمين الصندوق
            row.RelativeItem().Column(col =>
            {
                col.Item().PaddingBottom(1).Text("أمين الصندوق/منفذ الدفع").Bold().FontSize(9);
                col.Item().PaddingBottom(1).Text(dto.PaidByName ?? "—").FontSize(8);
                col.Item().PaddingBottom(1).Text(
                    dto.PaidAt?.ToString("yyyy/MM/dd") ?? "—").FontSize(8);
                col.Item().PaddingBottom(1).Text("التوقيع: ................").FontSize(8);
            });
        });
    }

    // ══════════════════════════════════════════════
    // إقرار الاستلام
    // ══════════════════════════════════════════════
    private static void ComposeReceiptAcknowledge(ColumnDescriptor content, PaymentOrderPrintDto dto, string amountInWords, string purpose)
    {
        content.Item().PaddingBottom(2).Column(section =>
        {
            section.Item().PaddingBottom(2)
                .Text("إقرار استلام").Bold().FontSize(10)
                .FontColor(OfficialPdfTemplate.PrimaryAccentColor);

            section.Item().Text("أقرّ أنا/ ................................................ بأنني استلمت مبلغاً قدره:")
                .FontSize(8.5f).LineHeight(1.4f);

            section.Item().PaddingTop(1).Text(t =>
            {
                t.Span("رقماً: ").FontSize(8.5f);
                t.Span($"{dto.NetAmount:N2} {dto.CurrencyName}").FontSize(8.5f).Bold();
            });

            section.Item().Text(t =>
            {
                t.Span("كتابةً: فقط ").FontSize(8.5f);
                t.Span($"{amountInWords} {dto.CurrencyName}").FontSize(8.5f).Bold();
                t.Span(" لا غير.").FontSize(8.5f);
            });

            section.Item().Text(t =>
            {
                t.Span("وذلك مقابل: ").FontSize(8.5f);
                t.Span(purpose).FontSize(8.5f);
            });

            section.Item().PaddingTop(2).Row(r =>
            {
                r.RelativeItem().Text("اسم المستلم: ................................").FontSize(8.5f);
                r.RelativeItem().Text("تاريخ الاستلام: ....../....../..........").FontSize(8.5f);
            });

            section.Item().PaddingTop(1)
                .Text("التوقيع/البصمة: ........................................").FontSize(8.5f);
        });
    }
}
```

---

## 6. تعديل PdfReportExporter.cs (minimal)

**الهدف:** delegates header/footer/style إلى `OfficialPdfTemplate` المشترك.

**التغييرات:**
1. احذف `ComposeOfficialHeader` → استبدل بـ `OfficialPdfTemplate.ComposeOfficialHeader`
2. احذف `ComposeExternalFooter` → استبدل بـ `OfficialPdfTemplate.ComposeExternalFooter`
3. احذف `HeaderCellStyle`, `CellStyle`, `NumberCellStyle`, `FooterCellStyle` → استبدل بـ `OfficialPdfTemplate.*`
4. احذف `PrimaryAccentColor` → استبدل بـ `OfficialPdfTemplate.PrimaryAccentColor`

**النتيجة:** `PdfReportExporter` يصبح أبسط، نفس المخرجات بصرياً.

---

## 7. تعديل PaymentOrders.cs (Endpoint)

**إضافة export endpoint إلى المجموعة الحالية:**

```csharp
// أضف في Map():
groupBuilder.MapGet("/{id:int}/export-pdf", ExportPaymentOrderPdf)
    .RequireAuthorization(PermissionCodes.PaymentOrdersView);

// أضف Handler:
[EndpointSummary("Export payment order as PDF")]
public static async Task<IResult> ExportPaymentOrderPdf(
    [FromServices] ISender sender,
    [FromServices] PaymentOrderPdfExporter exporter,
    int id)
{
    var dto = await sender.Send(new GetPaymentOrderPrintQuery { Id = id });
    if (dto is null) return Results.NotFound();

    var stream = new MemoryStream();
    await exporter.ExportAsync(dto, stream);
    stream.Position = 0;

    return Results.File(stream, "application/pdf",
        $"PaymentOrder-{dto.OrderNumber}.pdf");
}
```

**المسار الناتج:** `GET /api/PaymentOrders/{id}/export-pdf` — متوافق مع الواجهة.

---

## 8. تعديل DI Registration

**في `src/Infrastructure/DependencyInjection.cs`:**

```csharp
// أضف بعد سطر 95:
builder.Services.AddScoped<ERP_Government.Infrastructure.Services.PaymentOrderPdfExporter>();
```

---

## 9. Frontend Changes

### client.ts

```ts
// أضف في paymentOrdersClient:
exportPaymentOrderPdf: (id: number) =>
  `/api/PaymentOrders/${id}/export-pdf`,
```

### PaymentOrderDetailPage.tsx

```tsx
// Imports — أضف:
import { Printer } from 'lucide-react';
import { downloadBlobExport } from '@/shared/utils/download';

// State for loading/error:
const [exporting, setExporting] = useState(false);

// Handler:
async function handleExportPdf() {
  setExporting(true);
  try {
    const url = paymentOrdersClient.exportPaymentOrderPdf(orderId);
    await downloadBlobExport(url, `PaymentOrder-${order.paymentOrderNumber ?? orderId}.pdf`);
  } catch {
    notify({ type: 'error', title: 'فشل تنزيل ملف PDF' });
  } finally {
    setExporting(false);
  }
}

// Button — في headerActions:
<Button
  variant="secondary"
  size="sm"
  onClick={handleExportPdf}
  disabled={exporting}
  loading={exporting}
>
  <Printer size={14} className="ms-1" />
  طباعة أمر الصرف
</Button>
```

**ملاحظات RTL:**
- `ms-1` (margin-start) بدلاً من `ml-1` — متوافق مع RTL
- `Printer` icon بدلاً من `Download` — يناسب "طباعة"
- `disabled` + `loading` — منع الضغط المتكرر
- `try/catch` — رسالة خطأ عربية

---

## ضمان الصفحة الواحدة

**استراتيجية إجبارية (لا تعتمد على QuestPDF pagination):**

1. **قص النصوص الطويلة** قبل الرسم:
   - `BeneficiaryName` → 40 حرفmaximum مع "..."
   - `Purpose` → 120 حرفmaximum مع "..."
   - قيم الجداول (اسم الحساب، مركز التكلفة، البنك، etc.) → 25 حرف عبر `Display()` helper
   - يتم في Exporter عبر `Truncate()` و`Display()` helpers
2. **خط 9pt** + هوامش 15mm
3. **مسافات رأسية محدودة** (PaddingBottom 1-4)
4. **جدول الملخص صف واحد فقط**
5. **النصوص التعريفية line-height 1.4f**
6. **لا تفاصيل أسطر الاستقطاعات** — مجاميع فقط
7. **التوقيعات وإقرار الاستلام بخط 8-8.5pt**

**يكفي المحتوى صفحة واحدة:**
بيانات + ملخص + نص + توجيه + دفع + توقيع + إقرار — كلها بخط 9pt + مسافات محدودة + نصوص مقصوصة = أقل من A4.

**لا صفحتين:** `Truncate()` يقص النص قبل الرسم. QuestPDF لا ينشئ صفحة ثانية لأن content محدد مسبقاً.

---

## التحقق

```bash
# Backend
dotnet build src/Web/Web.csproj

# Frontend
cd src/Web/ClientApp
npm run generate-api
npm run lint
npm run build
```

### التحقق اليدوي

#### أولاً: التحقق من التقارير الموجودة بعد refactor (قبل أمر الصرف)

بما أن `PdfReportExporter.cs` يخضع لـ refactor، يجب التحقق من أن التقارير PDF الحالية لا تزال تعمل بنفس المخرجات:

- [ ] **Trial Balance:** `GET /api/TrialBalanceReports/export?format=pdf&FiscalYearId=1`
  - الترويسة متطابقة (وزارة الداخلية + شعار + خطوط خضراء)
  - التذييل متطابق (صادر عن + صفحة)
  - الجدول يظهر بتنسيق صحيح
- [ ] **Budget Execution:** `GET /api/BudgetExecutionReports/export?format=pdf&FiscalYearId=1`
  - نفس الفحص
- [ ] **Balance Sheet / Income Statement / General Ledger / Cash Flow:** `GET /api/Reports/{reportType}/export?format=pdf`
  - فحص عشوائي لتقرير واحد على الأقل

**الشرط:** إذا تغير أي تقرير موجود بصرياً، revert refactor وابحث عن بديل.

#### ثانياً: التحقق من أمر الصرف PDF

يجب فتح ملف PDF والتحقق من:
- [ ] الترويسة الرسمية اليمنية (وزارة الداخلية + شعار + خطوط خضراء)
- [ ] التذييل (صادر عن + صفحة X من Y)
- [ ] رقم الأمر في أعلى اليسار
- [ ] "نسخة غير معتمدة" للأوامر غير المعتمدة
- [ ] ملخص المبالغ: إجمالي + ضرائب + استقطاعات أخرى + إجمالي الاستقطاعات + صافي
- [ ] المبلغ بالحروف عربياً (فقط ... لا غير)
- [ ] نص أمر الصرف مع المستفيد والغرض وعدد المرفقات
- [ ] التوجيه المحاسبي: صندوق + تصنيف + حساب + مركز تكلفة (قيم اختيارية = "—")
- [ ] بيانات الدفع: طريقة + بنك + حساب + مرجع + رقم دفع + تاريخ
- [ ] الاعتماد: مُعدّ + معتمد + أمين صندوق (أسماء + تواريخ + مسافات توقيع)
- [ ] إقرار الاستلام: مساحة اسم + تاريخ + توقيع/بصمة
- [ ] عدد صفحات PDF = صفحة واحدة
- [ ] لا تتكرر الترويسة داخل المحتوى
- [ ] المبالغ صحيحة (Math check)

---

## ضوابط الحالة

- **يسمح بالطباعة في جميع الحالات** (Draft, Submitted, Approved, etc.)
- **للأوامر غير المعتمدة:** تظهر "نسخة غير معتمدة — حالة الأمر: [الحالة]" في الترويسة
- **لا تتغير حالة أمر الدفع عند الطباعة**

---

## ما يتغير وما لا يتغير

### يتغير (refactor فقط — لا تغيير في المخرجات البصرية)
- `PdfReportExporter.cs` — refactor: delegates header/footer/style إلى `OfficialPdfTemplate` المشترك. المخرجات البصرية متطابقة 100%.

### لا يتغير
- `ReportBranding.cs` — لا تعديل
- أي entity — لا Migration
- أي اختبار — لا تعديل
