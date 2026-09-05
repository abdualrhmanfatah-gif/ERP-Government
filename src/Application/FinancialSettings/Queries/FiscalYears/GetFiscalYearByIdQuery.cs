using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalYears;

// Q-F004 — GetFiscalYearByIdQuery
[Authorize(Policy = PermissionCodes.FiscalYearsView)]
public class GetFiscalYearByIdQuery : IRequest<FiscalYearDto?>
{
    public int Id { get; init; }
}

public class GetFiscalYearByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalYearByIdQuery, FiscalYearDto?>
{
    public async Task<FiscalYearDto?> Handle(
        GetFiscalYearByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalYears
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<FiscalYearDto>(entity);
    }
}
