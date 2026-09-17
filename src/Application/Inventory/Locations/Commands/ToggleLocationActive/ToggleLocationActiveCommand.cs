using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using FluentValidation;

namespace ERP_Government.Application.Inventory.Locations.Commands.ToggleLocationActive;

[Authorize(Policy = PermissionCodes.LocationsUpdate)]
public record ToggleLocationActiveCommand(
    int Id,
    bool IsActive,
    byte[] RowVersion) : IRequest<Result<int>>;

public class ToggleLocationActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleLocationActiveCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ToggleLocationActiveCommand request, CancellationToken ct)
    {
        var entity = await context.Locations.FindAsync([request.Id], ct);
        if (entity is null)
            return Result<int>.Failure(
                ErrorCodes.Inventory.LocationNotFound,
                ErrorCategory.NotFound,
                "الموقع غير موجود");

        if (entity.IsActive == request.IsActive)
        {
            var message = request.IsActive ? "الموقع مفعل بالفعل" : "الموقع معطل بالفعل";
            return Result<int>.Failure(
                ErrorCodes.Inventory.LocationStateUnchanged,
                ErrorCategory.Validation,
                message);
        }

        if (!request.IsActive)
        {
            if (await context.Assets.AnyAsync(a => a.LocationId == request.Id, ct))
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationDeactivationBlocked,
                    ErrorCategory.Validation,
                    "لا يمكن تعطيل موقع مرتبط بأصول");

            if (await context.Locations.AnyAsync(l => l.ParentLocationId == request.Id && l.IsActive, ct))
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationDeactivationBlocked,
                    ErrorCategory.Validation,
                    "لا يمكن تعطيل موقع له مواقع فرعية نشطة");
        }
        else if (entity.ParentLocationId.HasValue)
        {
            var parent = await context.Locations.FindAsync([entity.ParentLocationId.Value], ct);
            if (parent is not null && !parent.IsActive)
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationParentInactive,
                    ErrorCategory.Validation,
                    "لا يمكن تنشيط موقع تحت موقع أب معطل");
        }

        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(
                ErrorCodes.Request.ConcurrencyConflict,
                ErrorCategory.Conflict,
                "تم تعديل الموقع من مستخدم آخر");
        }

        return Result<int>.Success(entity.Id);
    }
}

public class ToggleLocationActiveCommandValidator : AbstractValidator<ToggleLocationActiveCommand>
{
    public ToggleLocationActiveCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion مطلوب");
    }
}
