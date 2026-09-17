using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Opening-balance journal entries — posted Opening entries that
/// establish account balances at the start of a fiscal year.
/// </summary>
public static class JournalEntrySeedData
{
    /// <summary>
    /// Blueprint for a single opening-balance entry.
    /// </summary>
    public record OpeningBalanceBlueprint(
        int FiscalYearNumber,
        DateOnly DocumentDate,
        string Narration,
        IReadOnlyList<OpeningBalanceLine> Lines);

    /// <summary>
    /// One line of an opening-balance entry.
    /// </summary>
    public record OpeningBalanceLine(
        string AccountCode,
        decimal Debit,
        decimal Credit,
        string? Description = null);

    /// <summary>
    /// Opening balance for fiscal year 2026.
    /// Pension fund — assets, accumulated depreciation,
    /// liabilities, and carried-forward surplus.
    ///
    /// Accounting equation:
    ///   Assets = 830,430,000
    ///   Liabilities + Depreciation = 106,630,000
    ///   Net Assets (Surplus) = 723,800,000
    /// </summary>
    public static OpeningBalanceBlueprint OpeningBalance2026 => new(
        FiscalYearNumber: 2026,
        DocumentDate: new DateOnly(2026, 1, 1),
        Narration: "الأرصدة الافتتاحية — السنة المالية 2026",
        Lines:
        [
            // ═══════════════════════════════════════════════════════
            // أصول ثابتة (أرصدة مدينية)  =  815,430,000
            // ═══════════════════════════════════════════════════════
            new("1121", Debit:  480_000_000m, Credit: 0m,         "مباني الإدارة — صنعاء"),
            new("1122", Debit:  210_000_000m, Credit: 0m,         "مبانٍ استثمارية — عدن"),
            new("1141", Debit:   95_800_000m, Credit: 0m,         "سيارات ووسائل نقل"),
            new("1151", Debit:    3_600_000m, Credit: 0m,         "الأثاث والمفروشات"),
            new("1158", Debit:   16_950_000m, Credit: 0m,         "أجهزة الكمبيوتر وملحقاتها"),
            new("1159", Debit:      480_000m, Credit: 0m,         "معدات مكاتب أخرى"),
            new("1161", Debit:    6_500_000m, Credit: 0m,         "البرمجيات"),
            new("1162", Debit:    2_100_000m, Credit: 0m,         "التراخيص الرقمية"),

            // ═══════════════════════════════════════════════════════
            // أموال جاهزة (أرصدة مدينية)  =  15,000,000
            // ═══════════════════════════════════════════════════════
            new("1811", Debit:   15_000_000m, Credit: 0m,         "الصندوق المركزي الرئيسي"),

            // ═══════════════════════════════════════════════════════
            // مخصصات الاهتلاك (أرصدة دائنة)  =  95,900,000
            // ═══════════════════════════════════════════════════════
            new("2311", Debit: 0m, Credit:   45_000_000m,         "مخصص اهتلاك المباني والإنشاءات"),
            new("2312", Debit: 0m, Credit:    8_200_000m,         "مخصص اهتلاك الآلات والتجهيزات والمعدات"),
            new("2313", Debit: 0m, Credit:   38_000_000m,         "مخصص اهتلاك السيارات ووسائل النقل"),
            new("2314", Debit: 0m, Credit:    1_200_000m,         "مخصص اهتلاك الأثاث والمفروشات"),
            new("2316", Debit: 0m, Credit:    3_500_000m,         "مخصص اهتلاك الأصول غير الملموسة"),

            // ═══════════════════════════════════════════════════════
            // التزامات جارية (أرصدة دائنة)  =  10,730,000
            // ═══════════════════════════════════════════════════════
            new("2511", Debit: 0m, Credit:    5_000_000m,         "موردون محليون - قطاع عام"),
            new("2545", Debit: 0m, Credit:    2_500_000m,         "صندوق الضمان الاجتماعي"),
            new("2721", Debit: 0m, Credit:    3_230_000m,         "رواتب وأجور محلية مستحقة"),

            // ═══════════════════════════════════════════════════════
            // فائض مرحل — حقوق ملكية (رصيد دائن)  =  723,800,000
            // ═══════════════════════════════════════════════════════
            new("2822", Debit: 0m, Credit:  723_800_000m,         "فائض النشاط الجاري (مراقب مرحل)"),
        ]);

    /// <summary>
    /// All opening balance blueprints.
    /// </summary>
    public static IReadOnlyList<OpeningBalanceBlueprint> All =>
        [OpeningBalance2026];
}
