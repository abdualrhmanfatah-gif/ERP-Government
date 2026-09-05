using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common.DTOs;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Queries.Users;

// T011 — GetUserByIdQuery
public class GetUserByIdQuery : IRequest<UserDetailDto?>
{
    public int Id { get; init; }
}

public class GetUserByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    public async Task<UserDetailDto?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
            return null;

        var activeSessionCount = await context.UserSessions
            .CountAsync(s => s.UserId == user.Id && !s.IsRevoked, cancellationToken);

        var role = await context.SecurityRoles
            .Where(r => r.Id == user.RoleId)
            .Select(r => new UserRoleDto
            {
                RoleId = r.Id,
                Code = r.Code,
                Name = r.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new UserDetailDto
        {
            Id = user.Id,
            Login = user.Login,
            AccountType = user.AccountType.ToString(),
            IsActive = user.IsActive,
            DepartmentId = user.DepartmentId,
            MfaEnabled = user.MfaEnabled,
            MfaMethod = user.MfaMethod,
            MustChangePassword = user.MustChangePassword,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            IsLocked = user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow,
            LockedUntil = user.LockedUntil,
            PasswordChangedAt = user.PasswordChangedAt,
            ActiveSessionCount = activeSessionCount,
            Role = role,
            CreatedAt = user.Created,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.LastModified,
            UpdatedBy = user.LastModifiedBy
        };
    }
}
