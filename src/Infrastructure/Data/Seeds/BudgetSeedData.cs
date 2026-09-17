using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Budget + BudgetItem seed data — 2 budgets with hierarchical items.
/// FundId/BudgetTypeId/FiscalYearId set after insert via code lookup.
/// </summary>
public static class BudgetSeedData
{
    public static List<Budget> GetBudgets() =>
    [
        new()
        {
            BudgetNumber = "BUD-2026-001",
            BudgetName = "موازنة المرتبات والأجور 2026",
            Status = BudgetStatus.Draft,
            AllowOverrun = false,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            Description = "موازنة التسيير للسنة المالية 2026",
        },
        new()
        {
            BudgetNumber = "BUD-2026-002",
            BudgetName = "موازنة الاستثمارات 2026",
            Status = BudgetStatus.Draft,
            AllowOverrun = false,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            Description = "موازنة المشاريع الاستثمارية 2026",
        },
    ];

    public static List<BudgetItem> GetBudgetItems() =>
    [
        // ── Budget 1: المرتبات والأجور ──
        // Level 1
        new() { ItemCode = "1000", ItemName = "المرتبات الأساسية", IsActive = true },
        new() { ItemCode = "2000", ItemName = "البدلات والمنح", IsActive = true },
        new() { ItemCode = "3000", ItemName = "العمليات التشغيلية", IsActive = true },

        // Level 2 — المرتبات
        new() { ItemCode = "1100", ItemName = "مرتبات الموظفين الدائمين", IsActive = true },
        new() { ItemCode = "1200", ItemName = "مرتبات الموظفين المؤقتين", IsActive = true },

        // Level 2 — البدلات
        new() { ItemCode = "2100", ItemName = "بدلات سكن", IsActive = true },
        new() { ItemCode = "2200", ItemName = "بدلات نقل", IsActive = true },
        new() { ItemCode = "2300", ItemName = "مكافآت نهاية الخدمة", IsActive = true },

        // Level 2 — عمليات تشغيلية
        new() { ItemCode = "3100", ItemName = "الوقود والصيانة", IsActive = true },
        new() { ItemCode = "3200", ItemName = "الكتب والمطبوعات", IsActive = true },
        new() { ItemCode = "3300", ItemName = "الإيجارات", IsActive = true },

        // Level 3
        new() { ItemCode = "1110", ItemName = "رواتب الإدارة العليا", IsActive = true },
        new() { ItemCode = "1120", ItemName = "رواتب الموظفين الفنيين", IsActive = true },
        new() { ItemCode = "3110", ItemName = "وقود المركبات", IsActive = true },
        new() { ItemCode = "3120", ItemName = "صيانة المباني", IsActive = true },

        // ── Budget 2: الاستثمارات ──
        new() { ItemCode = "INV-1000", ItemName = "مشاريع البنية التحتية", IsActive = true },
        new() { ItemCode = "INV-2000", ItemName = "تجهيز المكاتب", IsActive = true },
        new() { ItemCode = "INV-3000", ItemName = "الأنظمة التقنية", IsActive = true },

        new() { ItemCode = "INV-1100", ItemName = "مشروع تجديد المبنى الإداري", IsActive = true },
        new() { ItemCode = "INV-1200", ItemName = "مشروع إنشاء قاعة مؤتمرات", IsActive = true },
        new() { ItemCode = "INV-2100", ItemName = "شراء أثاث مكتبي", IsActive = true },
        new() { ItemCode = "INV-3100", ItemName = "شراء حواسيب وأجهزة", IsActive = true },
        new() { ItemCode = "INV-3200", ItemName = "شبكة الإنترنت والاتصالات", IsActive = true },
    ];

    public static Dictionary<string, string> GetAccountMapping() =>
        new()
        {
            // Budget 1: المرتبات والأجور
            ["1100"] = "3111",
            ["1200"] = "3111",
            ["2100"] = "3124",
            ["2200"] = "32252",
            ["2300"] = "3141",
            ["3100"] = "3212",
            ["3200"] = "3215",
            ["3300"] = "35136",
            ["1110"] = "3111",
            ["1120"] = "3111",
            ["3110"] = "32122",
            ["3120"] = "32211",
            // Budget 2: الاستثمارات
            ["INV-1000"] = "1222",
            ["INV-2000"] = "115",
            ["INV-3000"] = "1161",
            ["INV-1100"] = "12221",
            ["INV-1200"] = "12222",
            ["INV-2100"] = "1151",
            ["INV-3100"] = "1158",
            ["INV-3200"] = "1161",
        };
}
