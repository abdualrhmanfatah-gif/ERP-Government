using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T022 — ResetFailedLoginAttemptsCommand
[Authorize(Policy = PermissionCodes.UsersResetLogin)]
public class ResetFailedLoginAttemptsCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ResetFailedLoginAttemptsCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ResetFailedLoginAttemptsCommand, Result>
{
    public async Task<Result> Handle(
        ResetFailedLoginAttemptsCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        entity.FailedLoginAttempts = 0;
        entity.LockedUntil = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ResetFailedLoginAttemptsCommandValidator : AbstractValidator<ResetFailedLoginAttemptsCommand>
{
    public ResetFailedLoginAttemptsCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
