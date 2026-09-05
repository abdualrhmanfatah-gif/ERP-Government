using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Egyptian Government Pension Fund — Chart of Accounts (دليل محاسبي)
/// AccountGroup seed data — 3 Levels: Root + Main + Sub
/// Seeded in 2 passes: roots first, then children with Code-based parent lookup.
/// </summary>
public static class AccountGroupSeedData
{
    public static List<AccountGroup> GetRootAccountGroups() =>
    [
        new() { Code="1",  Name="الموجودات",              Type=AccountGroupType.Asset,    NormalBalance=NormalBalanceType.Debit,  Level=1, Description="إجمالي الأصول — المشاريع الاستثمارية والأصول المالية والنقدية" },
        new() { Code="2",  Name="الموارد الرأسمالية",      Type=AccountGroupType.Equity,   NormalBalance=NormalBalanceType.Credit, Level=1, Description="إجمالي حقوق الملكية — رأس المال والاحتياطيات والأرباح المحتجزة" },
        new() { Code="3",  Name="الاستخدامات الجارية",     Type=AccountGroupType.Expense,  NormalBalance=NormalBalanceType.Debit,  Level=1, Description="إجمالي الاستخدامات — المصروفات الجارية" },
        new() { Code="4",  Name="الإيرادات",               Type=AccountGroupType.Revenue,  NormalBalance=NormalBalanceType.Credit, Level=1, Description="إجمالي الإيرادات — الموارد الجارية" },
    ];

