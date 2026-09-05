using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.RolePermissions;

// FEATURE-008 — RemoveRolePermissionCommand
[Authorize(Policy = PermissionCodes.RolesUpdate)]
public class RemoveRolePermissionCommand : IRequest<Result>
{
    public int RoleId { get; init; }
    public int PermissionId { get; init; }
}

public class RemoveRolePermissionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveRolePermissionCommand, Result>
{
    public async Task<Result> Handle(
        RemoveRolePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (entity is null)
            return Result.Failure(["Permission assignment not found."]);

        context.RolePermissions.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RemoveRolePermissionCommandValidator : AbstractValidator<RemoveRolePermissionCommand>
{
    public RemoveRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Invalid role ID.");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("Invalid permission ID.");
    }
}
