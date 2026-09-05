using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsView)]
public record GetBudgetClassificationByIdQuery(int Id) : IRequest<BudgetClassificationDto>;

public class GetBudgetClassificationByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetClassificationByIdQuery, BudgetClassificationDto>
{
    public async Task<BudgetClassificationDto> Handle(
        GetBudgetClassificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetClassifications
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.BudgetClassification), request.Id)
            : mapper.Map<BudgetClassificationDto>(entity);
    }
}
