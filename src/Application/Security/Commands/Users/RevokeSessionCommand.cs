using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T020 — RevokeSessionCommand
[Authorize(Policy = PermissionCodes.UsersManageSessions)]
public class RevokeSessionCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public int SessionId { get; init; }
}

public class RevokeSessionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RevokeSessionCommand, Result>
{
    public async Task<Result> Handle(
        RevokeSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, cancellationToken);

        if (session is null)
            return Result.Failure(["Session not found."]);

        if (session.IsRevoked)
            return Result.Success(); // Idempotent

        session.IsRevoked = true;
        session.LogoutReason = "Administrator Revoked";

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("Invalid session ID.");
    }
}
