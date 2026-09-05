using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.Journals.GetJournalsList;

[Authorize(Policy = PermissionCodes.JournalsRead)]
public class GetJournalsListQuery : IRequest<List<JournalDto>>
{
    public bool? IsActive { get; init; }
    public JournalType? Type { get; init; }
}

public class GetJournalsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetJournalsListQuery, List<JournalDto>>
{
    public async Task<List<JournalDto>> Handle(
        GetJournalsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Journals.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.Type.HasValue)
            query = query.Where(x => x.Type == request.Type.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<JournalDto>>(items);
    }
}
