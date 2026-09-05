using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesView)]
public record GetBudgetTypeByIdQuery(int Id) : IRequest<BudgetTypeDto>;

public class GetBudgetTypeByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetTypeByIdQuery, BudgetTypeDto>
{
    public async Task<BudgetTypeDto> Handle(
        GetBudgetTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.BudgetType), request.Id)
            : mapper.Map<BudgetTypeDto>(entity);
    }
}
