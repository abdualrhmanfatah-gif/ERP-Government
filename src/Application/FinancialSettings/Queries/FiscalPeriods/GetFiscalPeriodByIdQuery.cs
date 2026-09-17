using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsView)]
public class GetFiscalPeriodByIdQuery : IRequest<Result<FiscalPeriodDto>>
{
    public int Id { get; init; }
}

public class GetFiscalPeriodByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalPeriodByIdQuery, Result<FiscalPeriodDto>>
{
    public async Task<Result<FiscalPeriodDto>> Handle(
        GetFiscalPeriodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalPeriods
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<FiscalPeriodDto>.Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, ErrorCategory.NotFound, $"Fiscal period with ID {request.Id} not found.");

        return Result<FiscalPeriodDto>.Success(mapper.Map<FiscalPeriodDto>(entity));
    }
}
