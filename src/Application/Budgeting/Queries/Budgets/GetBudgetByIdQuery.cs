using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsView)]
public record GetBudgetByIdQuery(int Id) : IRequest<BudgetDto>;

public class GetBudgetByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    public async Task<BudgetDto> Handle(
        GetBudgetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Budgets
            .Include(x => x.BudgetType)
            .Include(x => x.Fund)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.Budget), request.Id)
            : mapper.Map<BudgetDto>(entity);
    }
}
