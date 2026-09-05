using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.AccountingEvents.GetPendingEvents;

[Authorize(Policy = PermissionCodes.AccountingEventsRead)]
public class GetPendingEventsQuery : IRequest<List<AccountingEventDto>>
{
    public string? EventType { get; init; }
    public string? SourceDocumentType { get; init; }
}

public class GetPendingEventsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPendingEventsQuery, List<AccountingEventDto>>
{
    public async Task<List<AccountingEventDto>> Handle(
        GetPendingEventsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.AccountingEvents
            .Where(x => x.Status == EventStatus.Pending)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EventType))
            query = query.Where(x => x.EventType.ToString() == request.EventType);

        if (!string.IsNullOrEmpty(request.SourceDocumentType))
            query = query.Where(x => x.SourceDocumentType == request.SourceDocumentType);

        var items = await query
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<AccountingEventDto>>(items);
    }
}
