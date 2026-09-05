using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemAvailabilityQuery(int Id) : IRequest<BudgetAvailabilitySummary?>;

public class GetBudgetItemAvailabilityQueryHandler(
    IBudgetAvailabilityService availabilityService) : IRequestHandler<GetBudgetItemAvailabilityQuery, BudgetAvailabilitySummary?>
{
    public async Task<BudgetAvailabilitySummary?> Handle(
        GetBudgetItemAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await availabilityService.GetAvailabilitySummaryAsync(request.Id);
        if (summary.NetAppropriated == 0 && summary.Encumbered == 0 && summary.Available == 0)
            return null;
        return summary;
    }
}
