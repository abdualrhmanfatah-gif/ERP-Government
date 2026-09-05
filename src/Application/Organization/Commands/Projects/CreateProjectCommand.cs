using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Application.Organization.Commands.Projects;

// C-O009 — CreateProjectCommand
[Authorize(Policy = PermissionCodes.ProjectsCreate)]
public class CreateProjectCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public int? CostCenterId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
}

public class CreateProjectCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateProjectCommand, Result>
{
    public async Task<Result> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Projects
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Project code already exists."]);

        // FK validation
        if (request.CostCenterId.HasValue)
        {
            var costCenterExists = await context.CostCenters
                .AnyAsync(x => x.Id == request.CostCenterId.Value, cancellationToken);

            if (!costCenterExists)
                return Result.Failure(["Cost center not found."]);
        }

        var entity = new Project
        {
            Code = request.Code,
            Name = request.Name,
            FundId = request.FundId,
            CostCenterId = request.CostCenterId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BudgetAmount = request.BudgetAmount,
            Status = ProjectStatus.Draft,
            IsActive = true
        };

        context.Projects.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.BudgetAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Budget amount must be non-negative.")
            .When(x => x.BudgetAmount.HasValue);
    }
}
