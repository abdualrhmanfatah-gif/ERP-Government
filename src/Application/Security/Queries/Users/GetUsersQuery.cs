using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Domain.Security.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Queries.Users;

// T010 — GetUsersQuery
[Authorize(Policy = PermissionCodes.UsersView)]
public class GetUsersQuery : IRequest<List<UserDto>>
{
    public bool? IsActive { get; init; }
    public AccountType? AccountType { get; init; }
    public int? DepartmentId { get; init; }
    public string? Search { get; init; }
}

public class GetUsersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Users.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (request.AccountType.HasValue)
            query = query.Where(u => u.AccountType == request.AccountType.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u => u.Login.Contains(request.Search));

        return await query
            .OrderBy(u => u.Login)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Login = u.Login,
                AccountType = u.AccountType.ToString(),
                IsActive = u.IsActive,
                DepartmentId = u.DepartmentId,
                MfaEnabled = u.MfaEnabled,
                LastLoginAt = u.LastLoginAt,
                FailedLoginAttempts = u.FailedLoginAttempts,
                IsLocked = u.LockedUntil.HasValue && u.LockedUntil > DateTimeOffset.UtcNow,
                CreatedAt = u.Created
            })
            .ToListAsync(cancellationToken);
    }
}
