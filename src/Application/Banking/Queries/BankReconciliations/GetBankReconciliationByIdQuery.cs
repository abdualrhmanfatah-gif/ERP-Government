using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankReconciliations;

// Q-B004 — GetBankReconciliationByIdQuery
[Authorize(Policy = PermissionCodes.BankReconciliationView)]
public class GetBankReconciliationByIdQuery : IRequest<BankReconciliationDto?>
{
    public int Id { get; init; }
}

public class GetBankReconciliationByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankReconciliationByIdQuery, BankReconciliationDto?>
{
    public async Task<BankReconciliationDto?> Handle(
        GetBankReconciliationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankReconciliations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<BankReconciliationDto>(entity);
    }
}
