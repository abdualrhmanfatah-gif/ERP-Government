using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Organization.Enums;

namespace ERP_Government.Application.Organization.Commands.Employees;

// C-O002 — CreateEmployeeCommand
[Authorize(Policy = PermissionCodes.EmployeesCreate)]
public class CreateEmployeeCommand : IRequest<Result>
{
    public string EmployeeNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public int OrganizationalUnitId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string? JobGrade { get; init; }
    public DateOnly HireDate { get; init; }
}

public class CreateEmployeeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Employees
            .AnyAsync(x => x.EmployeeNumber == request.EmployeeNumber, cancellationToken);

        if (exists)
            return Result.Failure(["Employee number already exists."]);

        var orgUnit = await context.OrganizationalUnits
            .FindAsync(request.OrganizationalUnitId, cancellationToken);

        if (orgUnit is null)
            return Result.Failure(["Organizational unit not found."]);

        var entity = new Employee
        {
            EmployeeNumber = request.EmployeeNumber,
            Name = request.Name,
            UserId = request.UserId,
            OrganizationalUnitId = request.OrganizationalUnitId,
            JobTitle = request.JobTitle,
            JobGrade = request.JobGrade,
            HireDate = request.HireDate,
            EmploymentStatus = EmploymentStatus.Active,
            IsActive = true
        };

        context.Employees.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
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
    }
}
