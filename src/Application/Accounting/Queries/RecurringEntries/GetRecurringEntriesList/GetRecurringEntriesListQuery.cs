using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntriesList;

[Authorize(Policy = PermissionCodes.RecurringEntriesRead)]
public class GetRecurringEntriesListQuery : IRequest<List<RecurringEntryDto>>
{
    public bool? IsActive { get; init; }
    public int? JournalId { get; init; }
    public RecurringFrequency? Frequency { get; init; }
    public RecurringEntryStatus? Status { get; init; }
}

public class GetRecurringEntriesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRecurringEntriesListQuery, List<RecurringEntryDto>>
{
    public async Task<List<RecurringEntryDto>> Handle(
        GetRecurringEntriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.RecurringEntries
            .Include(x => x.Template)
            .Include(x => x.Journal)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.JournalId.HasValue)
            query = query.Where(x => x.JournalId == request.JournalId.Value);

        if (request.Frequency.HasValue)
            query = query.Where(x => x.Frequency == request.Frequency.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var items = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<RecurringEntryDto>>(items);
    }
}
