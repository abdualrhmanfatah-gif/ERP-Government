using ERP_Government.Domain.FinancialSettings.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// FiscalPeriod seed data — 24 periods (12 months × 2 years).
/// </summary>
public static class FiscalPeriodSeedData
{
    public static List<FiscalPeriod> GetFiscalPeriods()
    {
        var periods = new List<FiscalPeriod>();
        

        // 2025 — all locked
        for (int m =1; m <=12; m++)
        {
            var start = new DateOnly(2025, m,1);
            var end = start.AddMonths(1).AddDays(-1);
            var name = GetMonthName(m) + " 2025";
            periods.Add(new() { FiscalYearId=1, PeriodNumber=m, Name=name, StartDate=start, EndDate=end, IsLockedForPosting=true });
        }

        // 2026 — all unlocked
        for (int m =1; m <=12; m++)
        {
            var start = new DateOnly(2026, m,1);
            var end = start.AddMonths(1).AddDays(-1);
            var name = GetMonthName(m) + " 2026";
            periods.Add(new() { FiscalYearId=2, PeriodNumber=m, Name=name, StartDate=start, EndDate=end, IsLockedForPosting=false });
        }

        return periods;
    }

    private static string GetMonthName(int month) => month switch
    {
       1 => "يناير",
       2 => "فبراير",
       3 => "مارس",
       4 => "أبريل",
       5 => "مايو",
       6 => "يونيو",
       7 => "يوليو",
       8 => "أغسطس",
       9 => "سبتمبر",
       10 => "أكتوبر",
       11 => "نوفمبر",
       12 => "ديسمبر",
        _ => ""
    };
}
