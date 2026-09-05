using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T024 — ChangePasswordCommand
[Authorize]
public class ChangePasswordCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public string NewPassword { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; } = false;
}

public class ChangePasswordCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        // Hash the new password
        var passwordHash = HashPassword(request.NewPassword);

        entity.PasswordHash = passwordHash;
        entity.PasswordChangedAt = DateTime.UtcNow;
        entity.MustChangePassword = request.MustChangePassword;
        entity.FailedLoginAttempts = 0;
        entity.LockedUntil = null;

        // Invalidate all existing sessions (optional security measure)
        var activeSessions = await context.UserSessions
            .Where(s => s.UserId == entity.Id && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.LogoutReason = "Password Changed";
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string HashPassword(string password)
    {
        // In production, this would use BCrypt or similar
        // Placeholder for now
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
