using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankReconciliations;

// Q-B004 — GetBankReconciliationByIdQuery
[Authorize(Policy = PermissionCodes.BankReconciliationView)]
public class GetBankReconciliationByIdQuery : IRequest<Result<BankReconciliationDto>>
{
    public int Id { get; init; }
}

public class GetBankReconciliationByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankReconciliationByIdQuery, Result<BankReconciliationDto>>
{
    public async Task<Result<BankReconciliationDto>> Handle(
        GetBankReconciliationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankReconciliations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<BankReconciliationDto>.Failure(ErrorCodes.Banking.BankReconciliationNotFound, ErrorCategory.NotFound, $"Bank reconciliation with ID {request.Id} not found.");

        return Result<BankReconciliationDto>.Success(mapper.Map<BankReconciliationDto>(entity));
    }
}
