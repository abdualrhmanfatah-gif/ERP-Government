using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;

// Q-F004 — GetExchangeRateByIdQuery
[Authorize(Policy = PermissionCodes.ExchangeRatesView)]
public class GetExchangeRateByIdQuery : IRequest<Result<ExchangeRateDto>>
{
    public int Id { get; init; }
}

public class GetExchangeRateByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetExchangeRateByIdQuery, Result<ExchangeRateDto>>
{
    public async Task<Result<ExchangeRateDto>> Handle(
        GetExchangeRateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExchangeRates
            .Include(x => x.BaseCurrency)
            .Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<ExchangeRateDto>.Failure(ErrorCodes.FinancialSettings.ExchangeRateNotFound, ErrorCategory.NotFound, $"Exchange rate with ID {request.Id} not found.");

        return Result<ExchangeRateDto>.Success(mapper.Map<ExchangeRateDto>(entity));
    }
}
