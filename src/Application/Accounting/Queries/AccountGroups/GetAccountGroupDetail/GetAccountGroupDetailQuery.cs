using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupDetail;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountGroupDetailQuery : IRequest<Result<AccountGroupDetailResponse>>
{
    public int Id { get; init; }
}

public class GetAccountGroupDetailQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountGroupDetailQuery, Result<AccountGroupDetailResponse>>
{
    public async Task<Result<AccountGroupDetailResponse>> Handle(GetAccountGroupDetailQuery request, CancellationToken cancellationToken)
    {
        var group = await context.AccountGroups.FindAsync(request.Id, cancellationToken);
        if (group == null)
            return Result<AccountGroupDetailResponse>.Failure(ErrorCodes.Accounting.AccountGroupNotFound, ErrorCategory.NotFound, $"Account group with ID {request.Id} not found.");

        var groupDto = mapper.Map<AccountGroupDto>(group);

        var children = await context.AccountGroups
            .Where(x => x.ParentId == request.Id)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var childrenDtos = mapper.Map<List<AccountGroupDto>>(children);

        var accounts = await context.Accounts
            .Where(x => x.AccountGroupId == request.Id)
            .OrderBy(x => x.Code)
            .Select(x => new LinkedAccountRefDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                IsPostable = x.IsPostable
            })
            .ToListAsync(cancellationToken);

        var audit = await context.AuditTrails
            .Where(x => x.DocumentType == "AccountGroup" && x.DocumentId == request.Id)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new AccountGroupAuditEntryDto
            {
                Id = x.Id,
                Action = x.Action.ToString(),
                UserId = x.UserId,
                UserName = x.User != null ? x.User.Login : null,
                Timestamp = x.Timestamp,
                FieldChanges = x.FieldChanges,
                ChangeSummary = x.ChangeSummary,
                IpAddress = x.IpAddress
            })
            .ToListAsync(cancellationToken);

        return Result<AccountGroupDetailResponse>.Success(new AccountGroupDetailResponse
        {
            Group = groupDto,
            Children = childrenDtos,
            Accounts = accounts,
            Audit = audit
        });
    }
}
