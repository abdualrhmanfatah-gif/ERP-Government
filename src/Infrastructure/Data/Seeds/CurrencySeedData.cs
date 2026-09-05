using ERP_Government.Domain.FinancialSettings.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Currency seed data — 3 currencies (YER base).
/// </summary>
public static class CurrencySeedData
{
    public static List<Currency> GetCurrencies() =>
    [
        new() { Code="YER", Name="Yemeni Riyal", Symbol="ر.ي", DecimalPlaces=2, RoundingPrecision=0.01m, IsBase=true },
        new() { Code="SAR", Name="Saudi Riyal", Symbol="ر.س", DecimalPlaces=2, RoundingPrecision=0.01m, IsBase=false },
        new() { Code="USD", Name="US Dollar", Symbol="$", DecimalPlaces=2, RoundingPrecision=0.01m, IsBase=false },
    ];
}
