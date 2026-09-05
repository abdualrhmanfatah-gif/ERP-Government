using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.Roles;

// C-S001 — CreateRoleCommand
[Authorize(Policy = PermissionCodes.RolesCreate)]
public class CreateRoleCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public RoleLevel RoleLevel { get; init; }
    public bool IsMutuallyExclusive { get; init; }
    public int? ExclusiveWithRoleId { get; init; }
    public bool RequiresMfa { get; init; }
    public int? MaxSessionDuration { get; init; }
}

public class CreateRoleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateRoleCommand, Result>
{
    public async Task<Result> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.SecurityRoles
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Role code already exists."]);

        var entity = new SecurityRole
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            RoleLevel = request.RoleLevel,
            IsMutuallyExclusive = request.IsMutuallyExclusive,
            ExclusiveWithRoleId = request.ExclusiveWithRoleId,
            RequiresMfa = request.RequiresMfa,
            MaxSessionDuration = request.MaxSessionDuration,
            IsSystem = false,
            IsActive = true
        };

        context.SecurityRoles.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.RoleLevel)
            .IsInEnum().WithMessage("Invalid role level.");

        RuleFor(x => x.MaxSessionDuration)
            .GreaterThan(0).When(x => x.MaxSessionDuration.HasValue)
            .WithMessage("Max session duration must be greater than 0.");
    }
}
