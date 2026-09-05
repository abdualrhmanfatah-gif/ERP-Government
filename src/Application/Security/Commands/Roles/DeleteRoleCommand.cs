using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.Roles;

// C-S003 — DeleteRoleCommand
[Authorize(Policy = PermissionCodes.RolesDelete)]
public class DeleteRoleCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteRoleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityRoles
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Role not found."]);

        if (entity.IsSystem)
            return Result.Failure(["Cannot delete system roles."]);

        var hasUsers = await context.Users
            .AnyAsync(x => x.RoleId == request.Id, cancellationToken);

        if (hasUsers)
            return Result.Failure(["Cannot delete role with assigned users."]);

        entity.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
