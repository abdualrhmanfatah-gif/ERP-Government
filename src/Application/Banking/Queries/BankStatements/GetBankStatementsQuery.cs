using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankStatements;

// Q-B001 — GetBankStatementsQuery
[Authorize(Policy = PermissionCodes.BankStatementsView)]
public class GetBankStatementsQuery : IRequest<List<BankStatementDto>>
{
    public int? BankAccountId { get; init; }
    public string? Status { get; init; }
}

public class GetBankStatementsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankStatementsQuery, List<BankStatementDto>>
{
    public async Task<List<BankStatementDto>> Handle(
        GetBankStatementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.BankStatements.AsQueryable();

        if (request.BankAccountId.HasValue)
            query = query.Where(x => x.BankAccountId == request.BankAccountId.Value);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status.ToString() == request.Status);

        var items = await query
            .OrderByDescending(x => x.StatementDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BankStatementDto>>(items);
    }
}
