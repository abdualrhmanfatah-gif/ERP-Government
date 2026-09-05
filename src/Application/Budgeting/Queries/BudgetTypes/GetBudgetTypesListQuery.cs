using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesView)]
public record GetBudgetTypesListQuery : IRequest<List<BudgetTypeDto>>;

public class GetBudgetTypesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetTypesListQuery, List<BudgetTypeDto>>
{
    public async Task<List<BudgetTypeDto>> Handle(
        GetBudgetTypesListQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.BudgetTypes
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BudgetTypeDto>>(items);
    }
}
