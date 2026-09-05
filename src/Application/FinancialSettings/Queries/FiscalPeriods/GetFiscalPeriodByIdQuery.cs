using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsView)]
public class GetFiscalPeriodByIdQuery : IRequest<FiscalPeriodDto?>
{
    public int Id { get; init; }
}

public class GetFiscalPeriodByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalPeriodByIdQuery, FiscalPeriodDto?>
{
    public async Task<FiscalPeriodDto?> Handle(
        GetFiscalPeriodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalPeriods
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<FiscalPeriodDto>(entity);
    }
}
