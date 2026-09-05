using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesUpdate)]
public record UpdateBudgetTypeCommand(
    int Id,
    string Code,
    string Name,
    string? Description,
    BudgetControlMethod ControlMethod,
    bool AllowOverrun,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetTypeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetTypeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBudgetTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTypes
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget type not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var codeExists = await context.BudgetTypes
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(["Budget type code already exists."]);

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.ControlMethod = request.ControlMethod;
        entity.AllowOverrun = request.AllowOverrun;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBudgetTypeCommandValidator : AbstractValidator<UpdateBudgetTypeCommand>
{
    public UpdateBudgetTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid budget type ID.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
