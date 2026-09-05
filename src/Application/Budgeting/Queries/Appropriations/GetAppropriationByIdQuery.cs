using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsView)]
public record GetAppropriationByIdQuery(int Id) : IRequest<AppropriationDto>;

public class GetAppropriationByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAppropriationByIdQuery, AppropriationDto>
{
    public async Task<AppropriationDto> Handle(
        GetAppropriationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .Include(x => x.Budget)
                .ThenInclude(x => x.Fund)
            .Include(x => x.Budget)
                .ThenInclude(x => x.FiscalYear)
            .Include(x => x.BudgetItem)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.Appropriation), request.Id)
            : mapper.Map<AppropriationDto>(entity);
    }
}
