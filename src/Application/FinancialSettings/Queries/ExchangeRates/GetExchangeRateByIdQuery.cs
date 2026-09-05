using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;

// Q-F004 — GetExchangeRateByIdQuery
[Authorize(Policy = PermissionCodes.ExchangeRatesView)]
public class GetExchangeRateByIdQuery : IRequest<ExchangeRateDto?>
{
    public int Id { get; init; }
}

public class GetExchangeRateByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetExchangeRateByIdQuery, ExchangeRateDto?>
{
    public async Task<ExchangeRateDto?> Handle(
        GetExchangeRateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExchangeRates
            .Include(x => x.BaseCurrency)
            .Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<ExchangeRateDto>(entity);
    }
}
