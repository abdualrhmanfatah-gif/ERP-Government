using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// PostingRule seed data — 10 rules.
/// </summary>
public static class PostingRuleSeedData
{
    public static List<PostingRule> GetPostingRules() =>
    [
        new() { Name="ترحيل أمر الشراء", EventType="PurchaseOrderApproved", JournalId=2, Priority=1 },
        new() { Name="ترحيل سند الاستلام", EventType="GoodsReceiptNoteApproved", JournalId=2, Priority=2 },
        new() { Name="ترحيل المدفوعات", EventType="PaymentOrderExecuted", JournalId=4, Priority=1 },
        new() { Name="ترحيل الإيداعات", EventType="BankReconciliationPosted", JournalId=5, Priority=1 },
        new() { Name="ترحيل سندات الإيراد", EventType="ReceiptVoucherCollected", JournalId=3, Priority=1 },
        new() { Name="ترحيل القيود العامة", EventType="JournalEntryPosted", JournalId=1, Priority=1 },
        new() { Name="ترحيل هلاك الأصول", EventType="DepreciationPosted", JournalId=6, Priority=1 },
        new() { Name="ترحيل إقفال السنة", EventType="FiscalYearClosed", JournalId=7, Priority=1 },
        new() { Name="ترحيل حركات المخزون", EventType="StockTransactionPosted", JournalId=6, Priority=1 },
        new() { Name="ترحيل تصفية الأصول", EventType="AssetDisposed", JournalId=6, Priority=1 },
        new() { Name="ترحيل إعادة تقييم الأصول", EventType="AssetRevalued", JournalId=6, Priority=1 },
        new() { Name="ترحيل هلاك الأصول", EventType="AssetImpaired", JournalId=6, Priority=1 },
    ];
}
