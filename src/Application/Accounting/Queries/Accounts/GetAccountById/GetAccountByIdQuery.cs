using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.Accounts.GetAccountById;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountByIdQuery : IRequest<AccountDto?>
{
    public int Id { get; init; }
}

public class GetAccountByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    public async Task<AccountDto?> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Accounts
            .Include(x => x.AccountGroup)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<AccountDto>(entity);
    }
}
