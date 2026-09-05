using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.UserPermissions;

[Authorize(Policy = PermissionCodes.UsersManageRoles)]
public class AssignUserPermissionCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public int PermissionId { get; init; }
    public bool IsGranted { get; init; } = true;
    public DateTimeOffset? EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class AssignUserPermissionCommandHandler(
    IApplicationDbContext context,
    IUser currentUser) : IRequestHandler<AssignUserPermissionCommand, Result>
{
    public async Task<Result> Handle(
        AssignUserPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(["User not found."]);

        var permissionExists = await context.SecurityPermissions.AnyAsync(p => p.Id == request.PermissionId, cancellationToken);
        if (!permissionExists)
            return Result.Failure(["Permission not found."]);

        // Create-or-update semantics (unique constraint on UserId + PermissionId)
        var existing = await context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == request.UserId && up.PermissionId == request.PermissionId, cancellationToken);

        string action;
        object oldValues;
        object newValues;

        if (existing is not null)
        {
            oldValues = new { existing.IsGranted, existing.Reason, existing.EffectiveFrom, existing.EffectiveTo };
            existing.IsGranted = request.IsGranted;
            existing.Reason = request.Reason;
            existing.EffectiveFrom = request.EffectiveFrom;
            existing.EffectiveTo = request.EffectiveTo;
            newValues = new { request.IsGranted, request.Reason, request.EffectiveFrom, request.EffectiveTo };
            action = "UpdatePermissionOverride";
        }
        else
        {
            var entity = new UserPermission
            {
                UserId = request.UserId,
                PermissionId = request.PermissionId,
                IsGranted = request.IsGranted,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                Reason = request.Reason
            };

            context.UserPermissions.Add(entity);
            oldValues = new { };
            newValues = new { request.IsGranted, request.Reason, request.EffectiveFrom, request.EffectiveTo };
            action = "CreatePermissionOverride";
        }

        // SEC-002: Explicit audit trail for permission override
        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "PermissionOverride",
            Action = action,
            UserId = currentUser.Id ?? 0,
            EntityName = "UserPermission",
            EntityId = request.UserId,
            Success = true,
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(newValues),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class AssignUserPermissionCommandValidator : AbstractValidator<AssignUserPermissionCommand>
{
    public AssignUserPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("Invalid permission ID.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for every permission override.");

        RuleFor(x => x.EffectiveFrom)
            .LessThanOrEqualTo(x => x.EffectiveTo)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveFrom must be before EffectiveTo.");
    }
}
