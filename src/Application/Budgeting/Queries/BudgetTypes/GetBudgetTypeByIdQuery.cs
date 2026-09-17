using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesView)]
public record GetBudgetTypeByIdQuery(int Id) : IRequest<Result<BudgetTypeDto>>;

public class GetBudgetTypeByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetTypeByIdQuery, Result<BudgetTypeDto>>
{
    public async Task<Result<BudgetTypeDto>> Handle(
        GetBudgetTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<BudgetTypeDto>.Failure(ErrorCodes.Budgets.FundNotFound, ErrorCategory.NotFound, $"Budget type with ID {request.Id} not found.");

        return Result<BudgetTypeDto>.Success(mapper.Map<BudgetTypeDto>(entity));
    }
}
