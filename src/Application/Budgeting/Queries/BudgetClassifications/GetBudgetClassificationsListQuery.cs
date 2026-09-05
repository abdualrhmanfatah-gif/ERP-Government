using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsView)]
public record GetBudgetClassificationsListQuery : IRequest<List<BudgetClassificationDto>>;

public class GetBudgetClassificationsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetClassificationsListQuery, List<BudgetClassificationDto>>
{
    public async Task<List<BudgetClassificationDto>> Handle(
        GetBudgetClassificationsListQuery request,
        CancellationToken cancellationToken)
    {
        var items = await context.BudgetClassifications
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BudgetClassificationDto>>(items);
    }
}
