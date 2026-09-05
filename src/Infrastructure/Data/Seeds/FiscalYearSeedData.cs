using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// FiscalYear seed data — 2 years.
/// </summary>
public static class FiscalYearSeedData
{
    public static List<FiscalYear> GetFiscalYears() =>
    [
        new() { Name="السنة المالية 2025", YearNumber=2025, StartDate=new DateOnly(2025,1,1), EndDate=new DateOnly(2025,12,31), Status=FiscalYearStatus.HardClosed, IsClosed=true },
        new() { Name="السنة المالية 2026", YearNumber=2026, StartDate=new DateOnly(2026,1,1), EndDate=new DateOnly(2026,12,31), Status=FiscalYearStatus.Open, IsClosed=false },
    ];
}
