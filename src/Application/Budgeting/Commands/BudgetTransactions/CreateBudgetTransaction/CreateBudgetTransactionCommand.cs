using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsCreate)]
public record CreateBudgetTransactionCommand(
    int BudgetItemAllocationId,
    BudgetTransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount,
    TransactionDirection Direction,
    string? DocumentType,
    int? DocumentId,
    string? Description) : IRequest<Result<int>>;

public class CreateBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<CreateBudgetTransactionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(["User identity is required for this operation."]);

        var allocation = await context.BudgetItemAllocations
            .Include(a => a.Budget)
            .FirstOrDefaultAsync(a => a.Id == request.BudgetItemAllocationId, cancellationToken);

        if (allocation is null)
            return Result<int>.Failure(["المخصص غير موجود"]);

        if (allocation.Budget.Status != BudgetStatus.Active)
            return Result<int>.Failure(["الموازنة غير مفعلة"]);

        if (request.Amount <= 0)
            return Result<int>.Failure(["المبلغ يجب أن يكون أكبر من صفر"]);

        var transactionNumber = await sequenceService.GenerateNextNumberAsync("BudgetTransaction", cancellationToken);

        var entity = new Domain.Budgeting.Entities.BudgetTransaction
        {
            TransactionNumber = transactionNumber,
            BudgetId = allocation.BudgetId,
            BudgetItemAllocationId = request.BudgetItemAllocationId,
            TransactionType = request.TransactionType,
            TransactionDate = request.TransactionDate,
            Amount = request.Amount,
            Direction = request.Direction,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            Description = request.Description,
            Status = BudgetTransactionStatus.Draft
        };

        context.BudgetTransactions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            "",
            BudgetTransactionStatus.Draft.ToString(),
            userId,
            null,
            cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetTransactionCommandValidator : AbstractValidator<CreateBudgetTransactionCommand>
{
    public CreateBudgetTransactionCommandValidator()
    {
        RuleFor(x => x.BudgetItemAllocationId)
            .GreaterThan(0).WithMessage("Budget Item Allocation ID must be greater than 0.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Invalid transaction type.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("Invalid transaction direction.");
    }
}
