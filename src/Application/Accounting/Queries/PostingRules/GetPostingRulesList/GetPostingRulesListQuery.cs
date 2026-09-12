using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.PostingRules.GetPostingRulesList;

[Authorize(Policy = PermissionCodes.PostingRulesRead)]
public class GetPostingRulesListQuery : IRequest<List<PostingRuleDto>>
{
    public bool? IsActive { get; init; }
    public string? EventType { get; init; }
    public int? JournalId { get; init; }
}

public class GetPostingRulesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPostingRulesListQuery, List<PostingRuleDto>>
{
    public async Task<List<PostingRuleDto>> Handle(
        GetPostingRulesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PostingRules
            .Include(x => x.Journal)
            .Include(x => x.Lines).ThenInclude(l => l.FixedAccount)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrEmpty(request.EventType))
            query = query.Where(x => x.EventType == request.EventType);

        if (request.JournalId.HasValue)
            query = query.Where(x => x.JournalId == request.JournalId.Value);

        var items = await query
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<PostingRuleDto>>(items);
    }
}
