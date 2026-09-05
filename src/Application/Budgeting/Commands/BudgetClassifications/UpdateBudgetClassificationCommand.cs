using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Commands.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsUpdate)]
public record UpdateBudgetClassificationCommand(
    int Id,
    string Code,
    string Name,
    int? ParentId,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetClassificationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetClassificationCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBudgetClassificationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetClassifications
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget classification not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var codeExists = await context.BudgetClassifications
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(["Budget classification code already exists."]);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                return Result.Failure(["Budget classification cannot be its own parent."]);

            var parentExists = await context.BudgetClassifications
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
                return Result.Failure(["Parent budget classification not found."]);
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.ParentId = request.ParentId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBudgetClassificationCommandValidator : AbstractValidator<UpdateBudgetClassificationCommand>
{
    public UpdateBudgetClassificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid budget classification ID.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
