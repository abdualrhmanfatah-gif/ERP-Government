using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Banking.Commands.BankStatements;

// C-B007 — CreateBankStatementLineCommand
[Authorize(Policy = PermissionCodes.BankStatementsCreate)]
public class CreateBankStatementLineCommand : IRequest<Result>
{
    public int StatementId { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? Description { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal? Balance { get; init; }
    public string? Reference { get; init; }
}

public class CreateBankStatementLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBankStatementLineCommand, Result>
{
    public async Task<Result> Handle(
        CreateBankStatementLineCommand request,
        CancellationToken cancellationToken)
    {
        var statement = await context.BankStatements
            .FindAsync(request.StatementId, cancellationToken);

        if (statement is null)
            return Result.Failure(["Statement not found."]);

        var maxLineNumber = await context.BankStatementLines
            .Where(l => l.StatementId == request.StatementId)
            .MaxAsync(l => (int?)l.LineNumber, cancellationToken) ?? 0;

        var entity = new Domain.Banking.Entities.BankStatementLine
        {
            StatementId = request.StatementId,
            LineNumber = maxLineNumber + 1,
            TransactionDate = request.TransactionDate,
            Description = request.Description,
            Debit = request.Debit,
            Credit = request.Credit,
            Balance = request.Balance,
            Reference = request.Reference,
            IsReconciled = false
        };

        context.BankStatementLines.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateBankStatementLineCommandValidator : AbstractValidator<CreateBankStatementLineCommand>
{
    public CreateBankStatementLineCommandValidator()
    {
        RuleFor(x => x.StatementId)
            .GreaterThan(0).WithMessage("Statement ID is required.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Debit must be non-negative.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit must be non-negative.");

        RuleFor(x => x)
            .Must(x => x.Debit > 0 || x.Credit > 0)
            .WithMessage("Either Debit or Credit must be greater than zero.");
    }
}
