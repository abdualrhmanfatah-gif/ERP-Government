using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalYears;

// Q-F004 — GetFiscalYearByIdQuery
[Authorize(Policy = PermissionCodes.FiscalYearsView)]
public class GetFiscalYearByIdQuery : IRequest<Result<FiscalYearDto>>
{
    public int Id { get; init; }
}

public class GetFiscalYearByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetFiscalYearByIdQuery, Result<FiscalYearDto>>
{
    public async Task<Result<FiscalYearDto>> Handle(
        GetFiscalYearByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalYears
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<FiscalYearDto>.Failure(ErrorCodes.FinancialSettings.FiscalYearNotFound, ErrorCategory.NotFound, $"Fiscal year with ID {request.Id} not found.");

        return Result<FiscalYearDto>.Success(mapper.Map<FiscalYearDto>(entity));
    }
}
