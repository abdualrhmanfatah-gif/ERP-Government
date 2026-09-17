using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupById;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountGroupByIdQuery : IRequest<Result<AccountGroupDto>>
{
    public int Id { get; init; }
}

public class GetAccountGroupByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountGroupByIdQuery, Result<AccountGroupDto>>
{
    public async Task<Result<AccountGroupDto>> Handle(
        GetAccountGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.AccountGroups
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<AccountGroupDto>.Failure(ErrorCodes.Accounting.AccountGroupNotFound, ErrorCategory.NotFound, $"Account group with ID {request.Id} not found.");

        return Result<AccountGroupDto>.Success(mapper.Map<AccountGroupDto>(entity));
    }
}
