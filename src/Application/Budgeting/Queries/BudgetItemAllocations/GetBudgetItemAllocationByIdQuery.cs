using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItemAllocations;

[Authorize(Policy = PermissionCodes.BudgetItemAllocationsView)]
public record GetBudgetItemAllocationByIdQuery(int Id) : IRequest<BudgetItemAllocationDetailDto?>;

public class GetBudgetItemAllocationByIdQueryHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService availabilityService) : IRequestHandler<GetBudgetItemAllocationByIdQuery, BudgetItemAllocationDetailDto?>
{
    public async Task<BudgetItemAllocationDetailDto?> Handle(
        GetBudgetItemAllocationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItemAllocations
            .Include(x => x.Budget)
            .Include(x => x.BudgetItem)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        var summary = await availabilityService.GetAvailabilitySummaryAsync(entity.BudgetItemId);

        return new BudgetItemAllocationDetailDto
        {
            Id = entity.Id,
            BudgetId = entity.BudgetId,
            BudgetItemId = entity.BudgetItemId,
            BudgetItemCode = entity.BudgetItem.ItemCode,
            BudgetItemName = entity.BudgetItem.ItemName,
            ProposedAmount = entity.ProposedAmount,
            ApprovedAmount = entity.ApprovedAmount,
            Remarks = entity.Remarks,
            ActualExpenditure = summary.ActualExpenditure,
            OutstandingEncumbrance = summary.OutstandingEncumbrance,
            RemainingAmount = entity.ApprovedAmount.HasValue ? entity.ApprovedAmount.Value - summary.ActualExpenditure : null,
            AvailableAmount = entity.ApprovedAmount.HasValue ? entity.ApprovedAmount.Value - summary.ActualExpenditure - summary.OutstandingEncumbrance : null,
            Status = entity.Budget.Status.ToString(),
            RowVersion = entity.RowVersion,
        };
    }
}
