using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Permissions;

// Q-S004 — GetPermissionByIdQuery
[Authorize(Policy = PermissionCodes.PermissionsView)]
public class GetPermissionByIdQuery : IRequest<Result<SecurityPermissionDto>>
{
    public int Id { get; init; }
}

public class GetPermissionByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetPermissionByIdQuery, Result<SecurityPermissionDto>>
{
    public async Task<Result<SecurityPermissionDto>> Handle(
        GetPermissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityPermissions
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<SecurityPermissionDto>.Failure(ErrorCodes.Security.PermissionNotFound, ErrorCategory.NotFound, $"Security permission with ID {request.Id} not found.");

        return Result<SecurityPermissionDto>.Success(mapper.Map<SecurityPermissionDto>(entity));
    }
}
