using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.CostCenters;

// C-O006 — CreateCostCenterCommand
[Authorize(Policy = PermissionCodes.CostCentersCreate)]
public class CreateCostCenterCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? OrganizationUnitId { get; init; }
    public decimal? BudgetLimit { get; init; }
}

public class CreateCostCenterCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateCostCenterCommand, Result>
{
    public async Task<Result> Handle(
        CreateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.CostCenters
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Cost center code already exists."]);

        // FK validation
        if (request.OrganizationUnitId.HasValue)
        {
            var orgUnitExists = await context.OrganizationalUnits
                .AnyAsync(x => x.Id == request.OrganizationUnitId.Value, cancellationToken);

            if (!orgUnitExists)
                return Result.Failure(["Organizational unit not found."]);
        }

        var entity = new CostCenter
        {
            Code = request.Code,
            Name = request.Name,
            OrganizationUnitId = request.OrganizationUnitId,
            BudgetLimit = request.BudgetLimit,
            IsActive = true
        };

        context.CostCenters.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.BudgetLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Budget limit must be non-negative.")
            .When(x => x.BudgetLimit.HasValue);
    }
}
