using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;

// Q-F005 — LookupExchangeRateQuery
[Authorize(Policy = PermissionCodes.ExchangeRatesView)]
public class LookupExchangeRateQuery : IRequest<ExchangeRateLookupDto?>
{
    public int BaseCurrencyId { get; init; }
    public int CurrencyId { get; init; }
    public DateOnly Date { get; init; }
    public ExchangeRateType RateType { get; init; }
}

public class LookupExchangeRateQueryHandler(
    IApplicationDbContext context) : IRequestHandler<LookupExchangeRateQuery, ExchangeRateLookupDto?>
{
    public async Task<ExchangeRateLookupDto?> Handle(
        LookupExchangeRateQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExchangeRates
            .Where(x => x.BaseCurrencyId == request.BaseCurrencyId
                && x.CurrencyId == request.CurrencyId
                && x.RateType == request.RateType
                && x.RateDate <= request.Date
                && x.IsActive)
            .OrderByDescending(x => x.RateDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return null;

        return new ExchangeRateLookupDto
        {
            Rate = entity.Rate,
            RateDate = entity.RateDate,
            RateType = entity.RateType.ToString()
        };
    }
}
