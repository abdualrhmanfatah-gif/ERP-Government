using ERP_Government.Application.Budgeting.Common;
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
    int AppropriationId,
    EncumbranceType EncumbranceType,
    int? VendorId,
    int? PurchaseOrderId,
    string DocumentType,
    int DocumentId,
    string? Description,
    DateOnly EncumbranceDate,
    decimal Amount) : IRequest<Result<int>>;

public class CreateEncumbranceCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IBudgetAvailabilityService availabilityService,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CreateEncumbranceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateEncumbranceCommand request,
        CancellationToken cancellationToken)
    {
        var appropriation = await context.Appropriations
            .Include(a => a.BudgetItem)
            .ThenInclude(bi => bi!.Budget)
            .ThenInclude(b => b!.BudgetType)
            .FirstOrDefaultAsync(a => a.Id == request.AppropriationId, cancellationToken);

        if (appropriation is null)
            return Result<int>.Failure(["Appropriation not found."]);

        var budget = appropriation.BudgetItem?.Budget;
        var controlMethod = budget?.BudgetType?.ControlMethod ?? BudgetControlMethod.None;

        var available = await availabilityService.GetAvailableForEncumbranceAsync(request.AppropriationId);
        var (allowed, warning) = availabilityService.EvaluateControlMethod(controlMethod, request.Amount, available);

        if (!allowed)
            return Result<int>.Failure([$"Encumbrance blocked: {warning}"]);

        var encumbranceNumber = await sequenceService.GenerateNextNumberAsync("Encumbrance", cancellationToken);

        var entity = new Domain.Budgeting.Entities.Encumbrance
        {
            EncumbranceNumber = encumbranceNumber,
            EncumbranceType = request.EncumbranceType,
            AppropriationId = request.AppropriationId,
            VendorId = request.VendorId,
            PurchaseOrderId = request.PurchaseOrderId,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            Description = request.Description,
            EncumbranceDate = request.EncumbranceDate,
            Amount = request.Amount,
            Status = EncumbranceStatus.Draft
        };

        context.Encumbrances.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            "",
            EncumbranceStatus.Draft.ToString(),
            0,
            null,
            cancellationToken);

        if (warning is not null)
        {
            context.ApprovalHistory.Add(new ApprovalHistory
            {
                DocumentType = "Encumbrance",
                DocumentId = entity.Id,
                ApproverUserId = 0,
                RequiredRole = "",
                Decision = "Created (Warning override)",
                DecisionAt = DateTimeOffset.UtcNow,
                Reason = warning
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(entity.Id);
    }
}

public class CreateEncumbranceCommandValidator : AbstractValidator<CreateEncumbranceCommand>
{
    public CreateEncumbranceCommandValidator()
    {
        RuleFor(x => x.AppropriationId)
            .GreaterThan(0).WithMessage("Appropriation ID must be greater than 0.");

        RuleFor(x => x.EncumbranceType)
            .IsInEnum().WithMessage("Invalid encumbrance type.");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.");

        RuleFor(x => x.DocumentId)
            .GreaterThan(0).WithMessage("Document ID must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
