using ERP_Government.Application.Banking.Common.DTOs;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Banking.Queries.BankReconciliationLines;

// Q-B006 — GetBankReconciliationLinesQuery
[Authorize(Policy = PermissionCodes.BankReconciliationView)]
public class GetBankReconciliationLinesQuery : IRequest<List<BankReconciliationLineDto>>
{
    public int ReconciliationId { get; init; }
}

public class GetBankReconciliationLinesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankReconciliationLinesQuery, List<BankReconciliationLineDto>>
{
    public async Task<List<BankReconciliationLineDto>> Handle(
        GetBankReconciliationLinesQuery request,
        CancellationToken cancellationToken)
    {
        var lines = await context.BankReconciliationLines
            .Where(l => l.ReconciliationId == request.ReconciliationId)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BankReconciliationLineDto>>(lines);
    }
}
