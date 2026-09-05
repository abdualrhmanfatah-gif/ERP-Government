using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T025 — ForceResetPasswordCommand (admin forces reset)
[Authorize(Policy = PermissionCodes.UsersUpdate)]
public class ForceResetPasswordCommand : IRequest<Result>
{
    public int UserId { get; init; }
}

public class ForceResetPasswordCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ForceResetPasswordCommand, Result>
{
    public async Task<Result> Handle(
        ForceResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        // Set password change required flag
        entity.MustChangePassword = true;

        // Invalidate all existing sessions
        var activeSessions = await context.UserSessions
            .Where(s => s.UserId == entity.Id && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.LogoutReason = "Admin Forced Password Reset";
        }

        // Generate reset token on User entity
        entity.PasswordResetToken = Guid.NewGuid().ToString("N");
        entity.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

        await context.SaveChangesAsync(cancellationToken);

        // TODO: Send email notification with reset link

        return Result.Success();
    }
}

public class ForceResetPasswordCommandValidator : AbstractValidator<ForceResetPasswordCommand>
{
    public ForceResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
