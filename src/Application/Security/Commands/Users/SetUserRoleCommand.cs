using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

[Authorize(Policy = PermissionCodes.UsersManageRoles)]
public class SetUserRoleCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public int RoleId { get; init; }
}

public class SetUserRoleCommandHandler(
    IApplicationDbContext context,
    IUser currentUser) : IRequestHandler<SetUserRoleCommand, Result>
{
    public async Task<Result> Handle(
        SetUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(["User not found."]);

        var role = await context.SecurityRoles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
            return Result.Failure(["Role not found."]);

        if (!role.IsActive)
            return Result.Failure(["Cannot assign an inactive role."]);

        var oldRoleId = user.RoleId;

        user.RoleId = request.RoleId;

        // SEC-002: Explicit audit trail for role change
        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "RoleChange",
            Action = "SetUserRole",
            UserId = currentUser.Id ?? 0,
            EntityName = "User",
            EntityId = request.UserId,
            Success = true,
            OldValues = JsonSerializer.Serialize(new { oldRoleId }),
            NewValues = JsonSerializer.Serialize(new { newRoleId = request.RoleId }),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SetUserRoleCommandValidator : AbstractValidator<SetUserRoleCommand>
{
    public SetUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Invalid role ID.");
    }
}
