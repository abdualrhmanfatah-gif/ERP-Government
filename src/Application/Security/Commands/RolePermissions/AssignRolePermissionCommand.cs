using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.RolePermissions;

// FEATURE-008 — AssignRolePermissionCommand
[Authorize(Policy = PermissionCodes.RolesUpdate)]
public class AssignRolePermissionCommand : IRequest<Result>
{
    public int RoleId { get; init; }
    public int PermissionId { get; init; }
}

public class AssignRolePermissionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AssignRolePermissionCommand, Result>
{
    public async Task<Result> Handle(
        AssignRolePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var roleExists = await context.SecurityRoles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!roleExists)
            return Result.Failure(["Role not found."]);

        var permissionExists = await context.SecurityPermissions.AnyAsync(p => p.Id == request.PermissionId, cancellationToken);
        if (!permissionExists)
            return Result.Failure(["Permission not found."]);

        var alreadyAssigned = await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (alreadyAssigned)
            return Result.Failure(["Permission already assigned to this role."]);

        var entity = new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId
        };

        context.RolePermissions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class AssignRolePermissionCommandValidator : AbstractValidator<AssignRolePermissionCommand>
{
    public AssignRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Invalid role ID.");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("Invalid permission ID.");
    }
}
