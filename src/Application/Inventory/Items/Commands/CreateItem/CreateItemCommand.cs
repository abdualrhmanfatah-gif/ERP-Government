using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Items.Commands.CreateItem;

[Authorize(Policy = PermissionCodes.ItemsCreate)]
public record CreateItemCommand(
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
    bool IsActive = true) : IRequest<Result<int>>;

public class CreateItemCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateItemCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateItemCommand request,
        CancellationToken cancellationToken)
    {
        string code;
        try
        {
            code = await sequenceService.GenerateNextNumberAsync("Item", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure(ErrorCodes.Inventory.DuplicateItemCode, ErrorCategory.Internal, ex.Message);
        }

        var entity = new Item
        {
            Code = code,
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            SupplierId = request.SupplierId,
            Barcode = request.Barcode,
            ItemType = request.ItemType,
            OpeningStock = request.OpeningStock,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            ReorderLevel = request.ReorderLevel,
            ReorderQuantity = request.ReorderQuantity,
            LeadTimeDays = request.LeadTimeDays,
            IsActive = request.IsActive,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Items.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator(IApplicationDbContext context)
    {
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
            .MustAsync(async (barcode, ct) =>
                string.IsNullOrEmpty(barcode) ||
                !await context.Items.AnyAsync(i => i.Barcode == barcode, ct))
            .WithMessage("An item with this barcode already exists.");
    }
}