    public static List<AccountGroup> GetChildAccountGroups() =>
    [
        // ═══════════════════════════════════════════════════════
        // Level 2 — Under 1 (الموجودات) — ParentCode="1"
        // ═══════════════════════════════════════════════════════
        new() { Code="12", Name="مشاريع قيد التنفيذ",          Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="1", Description="المشاريع الاستثمارية قيد التنفيذ" },
        new() { Code="13", Name="التوظيفات والاستثمارات",      Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="1", Description="حصص المشاركة واستثمارات مالية بسندات" },
        new() { Code="18", Name="الزيادة في الأموال الجاهزة",  Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="1", Description="نقد لدى البنوك وحسابات جارية محلية" },

        // ═══════════════════════════════════════════════════════
        // Level 2 — Under 2 (الموارد الرأسمالية) — ParentCode="2"
        // ═══════════════════════════════════════════════════════
        new() { Code="22", Name="الاحتياطيات والفائض",    Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2", Description="الفائض المرحل وفائض الدورة الحالية" },
        new() { Code="23", Name="المخصصات",               Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2", Description="مخصص الاهتلاكات ومخصص حقوق العاملين" },
        new() { Code="28", Name="حسابات النتائج",         Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2", Description="حساب التوزيع — عجز/فائض النشاط الجاري" },

        // ═══════════════════════════════════════════════════════
        // Level 2 — Under 3 (الاستخدامات الجارية) — ParentCode="3"
        // ═══════════════════════════════════════════════════════
        new() { Code="31", Name="المرتبات والأجور وما في حكمها",                    Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="3", Description="المرتبات والأجور النقدية والبدلات والتعويضات والمزايا العينية والمكافآت وتأمينات العاملين" },
        new() { Code="32", Name="مستلزمات الإنتاج ومشتريات بغرض البيع",            Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="3", Description="المستلزمات السلعية والخدمية ومشتريات بغرض البيع" },
        new() { Code="35", Name="المصروفات الجارية التحويلية والمخصصة",            Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=2, ParentCode="3", Description="الاهتلاك والإيجارات والفوائد والعمولات والتبرعات والإعانات" },

        // ═══════════════════════════════════════════════════════
        // Level 2 — Under 4 (الإيرادات) — ParentCode="4"
        // ═══════════════════════════════════════════════════════
        new() { Code="41", Name="إيرادات النشاط الجاري",                  Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="4", Description="إيرادات العمليات التجارية وقطاع الخدمات والاشتراكات" },
        new() { Code="42", Name="الإيرادات المتنوعة",                     Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="4", Description="إيرادات أخرى ومختلفة" },
        new() { Code="43", Name="إيرادات الأوراق المالية والعوائد",       Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="4", Description="إيرادات أوراق مالية محلية وعوائد محلية" },
        new() { Code="45", Name="الإيرادات الجارية التحويلية",            Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="4", Description="الإيجارات الدائنة" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 12 — ParentCode="12"
        // ═══════════════════════════════════════════════════════
        new() { Code="122", Name="المشاريع الاستثمارية",              Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="12", Description="المشاريع الاستثمارية — مباني وإنشاءات وآلات وأثاث" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 13 — ParentCode="13"
        // ═══════════════════════════════════════════════════════
        new() { Code="133", Name="حصص المشاركة",                        Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="13", Description="حصص المشاركة — مؤسسات وشركات محلية" },
        new() { Code="135", Name="استثمارات مالية بسندات",              Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="13", Description="استثمارات مالية بسندات حكومية" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 18 — ParentCode="18"
        // ═══════════════════════════════════════════════════════
        new() { Code="182", Name="نقد لدى البنوك",                     Type=AccountGroupType.Asset,   NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="18", Description="نقد لدى البنوك — حسابات جارية محلية" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 22 — ParentCode="22"
        // ═══════════════════════════════════════════════════════
        new() { Code="222", Name="الفائض المرحل",                     Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="22", Description="الفائض المرحل — فائض الدورة الحالية" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 23 — ParentCode="23"
        // ═══════════════════════════════════════════════════════
        new() { Code="231", Name="مخصص الاهتلاكات",                    Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="23", Description="مخصص اهتلاك المباني والآلات والسيارات والأثاث وحقوق العاملين" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 28 — ParentCode="28"
        // ═══════════════════════════════════════════════════════
        new() { Code="281", Name="حساب توزيع (عجز النشاط الجاري)",   Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="28", Description="حساب التوزيع — عجز النشاط الجاري" },
        new() { Code="282", Name="حساب توزيع (فائض النشاط الجاري)",  Type=AccountGroupType.Equity,  NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="28", Description="حساب التوزيع — فائض النشاط الجاري" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 31 — ParentCode="31"
        // ═══════════════════════════════════════════════════════
        new() { Code="311", Name="المرتبات والأجور النقدية",           Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="31", Description="مرتبات الموظفين الدائمين والموسمية" },
        new() { Code="312", Name="البدلات والتعويضات",                Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="31", Description="بدل حضور جلسات وتعويض المسؤولية والعمل الإضافي" },
        new() { Code="313", Name="المزايا العينية",                    Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="31", Description="مزايا عينية مختلفة أخرى" },
        new() { Code="314", Name="المكافآت",                           Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="31", Description="المكافآت التشجيعية" },
        new() { Code="316", Name="تأمينات وتقاعد العاملين",            Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="31", Description="تأمين الشيخوخة أو التقاعد" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 32 — ParentCode="32"
        // ═══════════════════════════════════════════════════════
        new() { Code="321", Name="المستلزمات السلعية",                 Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="32", Description="الوقود والزيوت والقوى المحركة والمياه وقطع التبديل والقرطاسية" },
        new() { Code="322", Name="المستلزمات الخدمية",                 Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="32", Description="الصيانة والتصليحات والأبحاث والنشر والإعلان والتنقلات والخدمات الحكومية" },
        new() { Code="323", Name="مشتريات بغرض البيع",                 Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="32", Description="مشتريات محلية بثمن التكلفة" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 35 — ParentCode="35"
        // ═══════════════════════════════════════════════════════
        new() { Code="351", Name="المصروفات الجارية التحويلية",       Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="35", Description="الاهتلاك والإيجارات والفوائد والعمولات" },
        new() { Code="352", Name="المصروفات المخصصة",                 Type=AccountGroupType.Expense, NormalBalance=NormalBalanceType.Debit,  Level=3, ParentCode="35", Description="التبرعات والإعانات والمساعدات والزكاة وأعباء المخصصات" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 41 — ParentCode="41"
        // ═══════════════════════════════════════════════════════
        new() { Code="412", Name="إيرادات العمليات التجارية",         Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="41", Description="مبيعات البضائع الجاهزة" },
        new() { Code="414", Name="إيرادات قطاع الخدمات",             Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="41", Description="إيرادات الاشتراكات — حصة الحكومة والموظفين في التقاعد ودعم الحكومة" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 43 — ParentCode="43"
        // ═══════════════════════════════════════════════════════
        new() { Code="431", Name="إيرادات أوراق مالية محلية",         Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="43", Description="إيرادات أوراق مالية محلية" },
        new() { Code="433", Name="إيرادات عوائد محلية",               Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="43", Description="إيرادات عوائد محلية" },

        // ═══════════════════════════════════════════════════════
        // Level 3 — Under 45 — ParentCode="45"
        // ═══════════════════════════════════════════════════════
        new() { Code="452", Name="الإيجارات الدائنة",                 Type=AccountGroupType.Revenue, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="45", Description="إيجار مباني في الداخل" },
    ];
}
