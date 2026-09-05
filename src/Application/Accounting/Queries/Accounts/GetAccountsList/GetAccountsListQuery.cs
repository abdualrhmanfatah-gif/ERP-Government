using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.Accounts.GetAccountsList;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountsListQuery : IRequest<List<AccountDto>>
{
    public bool? IsActive { get; init; }
    public int? AccountGroupId { get; init; }
    public bool? IsPostable { get; init; }
    public NormalBalanceType? NormalBalance { get; init; }
}

public class GetAccountsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountsListQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(
        GetAccountsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Accounts
            .Include(x => x.AccountGroup)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.AccountGroupId.HasValue)
            query = query.Where(x => x.AccountGroupId == request.AccountGroupId.Value);

        if (request.IsPostable.HasValue)
            query = query.Where(x => x.IsPostable == request.IsPostable.Value);

        if (request.NormalBalance.HasValue)
            query = query.Where(x => x.NormalBalance == request.NormalBalance.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<AccountDto>>(items);
    }
}
