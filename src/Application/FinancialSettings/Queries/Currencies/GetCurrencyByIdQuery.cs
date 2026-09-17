using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.Currencies;

// Q-F002 — GetCurrencyByIdQuery
[Authorize(Policy = PermissionCodes.CurrenciesView)]
public class GetCurrencyByIdQuery : IRequest<Result<CurrencyDto>>
{
    public int Id { get; init; }
}

public class GetCurrencyByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCurrencyByIdQuery, Result<CurrencyDto>>
{
    public async Task<Result<CurrencyDto>> Handle(
        GetCurrencyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Currencies
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<CurrencyDto>.Failure(ErrorCodes.FinancialSettings.CurrencyNotFound, ErrorCategory.NotFound, $"Currency with ID {request.Id} not found.");

        return Result<CurrencyDto>.Success(mapper.Map<CurrencyDto>(entity));
    }
}
