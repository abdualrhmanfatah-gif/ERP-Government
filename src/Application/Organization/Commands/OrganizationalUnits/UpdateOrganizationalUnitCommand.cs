using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.OrganizationalUnits;

// C-O002 — UpdateOrganizationalUnitCommand
[Authorize(Policy = PermissionCodes.OrgUnitsUpdate)]
public class UpdateOrganizationalUnitCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public bool IsActive { get; init; } = true;
}

public class UpdateOrganizationalUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateOrganizationalUnitCommand, Result>
{
    public async Task<Result> Handle(
        UpdateOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.OrganizationalUnits
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Organizational unit not found."]);

        // Duplicate Code check (excluding self)
        var codeExists = await context.OrganizationalUnits
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);

        if (codeExists)
            return Result.Failure(["Organizational unit code already exists."]);

        // Circular parent check
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                return Result.Failure(["Organizational unit cannot be its own parent."]);

            // Check if new parent is a descendant of this unit
            var isDescendant = await context.OrganizationalUnits
                .AnyAsync(x => x.ParentId == request.Id && x.Id == request.ParentId.Value, cancellationToken);

            if (isDescendant)
                return Result.Failure(["Cannot assign a descendant as parent."]);
        }

        // Recompute ParentPath if ParentId or Code changed
        if (entity.ParentId != request.ParentId || entity.Code != request.Code)
        {
            string? parentPath = null;
            if (request.ParentId.HasValue)
            {
                var parent = await context.OrganizationalUnits
                    .FindAsync(request.ParentId.Value, cancellationToken);

                if (parent is null)
                    return Result.Failure(["Parent organizational unit not found."]);

                parentPath = $"{parent.ParentPath}/{request.Code}";
            }
            else
            {
                parentPath = $"/{request.Code}";
            }

            entity.ParentPath = parentPath;
        }

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.ParentId = request.ParentId;
        entity.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateOrganizationalUnitCommandValidator : AbstractValidator<UpdateOrganizationalUnitCommand>
{
    public UpdateOrganizationalUnitCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid organizational unit ID.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
