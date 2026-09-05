using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.Currencies;

// Q-F002 — GetCurrencyByIdQuery
[Authorize(Policy = PermissionCodes.CurrenciesView)]
public class GetCurrencyByIdQuery : IRequest<CurrencyDto?>
{
    public int Id { get; init; }
}

public class GetCurrencyByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto?>
{
    public async Task<CurrencyDto?> Handle(
        GetCurrencyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Currencies
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<CurrencyDto>(entity);
    }
}
