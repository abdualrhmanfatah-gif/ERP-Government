using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalYears;

// Q-F003 — GetFiscalYearsQuery
[Authorize(Policy = PermissionCodes.FiscalYearsView)]
public class GetFiscalYearsQuery : IRequest<List<FiscalYearDto>>
{
    public bool? IsActive { get; init; }
}

public class GetFiscalYearsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalYearsQuery, List<FiscalYearDto>>
{
    public async Task<List<FiscalYearDto>> Handle(
        GetFiscalYearsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.FiscalYears.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderByDescending(x => x.YearNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<FiscalYearDto>>(items);
    }
}
