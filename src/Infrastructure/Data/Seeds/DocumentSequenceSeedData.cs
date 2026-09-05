using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// DocumentSequence seed data — 11 sequences.
/// </summary>
public static class DocumentSequenceSeedData
{
    public static List<DocumentSequence> GetDocumentSequences() =>
    [
        new() { Name="تسلسل القيود", DocumentType="JournalEntry", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل أوامر الشراء", DocumentType="PurchaseOrder", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل طلبات الشراء", DocumentType="PurchaseRequest", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل عروض الأسعار", DocumentType="RequestForQuotation", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المدفوعات", DocumentType="PaymentOrder", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الإيرادات", DocumentType="RevenueReceipt", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل سندات الاستلام", DocumentType="GoodsReceiptNote", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل جرد المخزون", DocumentType="StockTake", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل الموازنات", DocumentType="Budget", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل التخصيصات", DocumentType="Appropriation", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل القيود المحجوزة", DocumentType="Encumbrance", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل طلبات الصرف", DocumentType="DisbursementRequest", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
        new() { Name="تسلسل المدفوعات المنفذة", DocumentType="Payment", FiscalYearId=2, CurrentNumber=1, ResetPolicy=ResetPolicy.Yearly },
    ];
}
