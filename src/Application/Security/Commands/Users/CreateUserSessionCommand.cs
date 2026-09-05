using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;
using System.Text;

namespace ERP_Government.Application.Security.Commands.Users;

// Create session after login — records IP, UserAgent, hashes
public class CreateUserSessionCommand : IRequest<Result<int>>
{
    public int UserId { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string? UserAgent { get; init; }
    public string? DeviceFingerprint { get; init; }
}

public class CreateUserSessionCommandHandler(
    IApplicationDbContext context,
    TimeProvider timeProvider) : IRequestHandler<CreateUserSessionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateUserSessionCommand request, CancellationToken cancellationToken)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            return Result<int>.Failure(["User not found."]);

        var now = timeProvider.GetUtcNow();

        var session = new UserSession
        {
            UserId = request.UserId,
            IpAddress = string.IsNullOrWhiteSpace(request.IpAddress) ? "0.0.0.0" : request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceFingerprint = request.DeviceFingerprint,
            SessionTokenHash = Hash(Guid.NewGuid().ToString()),
            RefreshTokenHash = Hash(Guid.NewGuid().ToString()),
            CreatedAt = now,
            LastActivityAt = now,
            ExpiresAt = now.AddHours(8),
            RefreshTokenExpiresAt = now.AddDays(7),
            AbsoluteExpiryAt = now.AddDays(7),
            IsRevoked = false
        };

        context.UserSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        // update LastLoginAt
        var user = await context.Users.FirstAsync(u => u.Id == request.UserId, cancellationToken);
        user.LastLoginAt = now;
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(session.Id);
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}

public class CreateUserSessionCommandValidator : AbstractValidator<CreateUserSessionCommand>
{
    public CreateUserSessionCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.IpAddress).NotEmpty();
    }
}
