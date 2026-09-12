using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Warehouses.Commands.CreateWarehouse;

[Authorize(Policy = PermissionCodes.WarehousesCreate)]
public record CreateWarehouseCommand(
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
    bool IsActive = true) : IRequest<Result<int>>;

public class CreateWarehouseCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateWarehouseCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            LocationId = request.LocationId,
            ManagerId = request.ManagerId,
            Address = request.Address,
            City = request.City,
            Phone = request.Phone,
            Email = request.Email,
            TotalCapacity = request.TotalCapacity,
            CurrentLoad = request.CurrentLoad,
            IsActive = request.IsActive,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Warehouses.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (code, ct) =>
                !await context.Warehouses.AnyAsync(w => w.Code == code, ct))
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
