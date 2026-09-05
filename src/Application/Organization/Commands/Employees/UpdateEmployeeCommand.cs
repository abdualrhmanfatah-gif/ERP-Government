using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.Employees;

// C-O004 — UpdateEmployeeCommand
[Authorize(Policy = PermissionCodes.EmployeesUpdate)]
public class UpdateEmployeeCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string EmployeeNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int OrganizationalUnitId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string? JobGrade { get; init; }
    public DateOnly HireDate { get; init; }
    public string EmploymentStatus { get; init; } = "Active";
    public bool IsActive { get; init; } = true;
}

public class UpdateEmployeeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Employees
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Employee not found."]);

        // Duplicate EmployeeNumber check (excluding self)
        var numberExists = await context.Employees
            .AnyAsync(x => x.EmployeeNumber == request.EmployeeNumber && x.Id != request.Id, cancellationToken);

        if (numberExists)
            return Result.Failure(["Employee number already exists."]);

        // OrgUnit FK validation
        var orgUnitExists = await context.OrganizationalUnits
            .AnyAsync(x => x.Id == request.OrganizationalUnitId, cancellationToken);

        if (!orgUnitExists)
            return Result.Failure(["Organizational unit not found."]);

        entity.EmployeeNumber = request.EmployeeNumber;
        entity.Name = request.Name;
        entity.OrganizationalUnitId = request.OrganizationalUnitId;
        entity.JobTitle = request.JobTitle;
        entity.JobGrade = request.JobGrade;
        entity.HireDate = request.HireDate;
        entity.IsActive = request.IsActive;

        // Parse EmploymentStatus
        if (Enum.TryParse<ERP_Government.Domain.Organization.Enums.EmploymentStatus>(request.EmploymentStatus, out var status))
            entity.EmploymentStatus = status;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid employee ID.");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required.")
            .MaximumLength(50).WithMessage("Employee number must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.OrganizationalUnitId)
            .GreaterThan(0).WithMessage("Organizational unit is required.");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required.");

        RuleFor(x => x.EmploymentStatus)
            .NotEmpty().WithMessage("Employment status is required.")
            .Must(s => Enum.TryParse<ERP_Government.Domain.Organization.Enums.EmploymentStatus>(s, out _))
            .WithMessage("Invalid employment status.");
    }
}
