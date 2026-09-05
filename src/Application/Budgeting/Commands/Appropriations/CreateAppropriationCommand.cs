using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsCreate)]
public record CreateAppropriationCommand(
    int BudgetId,
    int BudgetItemId,
    AppropriationType AppropriationType,
    string DocumentType,
    int DocumentId,
    decimal Amount) : IRequest<Result<int>>;

public class CreateAppropriationCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAppropriationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var budgetExists = await context.Budgets
            .AnyAsync(x => x.Id == request.BudgetId, cancellationToken);

        if (!budgetExists)
            return Result<int>.Failure(["Budget not found."]);

        var budgetItemExists = await context.BudgetItems
            .AnyAsync(x => x.Id == request.BudgetItemId && x.BudgetId == request.BudgetId, cancellationToken);

        if (!budgetItemExists)
            return Result<int>.Failure(["Budget item not found in the specified budget."]);

        if (request.AppropriationType == AppropriationType.Transfer)
            return Result<int>.Failure(["Transfer type is only valid in Draft status. Use Adjustment type after submission."]);

        if (request.Amount <= 0)
            return Result<int>.Failure(["Amount must be greater than zero."]);

        var appropriationNumber = await sequenceService.GenerateNextNumberAsync("Appropriation", cancellationToken);

        var entity = new Domain.Budgeting.Entities.Appropriation
        {
            AppropriationNumber = appropriationNumber,
            BudgetId = request.BudgetId,
            BudgetItemId = request.BudgetItemId,
            AppropriationType = request.AppropriationType,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            Amount = request.Amount,
            Status = AppropriationStatus.Draft
        };

        context.Appropriations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateAppropriationCommandValidator : AbstractValidator<CreateAppropriationCommand>
{
    public CreateAppropriationCommandValidator()
    {
        RuleFor(x => x.BudgetId)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0.");

        RuleFor(x => x.BudgetItemId)
            .GreaterThan(0).WithMessage("Budget item ID must be greater than 0.");

        RuleFor(x => x.AppropriationType)
            .IsInEnum().WithMessage("Invalid appropriation type.");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.")
            .MaximumLength(50).WithMessage("Document type must not exceed 50 characters.");

        RuleFor(x => x.DocumentId)
            .GreaterThan(0).WithMessage("Document ID must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
