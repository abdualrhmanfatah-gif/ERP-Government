using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Warehouses.Commands.UpdateWarehouse;

[Authorize(Policy = PermissionCodes.WarehousesUpdate)]
public record UpdateWarehouseCommand(
    int Id,
    string Code,
    string Name,
    int? LocationId,
    int? ManagerId,
    string? Address,
    string? City,
    string? Phone,
    string? Email,
    decimal? TotalCapacity,
    decimal? CurrentLoad,
    bool IsActive) : IRequest<Result>;

public class UpdateWarehouseCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateWarehouseCommand, Result>
{
    public async Task<Result> Handle(
        UpdateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Warehouses.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Warehouse not found."]);

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.LocationId = request.LocationId;
        entity.ManagerId = request.ManagerId;
        entity.Address = request.Address;
        entity.City = request.City;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.TotalCapacity = request.TotalCapacity;
        entity.CurrentLoad = request.CurrentLoad;
        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid warehouse.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (x, code, ct) =>
                !await context.Warehouses.AnyAsync(w => w.Code == code && w.Id != x.Id, ct))
            .WithMessage("A warehouse with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TotalCapacity)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Total capacity must be non-negative.");

        RuleFor(x => x.CurrentLoad)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Current load must be non-negative.");

        RuleFor(x => x)
            .Must(x => !x.CurrentLoad.HasValue || !x.TotalCapacity.HasValue || x.CurrentLoad <= x.TotalCapacity)
            .WithMessage("Current load must not exceed total capacity.");
    }
}
