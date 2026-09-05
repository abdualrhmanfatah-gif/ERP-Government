using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.JournalEntries.GetJournalEntriesList;

[Authorize(Policy = PermissionCodes.JournalEntriesRead)]
public class GetJournalEntriesListQuery : IRequest<List<JournalEntryDto>>
{
    public string? EntryStatus { get; init; }
    public int? JournalId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}

public class GetJournalEntriesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetJournalEntriesListQuery, List<JournalEntryDto>>
{
    public async Task<List<JournalEntryDto>> Handle(
        GetJournalEntriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.JournalEntries
            .Include(x => x.Journal)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntryStatus))
            query = query.Where(x => x.EntryStatus.ToString() == request.EntryStatus);

        if (request.JournalId.HasValue)
            query = query.Where(x => x.JournalId == request.JournalId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(x => x.DocumentDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(x => x.DocumentDate <= request.ToDate.Value);

        var items = await query
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<JournalEntryDto>>(items);
    }
}
