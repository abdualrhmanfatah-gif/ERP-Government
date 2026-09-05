using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.Funds;

[Authorize(Policy = PermissionCodes.FundsView)]
public record GetFundsListQuery : IRequest<List<FundDto>>;

public class GetFundsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFundsListQuery, List<FundDto>>
{
    public async Task<List<FundDto>> Handle(
        GetFundsListQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.Funds
            .OrderBy(x => x.FundNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<FundDto>>(items);
    }
}
