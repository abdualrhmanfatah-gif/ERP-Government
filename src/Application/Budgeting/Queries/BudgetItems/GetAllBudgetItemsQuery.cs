using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetItems;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetAllBudgetItemsQuery() : IRequest<List<BudgetItemDto>>;

public class GetAllBudgetItemsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAllBudgetItemsQuery, List<BudgetItemDto>>
{
    public async Task<List<BudgetItemDto>> Handle(
        GetAllBudgetItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.BudgetItems
            .Include(x => x.Budget)
            .Where(x => x.IsActive)
            .OrderBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<BudgetItemDto>>(items);

        foreach (var dto in dtos)
        {
            var entity = items.First(x => x.Id == dto.Id);
            dto.BudgetName = entity.Budget.BudgetName;
        }

        return dtos;
    }
}
