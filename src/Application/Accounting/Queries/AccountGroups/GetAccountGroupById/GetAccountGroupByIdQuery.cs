using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupById;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountGroupByIdQuery : IRequest<AccountGroupDto?>
{
    public int Id { get; init; }
}

public class GetAccountGroupByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountGroupByIdQuery, AccountGroupDto?>
{
    public async Task<AccountGroupDto?> Handle(
        GetAccountGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.AccountGroups
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<AccountGroupDto>(entity);
    }
}
