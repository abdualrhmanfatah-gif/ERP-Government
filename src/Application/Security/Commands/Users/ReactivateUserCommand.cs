using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T018 — ReactivateUserCommand
[Authorize(Policy = PermissionCodes.UsersDeactivate)]
public class ReactivateUserCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ReactivateUserCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ReactivateUserCommand, Result>
{
    public async Task<Result> Handle(
        ReactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        if (entity.IsActive)
            return Result.Success(); // Idempotent

        entity.IsActive = true;
        entity.FailedLoginAttempts = 0;
        entity.LockedUntil = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ReactivateUserCommandValidator : AbstractValidator<ReactivateUserCommand>
{
    public ReactivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
