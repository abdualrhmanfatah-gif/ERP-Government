using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.ClosingEntries;

// T-017-013 — GetClosingEntryByIdQuery
[Authorize(Policy = PermissionCodes.ClosingEntriesView)]
public class GetClosingEntryByIdQuery : IRequest<ClosingEntryDto?>
{
    public int Id { get; init; }
}

public class GetClosingEntryByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetClosingEntryByIdQuery, ClosingEntryDto?>
{
    public async Task<ClosingEntryDto?> Handle(
        GetClosingEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await context.YearEndClosingEntries
            .Include(e => e.FiscalYear)
            .Include(e => e.ReversalOf)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entry is null)
            return null;

        return mapper.Map<ClosingEntryDto>(entry);
    }
}
