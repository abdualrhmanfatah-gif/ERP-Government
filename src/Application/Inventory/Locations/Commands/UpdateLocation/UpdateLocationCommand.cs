using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Locations.Commands.UpdateLocation;

[Authorize(Policy = PermissionCodes.LocationsUpdate)]
public record UpdateLocationCommand(
    int Id,
    string Code,
    string Name,
    string? Barcode,
    int? ParentLocationId,
    string? City,
    string? Address,
    decimal? Capacity,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateLocationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateLocationCommand, Result>
{
    public async Task<Result> Handle(
        UpdateLocationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Locations.FindAsync([request.Id], cancellationToken);
        if (entity is null)
            return Result.Failure(
                ErrorCodes.Inventory.LocationNotFound,
                ErrorCategory.NotFound,
                "الموقع غير موجود");

        entity.RowVersion = request.RowVersion;
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Barcode = request.Barcode;
        entity.City = request.City;
        entity.Address = request.Address;
        entity.Capacity = request.Capacity;
        entity.LastModified = DateTimeOffset.UtcNow;

        var parentChanged = entity.ParentLocationId != request.ParentLocationId;
        if (request.ParentLocationId.HasValue)
        {
            var parent = await context.Locations.FindAsync([request.ParentLocationId.Value], cancellationToken);
            if (parent is null)
                return Result.Failure(
                    ErrorCodes.Inventory.LocationParentNotFound,
                    ErrorCategory.NotFound,
                    "الموقع الأب غير موجود");

            entity.ParentLocationId = request.ParentLocationId;
            entity.Level = (parent.Level ?? 0) + 1;
            entity.Breadcrumb = string.IsNullOrEmpty(parent.Breadcrumb)
                ? parent.Code
                : $"{parent.Breadcrumb}/{parent.Code}";
        }
        else
        {
            entity.ParentLocationId = null;
            entity.Level = null;
            entity.Breadcrumb = null;
        }

        if (parentChanged)
            await CascadeTreeFieldsAsync(entity, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(
                ErrorCodes.Request.ConcurrencyConflict,
                ErrorCategory.Conflict,
                "تم تعديل الموقع من مستخدم آخر");
        }

        return Result.Success();
    }

    private async Task CascadeTreeFieldsAsync(Location root, CancellationToken cancellationToken)
    {
        var all = await context.Locations.ToListAsync(cancellationToken);
        var childrenOf = all
            .Where(l => l.ParentLocationId.HasValue)
            .GroupBy(l => l.ParentLocationId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var queue = new Queue<Location>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!childrenOf.TryGetValue(node.Id, out var children))
                continue;

            foreach (var child in children)
            {
                child.Level = (node.Level ?? 0) + 1;
                child.Breadcrumb = string.IsNullOrEmpty(node.Breadcrumb)
                    ? node.Code
                    : $"{node.Breadcrumb}/{node.Code}";
                child.LastModified = DateTimeOffset.UtcNow;
                queue.Enqueue(child);
            }
        }
    }
}

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid location.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (x, code, ct) =>
                !await context.Locations.AnyAsync(l => l.Code == code && l.Id != x.Id, ct))
            .WithMessage("A location with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage("Barcode must not exceed 100 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.Capacity)
            .Must(v => !v.HasValue || v >= 0)
            .WithMessage("Capacity must be non-negative.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required.");

        RuleFor(x => x)
            .Must(x => !x.ParentLocationId.HasValue || x.ParentLocationId != x.Id)
            .WithMessage("A location cannot be its own parent.");

        RuleFor(x => x.ParentLocationId)
            .MustAsync(async (parentId, ct) =>
                !parentId.HasValue ||
                await context.Locations.AnyAsync(l => l.Id == parentId.Value, ct))
            .WithMessage("Parent location not found.");

        RuleFor(x => x)
            .MustAsync(async (x, ct) =>
                !await CreatesCycleAsync(context, x.Id, x.ParentLocationId, ct))
            .WithMessage("Parent assignment would create a cycle.");
    }

    private static async Task<bool> CreatesCycleAsync(
        IApplicationDbContext context,
        int id,
        int? parentId,
        CancellationToken cancellationToken)
    {
        var current = parentId;
        while (current.HasValue)
        {
            if (current.Value == id)
                return true;

            var ancestor = await context.Locations
                .FirstOrDefaultAsync(l => l.Id == current.Value, cancellationToken);
            if (ancestor is null)
                return false;

            current = ancestor.ParentLocationId;
        }

        return false;
    }
}
