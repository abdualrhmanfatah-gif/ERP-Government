using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.Accounts.GetAccountById;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountByIdQuery : IRequest<Result<AccountDto>>
{
    public int Id { get; init; }
}

public class GetAccountByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Accounts
            .Include(x => x.AccountGroup)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<AccountDto>.Failure(ErrorCodes.Accounting.AccountNotFound, ErrorCategory.NotFound, $"Account with ID {request.Id} not found.");

        return Result<AccountDto>.Success(mapper.Map<AccountDto>(entity));
    }
}
