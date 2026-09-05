using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesCreate)]
public record CreateBudgetTypeCommand(
    string Code,
    string Name,
    string? Description,
    BudgetControlMethod ControlMethod,
    bool AllowOverrun) : IRequest<Result<int>>;

public class CreateBudgetTypeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBudgetTypeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetTypeCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.BudgetTypes
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result<int>.Failure(["Budget type code already exists."]);

        var entity = new Domain.Budgeting.Entities.BudgetType
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ControlMethod = request.ControlMethod,
            AllowOverrun = request.AllowOverrun,
            IsActive = true
        };

        context.BudgetTypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetTypeCommandValidator : AbstractValidator<CreateBudgetTypeCommand>
{
    public CreateBudgetTypeCommandValidator()
    {
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
