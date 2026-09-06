using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Budgeting.Queries.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsView)]
public record GetBudgetByIdQuery(int Id) : IRequest<BudgetDto>;

public class GetBudgetByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<GetBudgetByIdQueryHandler> logger) : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    public async Task<BudgetDto> Handle(
        GetBudgetByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GetBudgetByIdQuery called with Id={Id}", request.Id);

        var entity = await context.Budgets
            .Include(x => x.BudgetType)
            .Include(x => x.Fund)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            logger.LogWarning("Budget not found for Id={Id}", request.Id);
            throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.Budget), request.Id);
        }

        logger.LogInformation("Budget found for Id={Id}", request.Id);
        return mapper.Map<BudgetDto>(entity);
    }
}
