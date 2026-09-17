using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Budgeting.Queries.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsView)]
public record GetBudgetByIdQuery(int Id) : IRequest<Result<BudgetDto>>;

public class GetBudgetByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<GetBudgetByIdQueryHandler> logger) : IRequestHandler<GetBudgetByIdQuery, Result<BudgetDto>>
{
    public async Task<Result<BudgetDto>> Handle(
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
            return Result<BudgetDto>.Failure(ErrorCodes.Budgets.BudgetNotFound, ErrorCategory.NotFound, $"Budget with ID {request.Id} not found.");
        }

        logger.LogInformation("Budget found for Id={Id}", request.Id);
        return Result<BudgetDto>.Success(mapper.Map<BudgetDto>(entity));
    }
}
