using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntryById;

[Authorize(Policy = PermissionCodes.RecurringEntriesRead)]
public class GetRecurringEntryByIdQuery : IRequest<RecurringEntryDto?>
{
    public int Id { get; init; }
}

public class GetRecurringEntryByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRecurringEntryByIdQuery, RecurringEntryDto?>
{
    public async Task<RecurringEntryDto?> Handle(
        GetRecurringEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RecurringEntries
            .Include(x => x.Template)
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<RecurringEntryDto>(entity);
    }
}
