using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.Common;
using MediatR;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

[Authorize(Policy = PermissionCodes.ReportingViewBudgetExecution)]
public record GetBudgetExecutionReportQuery : IRequest<BudgetExecutionReportDto>
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
    public int? FundId { get; init; }
    public int? ProgramId { get; init; }
    public int? ProjectId { get; init; }
    public int? BudgetItemId { get; init; }
}
