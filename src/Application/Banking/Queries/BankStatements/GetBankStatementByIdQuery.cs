using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Banking.Common.DTOs;

namespace ERP_Government.Application.Banking.Queries.BankStatements;

// Q-B002 — GetBankStatementByIdQuery
[Authorize(Policy = PermissionCodes.BankStatementsView)]
public class GetBankStatementByIdQuery : IRequest<Result<BankStatementDto>>
{
    public int Id { get; init; }
}

public class GetBankStatementByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBankStatementByIdQuery, Result<BankStatementDto>>
{
    public async Task<Result<BankStatementDto>> Handle(
        GetBankStatementByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankStatements
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<BankStatementDto>.Failure(ErrorCodes.Banking.StatementNotFound, ErrorCategory.NotFound, $"Bank statement with ID {request.Id} not found.");

        return Result<BankStatementDto>.Success(mapper.Map<BankStatementDto>(entity));
    }
}
