using ERP_Government.Application.Common.Security;
using Unit = ERP_Government.Domain.Inventory.Entities.Unit;

namespace ERP_Government.Application.Inventory.Units.Commands.CreateUnit;

[Authorize(Policy = PermissionCodes.UnitsCreate)]
public record CreateUnitCommand(
    string Code,
    string Name,
    string? NameAr,
    string? UnitType,
    int? BaseUnitId,
    decimal? ConversionToBase,
    bool IsActive = true) : IRequest<Result<int>>;

public class CreateUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateUnitCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new Unit
        {
            Code = request.Code,
            Name = request.Name,
            NameAr = request.NameAr,
            UnitType = request.UnitType,
            BaseUnitId = request.BaseUnitId,
            ConversionToBase = request.ConversionToBase,
            IsActive = request.IsActive,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Units.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (code, ct) =>
                !await context.Units.AnyAsync(u => u.Code == code, ct))
            .WithMessage("A unit with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x)
            .Must(x => !x.BaseUnitId.HasValue || (x.ConversionToBase.HasValue && x.ConversionToBase > 0))
            .WithMessage("Conversion to base must be greater than 0 when a base unit is selected.");
    }
}
