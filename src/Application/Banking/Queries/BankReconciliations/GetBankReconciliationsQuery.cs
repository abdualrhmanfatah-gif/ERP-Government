using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankReconciliations;

// Q-B003 — GetBankReconciliationsQuery
[Authorize(Policy = PermissionCodes.BankReconciliationView)]
public class GetBankReconciliationsQuery : IRequest<List<BankReconciliationDto>>
{
    public int? BankAccountId { get; init; }
    public string? Status { get; init; }
}

public class GetBankReconciliationsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankReconciliationsQuery, List<BankReconciliationDto>>
{
    public async Task<List<BankReconciliationDto>> Handle(
        GetBankReconciliationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.BankReconciliations.AsQueryable();

        if (request.BankAccountId.HasValue)
            query = query.Where(x => x.BankAccountId == request.BankAccountId.Value);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status.ToString() == request.Status);

        var items = await query
            .OrderByDescending(x => x.ReconciliationDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BankReconciliationDto>>(items);
    }
}
