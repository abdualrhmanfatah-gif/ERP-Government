using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemByIdQuery(int Id) : IRequest<Result<BudgetItemDto>>;

public class GetBudgetItemByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetItemByIdQuery, Result<BudgetItemDto>>
{
    public async Task<Result<BudgetItemDto>> Handle(
        GetBudgetItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItems
            .Include(x => x.Budget)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<BudgetItemDto>.Failure(ErrorCodes.Budgets.BudgetNotFound, ErrorCategory.NotFound, $"Budget item with ID {request.Id} not found.");

        var dto = mapper.Map<BudgetItemDto>(entity);

        dto.AllowOverrunEffective = await ResolveAllowOverrun(context, entity, cancellationToken);

        dto.FundName = await context.Funds
            .Where(x => x.Id == entity.Budget.FundId)
            .Select(x => x.FundName)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity.AccountId.HasValue)
            dto.AccountName = await context.Accounts
                .Where(x => x.Id == entity.AccountId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);

        if (entity.CostCenterId.HasValue)
            dto.CostCenterName = await context.CostCenters
                .Where(x => x.Id == entity.CostCenterId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);

        if (entity.BudgetClassificationId.HasValue)
            dto.BudgetClassificationName = await context.BudgetClassifications
                .Where(x => x.Id == entity.BudgetClassificationId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);

        return Result<BudgetItemDto>.Success(dto);
    }

    private static async Task<bool> ResolveAllowOverrun(
        IApplicationDbContext context,
        Domain.Budgeting.Entities.BudgetItem item,
        CancellationToken cancellationToken)
    {
        if (item.AllowOverrun.HasValue)
            return item.AllowOverrun.Value;

        var budget = await context.Budgets
            .Include(x => x.BudgetType)
            .FirstOrDefaultAsync(x => x.Id == item.BudgetId, cancellationToken);

        if (budget?.AllowOverrun.HasValue == true)
            return budget.AllowOverrun.Value;

        if (budget?.BudgetType is not null)
            return budget.BudgetType.AllowOverrun;

        return false;
    }
}
