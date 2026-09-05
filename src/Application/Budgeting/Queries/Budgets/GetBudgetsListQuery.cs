using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsView)]
public record GetBudgetsListQuery(
    int? FiscalYearId = null,
    int? FundId = null,
    int? BudgetTypeId = null,
    BudgetStatus? Status = null) : IRequest<List<BudgetDto>>;

public class GetBudgetsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetsListQuery, List<BudgetDto>>
{
    public async Task<List<BudgetDto>> Handle(
        GetBudgetsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Budgets
            .Include(x => x.BudgetType)
            .Include(x => x.Fund)
            .AsQueryable();

        if (request.FiscalYearId.HasValue)
            query = query.Where(x => x.FiscalYearId == request.FiscalYearId.Value);

        if (request.FundId.HasValue)
            query = query.Where(x => x.FundId == request.FundId.Value);

        if (request.BudgetTypeId.HasValue)
            query = query.Where(x => x.BudgetTypeId == request.BudgetTypeId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var items = await query
            .OrderBy(x => x.BudgetNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BudgetDto>>(items);
    }
}
