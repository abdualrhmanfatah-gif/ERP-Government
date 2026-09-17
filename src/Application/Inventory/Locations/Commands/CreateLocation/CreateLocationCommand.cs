using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Locations.Commands.CreateLocation;

[Authorize(Policy = PermissionCodes.LocationsCreate)]
public record CreateLocationCommand(
    string Code,
    string Name,
    string? Barcode,
    int? ParentLocationId,
    string? City,
    string? Address,
    decimal? Capacity,
    bool IsActive = true) : IRequest<Result<int>>;

public class CreateLocationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateLocationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateLocationCommand request,
        CancellationToken cancellationToken)
    {
        int? level = null;
        string? breadcrumb = null;

        if (request.ParentLocationId.HasValue)
        {
            var parent = await context.Locations.FindAsync([request.ParentLocationId.Value], cancellationToken);
            if (parent is null)
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationParentNotFound,
                    ErrorCategory.NotFound,
                    "الموقع الأب غير موجود");

            level = (parent.Level ?? 0) + 1;
            breadcrumb = string.IsNullOrEmpty(parent.Breadcrumb)
                ? parent.Code
                : $"{parent.Breadcrumb}/{parent.Code}";
        }

        var entity = new Location
        {
            Code = request.Code,
            Name = request.Name,
            Barcode = request.Barcode,
            ParentLocationId = request.ParentLocationId,
            Level = level,
            Breadcrumb = breadcrumb,
            City = request.City,
            Address = request.Address,
            Capacity = request.Capacity,
            IsActive = request.IsActive,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Locations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (code, ct) =>
                !await context.Locations.AnyAsync(l => l.Code == code, ct))
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

        RuleFor(x => x.ParentLocationId)
            .MustAsync(async (parentId, ct) =>
                !parentId.HasValue ||
                await context.Locations.AnyAsync(l => l.Id == parentId.Value, ct))
            .WithMessage("Parent location not found.");
    }
}
