using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.UserPermissions;

// FEATURE-008 — RemoveUserPermissionCommand
[Authorize(Policy = PermissionCodes.UsersManageRoles)]
public class RemoveUserPermissionCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public int PermissionId { get; init; }
}

public class RemoveUserPermissionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveUserPermissionCommand, Result>
{
    public async Task<Result> Handle(
        RemoveUserPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == request.UserId && up.PermissionId == request.PermissionId, cancellationToken);

        if (entity is null)
            return Result.Failure(["Permission assignment not found."]);

        context.UserPermissions.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RemoveUserPermissionCommandValidator : AbstractValidator<RemoveUserPermissionCommand>
{
    public RemoveUserPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("Invalid permission ID.");
    }
}
