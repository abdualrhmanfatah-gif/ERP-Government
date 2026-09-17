using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsView)]
public record GetBudgetClassificationByIdQuery(int Id) : IRequest<Result<BudgetClassificationDto>>;

public class GetBudgetClassificationByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetClassificationByIdQuery, Result<BudgetClassificationDto>>
{
    public async Task<Result<BudgetClassificationDto>> Handle(
        GetBudgetClassificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetClassifications
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<BudgetClassificationDto>.Failure(ErrorCodes.Budgets.ClassificationNotFound, ErrorCategory.NotFound, $"Budget classification with ID {request.Id} not found.");

        return Result<BudgetClassificationDto>.Success(mapper.Map<BudgetClassificationDto>(entity));
    }
}
