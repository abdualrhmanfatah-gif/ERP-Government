using ERP_Government.Application.Reporting.Common;

namespace ERP_Government.Infrastructure.Services;

public class ReportAuditLogger : IReportAuditLogger
{
    public Task LogReportAccessAsync(string reportType, int userId, ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Audit logging placeholder — wired to SecurityAuditLog when RBAC enforcement lands (DEP-020)
        return Task.CompletedTask;
    }
}
