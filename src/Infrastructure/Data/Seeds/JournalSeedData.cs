using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Journal seed data — 7 journals.
/// </summary>
public static class JournalSeedData
{
    public static List<Journal> GetJournals() =>
    [
        new() { Code="JR-GEN", Name="الدفتر العام", Type=JournalType.General, AllowForeignCurrency=false, RequireApprovalBeforePosting=false },
        new() { Code="JR-PUR", Name="دفتر المشتريات", Type=JournalType.Purchase, AllowForeignCurrency=false, RequireApprovalBeforePosting=false },
        new() { Code="JR-REV", Name="دفتر الإيرادات", Type=JournalType.Sale, AllowForeignCurrency=false, RequireApprovalBeforePosting=false },
        new() { Code="JR-PAY", Name="دفتر المدفوعات", Type=JournalType.Cash, AllowForeignCurrency=false, RequireApprovalBeforePosting=true },
        new() { Code="JR-BNK", Name="الدفتر البنكي", Type=JournalType.Bank, AllowForeignCurrency=true, RequireApprovalBeforePosting=true },
        new() { Code="JR-ADJ", Name="دفتر التعديلات", Type=JournalType.Adjustment, AllowForeignCurrency=true, RequireApprovalBeforePosting=true },
        new() { Code="JR-CLS", Name="دفتر الإقفال", Type=JournalType.Closing, AllowForeignCurrency=false, RequireApprovalBeforePosting=true },
    ];
}
