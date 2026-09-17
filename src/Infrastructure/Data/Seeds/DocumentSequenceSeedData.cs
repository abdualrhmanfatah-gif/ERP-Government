using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// DocumentSequence seed data — 18 sequences.
/// </summary>
public static class DocumentSequenceSeedData
{
    public static List<DocumentSequence> GetDocumentSequences() =>
    [
        new() { Name="تسلسل القيود", DocumentType="JournalEntry", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل أوامر الشراء", DocumentType="PurchaseOrder", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل طلبات الشراء", DocumentType="PurchaseRequest", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل عروض الأسعار", DocumentType="Quotation", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل فواتير الموردين", DocumentType="SupplierInvoice", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل القيود الدورية", DocumentType="RecurringEntry", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الأصناف", DocumentType="Item", FiscalYearId=2, CurrentNumber=10, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المدفوعات", DocumentType="PaymentOrder", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل سندات القبض", DocumentType="ReceiptVoucher", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الإيصالات البنكية", DocumentType="DepositSlip", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل سندات الاستلام", DocumentType="GoodsReceiptNote", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل جرد المخزون", DocumentType="StockTake", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الموازنات", DocumentType="Budget", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المعاملات المالية", DocumentType="BudgetTransaction", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل القيود المحجوزة", DocumentType="Encumbrance", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل طلبات الصرف", DocumentType="DisbursementRequest", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المدفوعات المنفذة", DocumentType="Payment", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الأطراف", DocumentType="Party", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المطالبات الإيرادية", DocumentType="RevenueClaim", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل أوامر التحصيل", DocumentType="CollectionOrder", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل حوافظ توريد النقد 47", DocumentType="DepositSlip47", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل حوافظ الشيكات 48", DocumentType="DepositSlip48", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },

        // ═══════════════════════════════════════════════════════════════
        // Asset Management (DEP-030)
        // ═══════════════════════════════════════════════════════════════
        new() { Name="تسلسل الأصول", DocumentType="Asset", FiscalYearId=2, CurrentNumber=100, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل نقل الأصول", DocumentType="AssetTransfer", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل تخلص الأصول", DocumentType="AssetDisposal", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل إعادة تقييم الأصول", DocumentType="AssetRevaluation", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل هبوط قيمة الأصول", DocumentType="AssetImpairment", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل عمليات إهلاك الأصول", DocumentType="DepreciationRun", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل جرد الأصول الفعلي", DocumentType="AssetPhysicalCount", FiscalYearId=2, CurrentNumber=50, ResetPolicy=ResetPolicy.Yearly },
    ];
}
