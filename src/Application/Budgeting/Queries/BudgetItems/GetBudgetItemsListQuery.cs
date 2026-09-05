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

        return mapper.Map<List<BudgetItemDto>>(items);
    }
}
