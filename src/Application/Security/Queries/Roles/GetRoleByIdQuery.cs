using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.Roles;

// Q-S002 — GetRoleByIdQuery
[Authorize(Policy = PermissionCodes.RolesView)]
public class GetRoleByIdQuery : IRequest<Result<SecurityRoleDto>>
{
    public int Id { get; init; }
}

public class GetRoleByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRoleByIdQuery, Result<SecurityRoleDto>>
{
    public async Task<Result<SecurityRoleDto>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityRoles
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<SecurityRoleDto>.Failure(ErrorCodes.Security.RoleNotFound, ErrorCategory.NotFound, $"Security role with ID {request.Id} not found.");

        return Result<SecurityRoleDto>.Success(mapper.Map<SecurityRoleDto>(entity));
    }
}
