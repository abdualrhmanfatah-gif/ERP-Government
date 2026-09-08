using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Enums;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T017 — DeactivateUserCommand
[Authorize(Policy = PermissionCodes.UsersDeactivate)]
public class DeactivateUserCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeactivateUserCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateUserCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        if (!entity.IsActive)
            return Result.Success(); // Idempotent

        // FR-005: Block deactivation of System accounts
        if (entity.AccountType == AccountType.System)
            return Result.Failure(["System accounts cannot be deactivated."]);

        // FR-008: Block deactivation of last active administrator
        var isAdmin = await context.Users
            .Where(u => u.Id == entity.Id)
            .Join(context.SecurityRoles,
                u => u.RoleId,
                r => r.Id,
                (u, r) => r)
            .AnyAsync(r => r.IsActive && r.IsAdmin, cancellationToken);

        if (isAdmin)
        {
            var otherAdminCount = await context.Users
                .Where(u => u.Id != entity.Id && u.IsActive)
                .Join(context.SecurityRoles,
                    u => u.RoleId,
                    r => r.Id,
                    (u, r) => r)
                .AnyAsync(r => r.IsActive && r.IsAdmin, cancellationToken);

            if (!otherAdminCount)
                return Result.Failure(["Cannot deactivate the last active administrator."]);
        }

        // FR-005 (revised): pending approval delegations removed (DEP-026) — no guard required

        // Deactivate user
        entity.IsActive = false;

        // Revoke all active sessions
        var activeSessions = await context.UserSessions
            .Where(s => s.UserId == entity.Id && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.LogoutReason = "Administrator Revoked";
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
