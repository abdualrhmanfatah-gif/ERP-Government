using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesCreate)]
public record CreateEncumbranceCommand(
    EncumbranceType EncumbranceType,
    int? VendorId,
    int? PurchaseOrderId,
    string? DocumentType,
    int? DocumentId,
    string? Description,
    DateOnly EncumbranceDate,
    List<EncumbranceLineRequest> Lines) : IRequest<Result<int>>;

public record EncumbranceLineRequest(
    int BudgetItemId,
    decimal Amount,
    string? Description);

public class CreateEncumbranceCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IDocumentStatusLogger statusLogger,
    IBudgetAvailabilityService availabilityService,
    IUser user) : IRequestHandler<CreateEncumbranceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateEncumbranceCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(["User identity is required for this operation."]);

        if (request.Lines is null || request.Lines.Count == 0)
            return Result<int>.Failure(["At least one encumbrance line is required."]);

        var totalAmount = request.Lines.Sum(l => l.Amount);
        if (totalAmount <= 0)
            return Result<int>.Failure(["Total amount must be greater than zero."]);

        var budgetItemIds = request.Lines.Select(l => l.BudgetItemId).Distinct().ToList();
        var existingItems = await context.BudgetItems
            .Include(bi => bi.Budget)
            .Where(bi => budgetItemIds.Contains(bi.Id))
            .ToListAsync(cancellationToken);

        var missingIds = budgetItemIds.Except(existingItems.Select(bi => bi.Id)).ToList();
        if (missingIds.Count > 0)
            return Result<int>.Failure([$"Budget items not found: {string.Join(", ", missingIds)}"]);

        var inactiveBudgetItems = existingItems
            .Where(bi => bi.Budget.Status != BudgetStatus.Active)
            .ToList();

        if (inactiveBudgetItems.Count > 0)
        {
            var itemNames = string.Join(", ", inactiveBudgetItems.Select(bi => bi.ItemCode));
            return Result<int>.Failure([$"Budget items belong to inactive budgets: {itemNames}"]);
        }

        var allocationIds = await context.BudgetItemAllocations
            .Where(a => budgetItemIds.Contains(a.BudgetItemId))
            .Select(a => new { a.Id, a.BudgetItemId })
            .ToListAsync(cancellationToken);

        foreach (var line in request.Lines)
        {
            var allocation = allocationIds.FirstOrDefault(a => a.BudgetItemId == line.BudgetItemId);
            if (allocation is null)
                return Result<int>.Failure([$"No allocation found for budget item {line.BudgetItemId}."]);

            var available = await availabilityService.GetAvailableForAppropriationAsync(allocation.Id);
            if (available < line.Amount)
                return Result<int>.Failure([$"Insufficient budget for item {line.BudgetItemId}. Available: {available:C}, Requested: {line.Amount:C}."]);
        }

        var encumbranceNumber = await sequenceService.GenerateNextNumberAsync("Encumbrance", cancellationToken);

        var entity = new Domain.Budgeting.Entities.Encumbrance
        {
            EncumbranceNumber = encumbranceNumber,
            EncumbranceType = request.EncumbranceType,
            VendorPartyId = request.VendorId,
            PurchaseOrderId = request.PurchaseOrderId,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            Description = request.Description,
            EncumbranceDate = request.EncumbranceDate,
            TotalAmount = totalAmount,
            Status = EncumbranceStatus.Draft
        };

        context.Encumbrances.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var encumbranceLines = request.Lines.Select(l => new Domain.Budgeting.Entities.EncumbranceLine
        {
            EncumbranceId = entity.Id,
            BudgetItemId = l.BudgetItemId,
            Amount = l.Amount,
            Description = l.Description
        }).ToList();

        context.EncumbranceLines.AddRange(encumbranceLines);
        await context.SaveChangesAsync(cancellationToken);

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            "",
            EncumbranceStatus.Draft.ToString(),
            userId,
            null,
            cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateEncumbranceCommandValidator : AbstractValidator<CreateEncumbranceCommand>
{
    public CreateEncumbranceCommandValidator()
    {
        RuleFor(x => x.EncumbranceType)
            .IsInEnum().WithMessage("Invalid encumbrance type.");

        RuleFor(x => x.EncumbranceDate)
            .NotEmpty().WithMessage("Encumbrance date is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one encumbrance line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.BudgetItemId)
                .GreaterThan(0).WithMessage("Budget item ID must be greater than 0.");
            line.RuleFor(l => l.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        });
    }
}
