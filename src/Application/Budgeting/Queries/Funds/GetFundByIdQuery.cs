using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.Funds;

[Authorize(Policy = PermissionCodes.FundsView)]
public record GetFundByIdQuery(int Id) : IRequest<Result<FundDto>>;

public class GetFundByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFundByIdQuery, Result<FundDto>>
{
    public async Task<Result<FundDto>> Handle(
        GetFundByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Funds
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<FundDto>.Failure(ErrorCodes.Budgets.FundNotFound, ErrorCategory.NotFound, $"Fund with ID {request.Id} not found.");

        return Result<FundDto>.Success(mapper.Map<FundDto>(entity));
    }
}
