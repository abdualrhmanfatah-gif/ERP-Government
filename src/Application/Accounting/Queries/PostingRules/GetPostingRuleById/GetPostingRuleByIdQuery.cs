using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.PostingRules.GetPostingRuleById;

[Authorize(Policy = PermissionCodes.PostingRulesRead)]
public class GetPostingRuleByIdQuery : IRequest<PostingRuleDto?>
{
    public int Id { get; init; }
}

public class GetPostingRuleByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPostingRuleByIdQuery, PostingRuleDto?>
{
    public async Task<PostingRuleDto?> Handle(
        GetPostingRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PostingRules
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<PostingRuleDto>(entity);
    }
}
