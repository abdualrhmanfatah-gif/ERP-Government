using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Items.Commands.UpdateItem;

[Authorize(Policy = PermissionCodes.ItemsUpdate)]
public record UpdateItemCommand(
    int Id,
    string Name,
    string? NameEn,
    string? Description,
    int? CategoryId,
    int UnitId,
    int? SupplierId,
    string? Barcode,
    string ItemType,
    decimal? OpeningStock,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    bool IsActive) : IRequest<Result>;

public class UpdateItemCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateItemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Items.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Item not found."]);

        entity.Name = request.Name;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.CategoryId = request.CategoryId;
        entity.UnitId = request.UnitId;
        entity.SupplierId = request.SupplierId;
        entity.Barcode = request.Barcode;
        entity.ItemType = request.ItemType;
        entity.OpeningStock = request.OpeningStock;
        entity.MinimumStock = request.MinimumStock;
        entity.MaximumStock = request.MaximumStock;
        entity.ReorderLevel = request.ReorderLevel;
        entity.ReorderQuantity = request.ReorderQuantity;
        entity.LeadTimeDays = request.LeadTimeDays;
        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid item.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.NameEn)
            .MaximumLength(500).WithMessage("English name must not exceed 500 characters.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Unit is required.");

        RuleFor(x => x.ItemType)
            .NotEmpty().WithMessage("Item type is required.")
            .Must(t => Enum.TryParse<ERP_Government.Application.Inventory.Items.Enums.ItemType>(t, out _))
            .WithMessage("Invalid item type.");

        RuleFor(x => x.OpeningStock)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Opening stock must be non-negative.");

        RuleFor(x => x.MinimumStock)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Minimum stock must be non-negative.");

        RuleFor(x => x.MaximumStock)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Maximum stock must be non-negative.");

        RuleFor(x => x)
            .Must(x => !x.MinimumStock.HasValue || !x.MaximumStock.HasValue || x.MinimumStock <= x.MaximumStock)
            .WithMessage("Minimum stock must not exceed maximum stock.");

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage("Barcode must not exceed 100 characters.")
            .MustAsync(async (x, barcode, ct) =>
                string.IsNullOrEmpty(barcode) ||
                !await context.Items.AnyAsync(i => i.Barcode == barcode && i.Id != x.Id, ct))
            .WithMessage("An item with this barcode already exists.");
    }
}
