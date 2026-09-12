using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItemAllocations;

[Authorize(Policy = PermissionCodes.BudgetItemAllocationsView)]
public record GetBudgetItemAllocationsListQuery(int BudgetId) : IRequest<List<BudgetItemAllocationDto>>;

public class GetBudgetItemAllocationsListQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBudgetItemAllocationsListQuery, List<BudgetItemAllocationDto>>
{
    public async Task<List<BudgetItemAllocationDto>> Handle(
        GetBudgetItemAllocationsListQuery request,
        CancellationToken cancellationToken)
    {
        return await context.BudgetItemAllocations
            .Where(x => x.BudgetId == request.BudgetId)
            .Select(x => new BudgetItemAllocationDto
            {
                Id = x.Id,
                BudgetId = x.BudgetId,
                BudgetItemId = x.BudgetItemId,
                BudgetItemCode = x.BudgetItem.ItemCode,
                BudgetItemName = x.BudgetItem.ItemName,
                AccountId = x.BudgetItem.AccountId,
                CostCenterId = x.BudgetItem.CostCenterId,
                BudgetClassificationId = x.BudgetItem.BudgetClassificationId,
                ProposedAmount = x.ProposedAmount,
                ApprovedAmount = x.ApprovedAmount,
                Remarks = x.Remarks,
                RowVersion = x.RowVersion,
            })
            .ToListAsync(cancellationToken);
    }
}
