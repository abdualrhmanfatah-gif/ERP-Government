using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Common.Services;

public class ExchangeRateResolver(IApplicationDbContext context) : IExchangeRateResolver
{
    public async Task<EffectiveRate?> GetEffectiveRateAsync(
        int baseCurrencyId,
        int currencyId,
        DateOnly date,
        ExchangeRateType rateType,
        CancellationToken ct = default)
    {
        if (currencyId == baseCurrencyId)
            return new EffectiveRate(1m, date, true);

        var entity = await context.ExchangeRates
            .Where(x => x.BaseCurrencyId == baseCurrencyId
                && x.CurrencyId == currencyId
                && x.RateType == rateType
                && x.RateDate <= date
                && x.IsActive)
            .OrderByDescending(x => x.RateDate)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return null;

        return new EffectiveRate(entity.Rate, entity.RateDate, false);
    }
}
