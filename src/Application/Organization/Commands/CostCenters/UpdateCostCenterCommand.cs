using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.CostCenters;

// C-O007 — UpdateCostCenterCommand
[Authorize(Policy = PermissionCodes.CostCentersUpdate)]
public class UpdateCostCenterCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? OrganizationUnitId { get; init; }
    public decimal? BudgetLimit { get; init; }
    public bool IsActive { get; init; } = true;
}

public class UpdateCostCenterCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateCostCenterCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CostCenters
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Cost center not found."]);

        // Duplicate Code check (excluding self)
        var codeExists = await context.CostCenters
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(["Cost center code already exists."]);

        // FK validation
        if (request.OrganizationUnitId.HasValue)
        {
            var orgUnitExists = await context.OrganizationalUnits
                .AnyAsync(x => x.Id == request.OrganizationUnitId.Value, cancellationToken);

            if (!orgUnitExists)
                return Result.Failure(["Organizational unit not found."]);
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.OrganizationUnitId = request.OrganizationUnitId;
        entity.BudgetLimit = request.BudgetLimit;
        entity.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateCostCenterCommandValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid cost center ID.");

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
