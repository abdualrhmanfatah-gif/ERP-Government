using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Queries.Users;

// T033 — GetLoginHistoryQuery
public class GetLoginHistoryQuery : IRequest<List<LoginHistoryDto>>
{
    public int UserId { get; init; }
    public int? Limit { get; init; } = 20;
}

public class LoginHistoryDto
{
    public DateTimeOffset Timestamp { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
}

public class GetLoginHistoryQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetLoginHistoryQuery, List<LoginHistoryDto>>
{
    public async Task<List<LoginHistoryDto>> Handle(
        GetLoginHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await context.SecurityAuditLogs
            .Where(a => a.UserId == request.UserId
                && a.Action == "Login")
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Limit ?? 20)
            .Select(a => new LoginHistoryDto
            {
                Timestamp = a.Timestamp,
                Status = a.Success ? "Success" : "Failed",
                IpAddress = a.IpAddress
            })
            .ToListAsync(cancellationToken);
    }
}
