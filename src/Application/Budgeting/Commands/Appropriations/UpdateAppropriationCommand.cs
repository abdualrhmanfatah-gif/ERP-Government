using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsUpdate)]
public record UpdateAppropriationCommand(
    int Id,
    AppropriationType AppropriationType,
    string DocumentType,
    int DocumentId,
    decimal Amount,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateAppropriationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAppropriationCommand, Result>
{
    public async Task<Result> Handle(
        UpdateAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Draft)
            return Result.Failure(["Only Draft appropriations can be updated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        if (request.AppropriationType == AppropriationType.Transfer)
            return Result.Failure(["Transfer type is only valid in Draft status. Use Adjustment type after submission."]);

        if (request.Amount <= 0)
            return Result.Failure(["Amount must be greater than zero."]);

        entity.AppropriationType = request.AppropriationType;
        entity.DocumentType = request.DocumentType;
        entity.DocumentId = request.DocumentId;
        entity.Amount = request.Amount;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateAppropriationCommandValidator : AbstractValidator<UpdateAppropriationCommand>
{
    public UpdateAppropriationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid appropriation ID.");

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
