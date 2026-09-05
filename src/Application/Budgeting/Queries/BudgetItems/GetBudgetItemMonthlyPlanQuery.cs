using ERP_Government.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemMonthlyPlanQuery(int BudgetItemId) : IRequest<List<MonthlyPlanDto>>;

public record MonthlyPlanDto(int Id, int Month, decimal PlannedAmount, byte[] RowVersion);

public class GetBudgetItemMonthlyPlanQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBudgetItemMonthlyPlanQuery, List<MonthlyPlanDto>>
{
    public async Task<List<MonthlyPlanDto>> Handle(
        GetBudgetItemMonthlyPlanQuery request,
        CancellationToken cancellationToken)
    {
        return await context.BudgetItemMonthlyPlans
            .Where(p => p.BudgetItemId == request.BudgetItemId)
            .OrderBy(p => p.Month)
            .Select(p => new MonthlyPlanDto(p.Id, p.Month, p.PlannedAmount, p.RowVersion))
            .ToListAsync(cancellationToken);
    }
}
