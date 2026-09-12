using ERP_Government.Application.Common.Security;
using Unit = ERP_Government.Domain.Inventory.Entities.Unit;

namespace ERP_Government.Application.Inventory.Units.Commands.UpdateUnit;

[Authorize(Policy = PermissionCodes.UnitsUpdate)]
public record UpdateUnitCommand(
    int Id,
    string Code,
    string Name,
    string? NameAr,
    string? UnitType,
    int? BaseUnitId,
    decimal? ConversionToBase,
    bool IsActive) : IRequest<Result>;

public class UpdateUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateUnitCommand, Result>
{
    public async Task<Result> Handle(
        UpdateUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Units.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Unit not found."]);

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.NameAr = request.NameAr;
        entity.UnitType = request.UnitType;
        entity.BaseUnitId = request.BaseUnitId;
        entity.ConversionToBase = request.ConversionToBase;
        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid unit.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (x, code, ct) =>
                !await context.Units.AnyAsync(u => u.Code == code && u.Id != x.Id, ct))
            .WithMessage("A unit with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x)
            .Must(x => !x.BaseUnitId.HasValue || (x.ConversionToBase.HasValue && x.ConversionToBase > 0))
            .WithMessage("Conversion to base must be greater than 0 when a base unit is selected.");
    }
}
