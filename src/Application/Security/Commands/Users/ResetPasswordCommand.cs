using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T023 — ResetPasswordCommand (triggers email notification)
[Authorize(Policy = PermissionCodes.UsersUpdate)]
public class ResetPasswordCommand : IRequest<Result>
{
    public int UserId { get; init; }
}

public class ResetPasswordCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        // Generate reset token and store on User entity
        entity.PasswordResetToken = Guid.NewGuid().ToString("N");
        entity.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

        await context.SaveChangesAsync(cancellationToken);

        // TODO: Send email notification with reset link
        // This would integrate with an IEmailService

        return Result.Success();
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
