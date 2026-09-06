using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReportAuditService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task LogAsync(
        string reportName,
        object parameters,
        string format,
        bool success,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        if (_user.Id is not int userId)
            throw new UnauthorizedAccessException();

        var auditLog = new SecurityAuditLog
        {
            EventCategory = "Report",
            Action = format == "Screen" ? "ReportGenerate" : format == "Print" ? "ReportPrint" : "ReportExport",
            UserId = userId,
            EntityName = reportName,
            EntityId = 0,
            Success = success,
            FailureReason = failureReason,
            NewValues = JsonSerializer.Serialize(new
            {
                reportName,
                @params = parameters,
                format,
                success,
                failureReason,
                generatedAt = DateTimeOffset.UtcNow,
            }),
            Timestamp = DateTimeOffset.UtcNow,
        };

        _context.SecurityAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
