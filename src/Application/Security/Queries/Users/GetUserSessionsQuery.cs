using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common.DTOs;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Queries.Users;

// T019 — GetUserSessionsQuery
public class GetUserSessionsQuery : IRequest<List<UserSessionDto>>
{
    public int UserId { get; init; }
}

public class GetUserSessionsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUserSessionsQuery, List<UserSessionDto>>
{
    public async Task<List<UserSessionDto>> Handle(
        GetUserSessionsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.UserSessions
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new UserSessionDto
            {
                Id = s.Id,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                DeviceFingerprint = s.DeviceFingerprint,
                CreatedAt = s.CreatedAt,
                LastActivityAt = s.LastActivityAt,
                ExpiresAt = s.ExpiresAt,
                IsRevoked = s.IsRevoked,
                LogoutReason = s.LogoutReason
            })
            .ToListAsync(cancellationToken);
    }
}
