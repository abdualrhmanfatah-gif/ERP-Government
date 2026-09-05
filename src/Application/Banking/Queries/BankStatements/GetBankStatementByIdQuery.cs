using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankStatements;

// Q-B002 — GetBankStatementByIdQuery
[Authorize(Policy = PermissionCodes.BankStatementsView)]
public class GetBankStatementByIdQuery : IRequest<BankStatementDto?>
{
    public int Id { get; init; }
}

public class GetBankStatementByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankStatementByIdQuery, BankStatementDto?>
{
    public async Task<BankStatementDto?> Handle(
        GetBankStatementByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankStatements
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<BankStatementDto>(entity);
    }
}
