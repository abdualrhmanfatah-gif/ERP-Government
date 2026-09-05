using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Security.Commands.Roles;

// C-S002 — UpdateRoleCommand
[Authorize(Policy = PermissionCodes.RolesUpdate)]
public class UpdateRoleCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public RoleLevel RoleLevel { get; init; }
    public bool IsMutuallyExclusive { get; init; }
    public int? ExclusiveWithRoleId { get; init; }
    public bool RequiresMfa { get; init; }
    public int? MaxSessionDuration { get; init; }
}

public class UpdateRoleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SecurityRoles
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Role not found."]);

        if (entity.IsSystem)
            return Result.Failure(["Cannot modify system roles."]);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.RoleLevel = request.RoleLevel;
        entity.IsMutuallyExclusive = request.IsMutuallyExclusive;
        entity.ExclusiveWithRoleId = request.ExclusiveWithRoleId;
        entity.RequiresMfa = request.RequiresMfa;
        entity.MaxSessionDuration = request.MaxSessionDuration;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.RoleLevel)
            .IsInEnum().WithMessage("Invalid role level.");
    }
}
