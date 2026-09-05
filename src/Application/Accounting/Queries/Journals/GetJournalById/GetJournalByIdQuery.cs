using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.Journals.GetJournalById;

[Authorize(Policy = PermissionCodes.JournalsRead)]
public class GetJournalByIdQuery : IRequest<JournalDto?>
{
    public int Id { get; init; }
}

public class GetJournalByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetJournalByIdQuery, JournalDto?>
{
    public async Task<JournalDto?> Handle(
        GetJournalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Journals
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<JournalDto>(entity);
    }
}
