using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;

// Q-F003 — GetExchangeRatesQuery
[Authorize(Policy = PermissionCodes.ExchangeRatesView)]
public class GetExchangeRatesQuery : IRequest<List<ExchangeRateDto>>
{
    public int? CurrencyId { get; init; }
    public ExchangeRateType? RateType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public bool? IsActive { get; init; }
}

public class GetExchangeRatesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetExchangeRatesQuery, List<ExchangeRateDto>>
{
    public async Task<List<ExchangeRateDto>> Handle(
        GetExchangeRatesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ExchangeRates
            .Include(x => x.BaseCurrency)
            .Include(x => x.Currency)
            .AsQueryable();

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        if (request.RateType.HasValue)
            query = query.Where(x => x.RateType == request.RateType.Value);

        if (request.FromDate.HasValue)
            query = query.Where(x => x.RateDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(x => x.RateDate <= request.ToDate.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderByDescending(x => x.RateDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ExchangeRateDto>>(items);
    }
}
