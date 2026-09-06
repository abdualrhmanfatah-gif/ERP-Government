using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemsListQuery(int BudgetId) : IRequest<List<BudgetItemDto>>;

public class GetBudgetItemsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetItemsListQuery, List<BudgetItemDto>>
{
    public async Task<List<BudgetItemDto>> Handle(
        GetBudgetItemsListQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.BudgetItems
            .Where(x => x.BudgetId == request.BudgetId)
            .OrderBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);

        // Collect IDs for lookup
        var fundIds = items.Where(x => x.FundId.HasValue).Select(x => x.FundId!.Value).Distinct().ToList();
        var accountIds = items.Where(x => x.AccountId.HasValue).Select(x => x.AccountId!.Value).Distinct().ToList();
        var costCenterIds = items.Where(x => x.CostCenterId.HasValue).Select(x => x.CostCenterId!.Value).Distinct().ToList();
        var classificationIds = items.Where(x => x.BudgetClassificationId.HasValue).Select(x => x.BudgetClassificationId!.Value).Distinct().ToList();

        var funds = await context.Funds
            .Where(x => fundIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FundName, cancellationToken);

        var accounts = await context.Accounts
            .Where(x => accountIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var costCenters = await context.CostCenters
            .Where(x => costCenterIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var classifications = await context.BudgetClassifications
            .Where(x => classificationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var dtos = mapper.Map<List<BudgetItemDto>>(items);

        foreach (var dto in dtos)
        {
            var entity = items.First(x => x.Id == dto.Id);
            if (entity.FundId.HasValue && funds.TryGetValue(entity.FundId.Value, out var fundName))
                dto.FundName = fundName;
            if (entity.AccountId.HasValue && accounts.TryGetValue(entity.AccountId.Value, out var accountName))
                dto.AccountName = accountName;
            if (entity.CostCenterId.HasValue && costCenters.TryGetValue(entity.CostCenterId.Value, out var costCenterName))
                dto.CostCenterName = costCenterName;
            if (entity.BudgetClassificationId.HasValue && classifications.TryGetValue(entity.BudgetClassificationId.Value, out var classificationName))
                dto.BudgetClassificationName = classificationName;
        }

        return dtos;
    }
}
