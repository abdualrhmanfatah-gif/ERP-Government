using ERP_Government.Application.Banking.Common.DTOs;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Banking.Queries.BankStatementLines;

// Q-B005 — GetBankStatementLinesQuery
[Authorize(Policy = PermissionCodes.BankStatementsView)]
public class GetBankStatementLinesQuery : IRequest<List<BankStatementLineDto>>
{
    public int StatementId { get; init; }
}

public class GetBankStatementLinesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankStatementLinesQuery, List<BankStatementLineDto>>
{
    public async Task<List<BankStatementLineDto>> Handle(
        GetBankStatementLinesQuery request,
        CancellationToken cancellationToken)
    {
        var lines = await context.BankStatementLines
            .Where(l => l.StatementId == request.StatementId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BankStatementLineDto>>(lines);
    }
}
