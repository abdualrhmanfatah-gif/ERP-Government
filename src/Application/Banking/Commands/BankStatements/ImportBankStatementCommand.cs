using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;
using ERP_Government.Domain.Events.Banking;

namespace ERP_Government.Application.Banking.Commands.BankStatements;

// C-B002 — ImportBankStatementCommand
[Authorize(Policy = PermissionCodes.BankStatementsImport)]
public class ImportBankStatementCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ImportBankStatementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ImportBankStatementCommand, Result>
{
    public async Task<Result> Handle(
        ImportBankStatementCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankStatements
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank statement not found."]);

        if (entity.Status == BankStatementStatus.Imported)
            return Result.Success(); // Idempotent

        if (entity.Status != BankStatementStatus.Draft)
            return Result.Failure(["Only draft statements can be imported."]);

        entity.Status = BankStatementStatus.Imported;

        entity.AddDomainEvent(new BankStatementImported
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            BankAccountId = entity.BankAccountId,
            StatementDate = entity.StatementDate
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
