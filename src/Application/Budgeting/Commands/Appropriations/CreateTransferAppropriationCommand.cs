using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Budgeting.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsCreate)]
public record CreateTransferAppropriationCommand(
    int BudgetId,
    int SourceBudgetItemId,
    int TargetBudgetItemId,
    decimal Amount) : IRequest<Result<(int SourceId, int TargetId)>>;

public class CreateTransferAppropriationCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IBudgetAvailabilityService availabilityService) : IRequestHandler<CreateTransferAppropriationCommand, Result<(int SourceId, int TargetId)>>
{
    public async Task<Result<(int SourceId, int TargetId)>> Handle(
        CreateTransferAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var sourceItem = await context.BudgetItems
            .FirstOrDefaultAsync(bi => bi.Id == request.SourceBudgetItemId && bi.BudgetId == request.BudgetId, cancellationToken);

        if (sourceItem is null)
            return Result<(int, int)>.Failure(["Source budget item not found in the specified budget."]);

        var targetItem = await context.BudgetItems
            .FirstOrDefaultAsync(bi => bi.Id == request.TargetBudgetItemId && bi.BudgetId == request.BudgetId, cancellationToken);

        if (targetItem is null)
            return Result<(int, int)>.Failure(["Target budget item not found in the specified budget."]);

        if (request.SourceBudgetItemId == request.TargetBudgetItemId)
            return Result<(int, int)>.Failure(["Source and target items must be different."]);

        if (request.Amount <= 0)
            return Result<(int, int)>.Failure(["Amount must be greater than zero."]);

        var available = await availabilityService.GetAvailableForAppropriationAsync(request.SourceBudgetItemId);
        if (available < request.Amount)
            return Result<(int, int)>.Failure([$"Insufficient availability. Available: {available:C}, Requested: {request.Amount:C}."]);

        var sourceNumber = await sequenceService.GenerateNextNumberAsync("Appropriation", cancellationToken);
        var targetNumber = await sequenceService.GenerateNextNumberAsync("Appropriation", cancellationToken);

        var sourceRow = new Domain.Budgeting.Entities.Appropriation
        {
            AppropriationNumber = sourceNumber,
            BudgetId = request.BudgetId,
            BudgetItemId = request.SourceBudgetItemId,
            AppropriationType = AppropriationType.Transfer,
            DocumentType = "Transfer",
            DocumentId = 0,
            Amount = -request.Amount,
            TargetBudgetItemId = request.TargetBudgetItemId,
            Status = AppropriationStatus.Draft
        };

        var targetRow = new Domain.Budgeting.Entities.Appropriation
        {
            AppropriationNumber = targetNumber,
            BudgetId = request.BudgetId,
            BudgetItemId = request.TargetBudgetItemId,
            AppropriationType = AppropriationType.Transfer,
            DocumentType = "Transfer",
            DocumentId = 0,
            Amount = request.Amount,
            TargetBudgetItemId = request.SourceBudgetItemId,
            Status = AppropriationStatus.Draft
        };

        context.Appropriations.Add(sourceRow);
        context.Appropriations.Add(targetRow);
        await context.SaveChangesAsync(cancellationToken);

        return Result<(int, int)>.Success((sourceRow.Id, targetRow.Id));
    }
}

public class CreateTransferAppropriationCommandValidator : AbstractValidator<CreateTransferAppropriationCommand>
{
    public CreateTransferAppropriationCommandValidator()
    {
        RuleFor(x => x.BudgetId).GreaterThan(0);
        RuleFor(x => x.SourceBudgetItemId).GreaterThan(0);
        RuleFor(x => x.TargetBudgetItemId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
