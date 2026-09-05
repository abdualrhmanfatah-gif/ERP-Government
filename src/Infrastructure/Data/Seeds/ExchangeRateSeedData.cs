using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// ExchangeRate seed data — 3 rates (YER base).
/// YER/SAR ≈ 65, YER/USD ≈ 250, SAR/USD ≈ 3.85
/// </summary>
public static class ExchangeRateSeedData
{
    public static List<ExchangeRate> GetExchangeRates() =>
    [
        new() { BaseCurrencyId=1, CurrencyId=2, RateDate=new DateOnly(2026,1,1), RateType=ExchangeRateType.Official, Rate=65.00m },
        new() { BaseCurrencyId=1, CurrencyId=3, RateDate=new DateOnly(2026,1,1), RateType=ExchangeRateType.Official, Rate=250.00m },
        new() { BaseCurrencyId=2, CurrencyId=3, RateDate=new DateOnly(2026,1,1), RateType=ExchangeRateType.Official, Rate=3.85m },
    ];
}
