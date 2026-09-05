using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.Funds;

[Authorize(Policy = PermissionCodes.FundsView)]
public record GetFundByIdQuery(int Id) : IRequest<FundDto>;

public class GetFundByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFundByIdQuery, FundDto>
{
    public async Task<FundDto> Handle(
        GetFundByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Funds
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Domain.Budgeting.Entities.Fund), request.Id)
            : mapper.Map<FundDto>(entity);
    }
}
