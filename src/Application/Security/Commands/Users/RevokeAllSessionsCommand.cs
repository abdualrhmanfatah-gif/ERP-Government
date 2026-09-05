using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T021 — RevokeAllSessionsCommand
[Authorize(Policy = PermissionCodes.UsersManageSessions)]
public class RevokeAllSessionsCommand : IRequest<Result>
{
    public int UserId { get; init; }
}

public class RevokeAllSessionsCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RevokeAllSessionsCommand, Result>
{
    public async Task<Result> Handle(
        RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var activeSessions = await context.UserSessions
            .Where(s => s.UserId == request.UserId && !s.IsRevoked)
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

public class RevokeAllSessionsCommandValidator : AbstractValidator<RevokeAllSessionsCommand>
{
    public RevokeAllSessionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID.");
    }
}
