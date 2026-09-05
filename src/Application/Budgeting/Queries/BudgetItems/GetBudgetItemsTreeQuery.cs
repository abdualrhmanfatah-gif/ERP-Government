using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetBudgetItemsTreeQuery(int BudgetId) : IRequest<List<BudgetItemDto>>;

public class GetBudgetItemsTreeQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetItemsTreeQuery, List<BudgetItemDto>>
{
    public async Task<List<BudgetItemDto>> Handle(
        GetBudgetItemsTreeQuery request,
        CancellationToken cancellationToken)
    {
        var allItems = await context.BudgetItems
            .Where(x => x.BudgetId == request.BudgetId)
            .OrderBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);

        var budget = await context.Budgets
            .Include(x => x.BudgetType)
            .FirstOrDefaultAsync(x => x.Id == request.BudgetId, cancellationToken);

        var dtos = mapper.Map<List<BudgetItemDto>>(allItems);

        // Resolve AllowOverrunEffective for each item
        foreach (var dto in dtos)
        {
            var entity = allItems.First(x => x.Id == dto.Id);
            dto.AllowOverrunEffective = ResolveAllowOverrun(entity, budget);
        }

        var lookup = dtos.ToDictionary(x => x.Id);

        List<BudgetItemDto> roots = [];

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && lookup.TryGetValue(dto.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        SetLevel(roots, 0);

        return roots;
    }

    private static void SetLevel(List<BudgetItemDto> items, int level)
    {
        foreach (var item in items)
        {
            item.Level = level;
            if (item.Children.Count > 0)
                SetLevel(item.Children, level + 1);
        }
    }

    private static bool ResolveAllowOverrun(
        Domain.Budgeting.Entities.BudgetItem item,
        Domain.Budgeting.Entities.Budget? budget)
    {
        if (item.AllowOverrun.HasValue)
            return item.AllowOverrun.Value;

        if (budget?.AllowOverrun.HasValue == true)
            return budget.AllowOverrun.Value;

        if (budget?.BudgetType is not null)
            return budget.BudgetType.AllowOverrun;

        return false;
    }
}
