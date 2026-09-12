using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using FluentValidation;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.UpdateBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsUpdate)]
public record UpdateBudgetTransactionCommand(
    int Id,
    DateOnly? TransactionDate,
    string? DocumentType,
    int? DocumentId,
    string? Description,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetTransactionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(UpdateBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Draft)
            return Result.Failure(["Only Draft transactions can be updated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        if (request.TransactionDate.HasValue) entity.TransactionDate = request.TransactionDate.Value;
        if (request.DocumentType is not null) entity.DocumentType = request.DocumentType;
        if (request.DocumentId.HasValue) entity.DocumentId = request.DocumentId;
        if (request.Description is not null) entity.Description = request.Description;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateBudgetTransactionCommandValidator : AbstractValidator<UpdateBudgetTransactionCommand>
{
    public UpdateBudgetTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Transaction ID must be greater than 0.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required.");
    }
}
