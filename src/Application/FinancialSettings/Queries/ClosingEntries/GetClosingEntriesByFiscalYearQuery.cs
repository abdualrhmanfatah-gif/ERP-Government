using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.ClosingEntries;

// T-017-012 — GetClosingEntriesByFiscalYearQuery
[Authorize(Policy = PermissionCodes.ClosingEntriesView)]
public class GetClosingEntriesByFiscalYearQuery : IRequest<List<ClosingEntryDto>>
{
    public int FiscalYearId { get; init; }
}

public class GetClosingEntriesByFiscalYearQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetClosingEntriesByFiscalYearQuery, List<ClosingEntryDto>>
{
    public async Task<List<ClosingEntryDto>> Handle(
        GetClosingEntriesByFiscalYearQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await context.YearEndClosingEntries
            .Include(e => e.FiscalYear)
            .Include(e => e.ReversalOf)
            .Where(e => e.FiscalYearId == request.FiscalYearId && e.IsActive)
            .OrderByDescending(e => e.ClosingDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ClosingEntryDto>>(entries);
    }
}
