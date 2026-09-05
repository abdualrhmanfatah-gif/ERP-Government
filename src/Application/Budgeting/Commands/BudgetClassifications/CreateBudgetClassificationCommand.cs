using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Commands.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsCreate)]
public record CreateBudgetClassificationCommand(
    string Code,
    string Name,
    int? ParentId) : IRequest<Result<int>>;

public class CreateBudgetClassificationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBudgetClassificationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBudgetClassificationCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.BudgetClassifications
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result<int>.Failure(["Budget classification code already exists."]);

        if (request.ParentId.HasValue)
        {
            var parentExists = await context.BudgetClassifications
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
                return Result<int>.Failure(["Parent budget classification not found."]);
        }

        var entity = new Domain.Budgeting.Entities.BudgetClassification
        {
            Code = request.Code,
            Name = request.Name,
            ParentId = request.ParentId,
            IsActive = true
        };

        context.BudgetClassifications.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateBudgetClassificationCommandValidator : AbstractValidator<CreateBudgetClassificationCommand>
{
    public CreateBudgetClassificationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
