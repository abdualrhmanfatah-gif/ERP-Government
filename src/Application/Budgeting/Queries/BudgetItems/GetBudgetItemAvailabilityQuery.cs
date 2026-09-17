using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemAvailabilityQuery(int Id) : IRequest<Result<BudgetAvailabilitySummary>>;

public class GetBudgetItemAvailabilityQueryHandler(
    IBudgetAvailabilityService availabilityService) : IRequestHandler<GetBudgetItemAvailabilityQuery, Result<BudgetAvailabilitySummary>>
{
    public async Task<Result<BudgetAvailabilitySummary>> Handle(
        GetBudgetItemAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await availabilityService.GetAvailabilitySummaryAsync(request.Id);
        if (summary.ApprovedAmount == 0 && summary.OutstandingEncumbrance == 0 && summary.AvailableAmount == 0)
            return Result<BudgetAvailabilitySummary>.Failure(ErrorCodes.Budgets.AvailabilityNotFound, ErrorCategory.NotFound, $"No budget availability found for item {request.Id}.");
        return Result<BudgetAvailabilitySummary>.Success(summary);
    }
}
