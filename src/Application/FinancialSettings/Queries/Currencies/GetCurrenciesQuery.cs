using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.Currencies;

// Q-F001 — GetCurrenciesQuery
[Authorize(Policy = PermissionCodes.CurrenciesView)]
public class GetCurrenciesQuery : IRequest<List<CurrencyDto>>
{
    public bool? IsActive { get; init; }
}

public class GetCurrenciesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCurrenciesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> Handle(
        GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Currencies.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<CurrencyDto>>(items);
    }
}
