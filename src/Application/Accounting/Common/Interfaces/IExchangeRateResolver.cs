using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Common.Interfaces;

public interface IExchangeRateResolver
{
    Task<EffectiveRate?> GetEffectiveRateAsync(
        int baseCurrencyId,
        int currencyId,
        DateOnly date,
        ExchangeRateType rateType,
        CancellationToken ct = default);
}

public sealed record EffectiveRate(decimal Rate, DateOnly ResolvedRateDate, bool IsDefaultBase);
