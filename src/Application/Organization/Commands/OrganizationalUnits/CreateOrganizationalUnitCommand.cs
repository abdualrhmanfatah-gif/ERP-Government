using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.OrganizationalUnits;

// C-O001 — CreateOrganizationalUnitCommand
[Authorize(Policy = PermissionCodes.OrgUnitsCreate)]
public class CreateOrganizationalUnitCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? ParentId { get; init; }
}

public class CreateOrganizationalUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateOrganizationalUnitCommand, Result>
{
    public async Task<Result> Handle(
        CreateOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.OrganizationalUnits
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Organizational unit code already exists."]);

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

        var entity = new OrganizationalUnit
        {
            Code = request.Code,
            Name = request.Name,
            ParentId = request.ParentId,
            ParentPath = parentPath,
            IsActive = true
        };

        context.OrganizationalUnits.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateOrganizationalUnitCommandValidator : AbstractValidator<CreateOrganizationalUnitCommand>
{
    public CreateOrganizationalUnitCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
