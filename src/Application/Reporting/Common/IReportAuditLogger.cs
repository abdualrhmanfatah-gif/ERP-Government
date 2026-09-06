namespace ERP_Government.Application.Reporting.Common;

public interface IReportAuditLogger
{
    Task LogReportAccessAsync(string reportType, int userId, ReportFilterDto filter, CancellationToken cancellationToken = default);
}
