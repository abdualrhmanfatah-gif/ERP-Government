using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;

[Authorize(Policy = PermissionCodes.ReportingViewTrialBalanceReport)]
public record GetTrialBalanceReportQuery : IRequest<TrialBalanceReportDto>
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
    public int? FundId { get; init; }
    public int? ProjectId { get; init; }
}
