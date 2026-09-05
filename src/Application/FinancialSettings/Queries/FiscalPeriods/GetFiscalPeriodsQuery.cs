using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsView)]
public class GetFiscalPeriodsQuery : IRequest<List<FiscalPeriodDto>>
{
    public int FiscalYearId { get; init; }
}

public class GetFiscalPeriodsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalPeriodsQuery, List<FiscalPeriodDto>>
{
    public async Task<List<FiscalPeriodDto>> Handle(
        GetFiscalPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.FiscalPeriods
            .Where(fp => fp.FiscalYearId == request.FiscalYearId)
            .OrderBy(fp => fp.PeriodNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<FiscalPeriodDto>>(items);
    }
}
