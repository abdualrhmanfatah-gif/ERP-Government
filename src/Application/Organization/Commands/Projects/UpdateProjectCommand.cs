using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Application.Organization.Commands.Projects;

// C-O010 — UpdateProjectCommand with state machine enforcement (FR-017)
[Authorize(Policy = PermissionCodes.ProjectsUpdate)]
public class UpdateProjectCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public int? CostCenterId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public class UpdateProjectCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateProjectCommand, Result>
{
    // FR-017: Allowed transitions per spec State Machines section
    private static readonly Dictionary<ProjectStatus, HashSet<ProjectStatus>> AllowedTransitions = new()
    {
        [ProjectStatus.Draft] = [ProjectStatus.Active, ProjectStatus.OnHold, ProjectStatus.Cancelled],
        [ProjectStatus.Active] = [ProjectStatus.OnHold, ProjectStatus.Completed, ProjectStatus.Cancelled],
        [ProjectStatus.OnHold] = [ProjectStatus.Active, ProjectStatus.Completed, ProjectStatus.Cancelled],
    };

    public async Task<Result> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Projects
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Project not found."]);

        // Duplicate Code check (excluding self)
        var codeExists = await context.Projects
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(["Project code already exists."]);

        // FK validation
        if (request.CostCenterId.HasValue)
        {
            var costCenterExists = await context.CostCenters
                .AnyAsync(x => x.Id == request.CostCenterId.Value, cancellationToken);

            if (!costCenterExists)
                return Result.Failure(["Cost center not found."]);
        }

        // State machine enforcement (FR-017)
        if (Enum.TryParse<ProjectStatus>(request.Status, out var newStatus))
        {
            if (entity.Status != newStatus)
            {
                if (!AllowedTransitions.TryGetValue(entity.Status, out var allowed) || !allowed.Contains(newStatus))
                {
                    return Result.Failure([$"Invalid status transition from {entity.Status} to {newStatus}."]);
                }

                // StartDate required for Draft→Active
                if (entity.Status == ProjectStatus.Draft && newStatus == ProjectStatus.Active && !entity.StartDate.HasValue)
                {
                    return Result.Failure(["Start date is required to activate a project."]);
                }

                entity.Status = newStatus;
            }
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.FundId = request.FundId;
        entity.CostCenterId = request.CostCenterId;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.BudgetAmount = request.BudgetAmount;
        entity.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid project ID.");

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

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => Enum.TryParse<ProjectStatus>(s, out _))
            .WithMessage("Invalid project status.");
    }
}
