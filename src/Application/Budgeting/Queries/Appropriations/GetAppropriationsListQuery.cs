using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsView)]
public record GetAppropriationsListQuery(
    int? BudgetId = null,
    int? BudgetItemId = null,
    AppropriationStatus? Status = null,
    AppropriationType? AppropriationType = null) : IRequest<List<AppropriationDto>>;

public class GetAppropriationsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAppropriationsListQuery, List<AppropriationDto>>
{
    public async Task<List<AppropriationDto>> Handle(
        GetAppropriationsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Appropriations
            .Include(x => x.Budget)
                .ThenInclude(x => x.Fund)
            .Include(x => x.Budget)
                .ThenInclude(x => x.FiscalYear)
            .Include(x => x.BudgetItem)
            .AsQueryable();

        if (request.BudgetId.HasValue)
            query = query.Where(x => x.BudgetId == request.BudgetId.Value);

        if (request.BudgetItemId.HasValue)
            query = query.Where(x => x.BudgetItemId == request.BudgetItemId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.AppropriationType.HasValue)
            query = query.Where(x => x.AppropriationType == request.AppropriationType.Value);

        var items = await query
            .OrderBy(x => x.AppropriationNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<AppropriationDto>>(items);
    }
}
