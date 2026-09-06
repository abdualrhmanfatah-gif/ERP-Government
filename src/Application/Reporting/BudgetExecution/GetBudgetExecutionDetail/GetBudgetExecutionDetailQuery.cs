using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;

[Authorize(Policy = PermissionCodes.ReportingViewBudgetExecution)]
public record GetBudgetExecutionDetailQuery : IRequest<BudgetExecutionDetailDto>
{
    public int BudgetItemId { get; init; }
    public int FiscalYearId { get; init; }
    public int? FundId { get; init; }
}
