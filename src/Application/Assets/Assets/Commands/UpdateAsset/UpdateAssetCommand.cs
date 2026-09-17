using ERP_Government.Application.Assets.AssetAttributes.Values;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Constants;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Assets.Commands.UpdateAsset;

[Authorize(Policy = PermissionCodes.AssetsUpdate)]
public record UpdateAssetCommand(
    int Id,
    string Name,
    string? Description,
    int AssetGroupId,
    int? LocationId,
    int? EmployeeId,
    string? AssetTag,
    string? Barcode,
    string? SerialNumber,
    int CurrencyId,
    int? ExchangeRateId,
    decimal OriginalValue,
    decimal? AcquisitionCost,
    DateOnly PurchaseDate,
    DateOnly DepreciationStartDate,
    string AcquisitionType,
    int? UsefulLifeYears,
    string? Notes,
    string? Status,
    byte[] RowVersion,
    List<AttributeValueInput>? AttributeValues = null) : IRequest<Result>;

public class UpdateAssetCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssetCommand, Result>
{
    private static readonly HashSet<string> LockedStatuses = ["Active", "UnderMaintenance", "Disposed", "WrittenOff"];

    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        ["Draft"] = ["Active"],
        ["Active"] = ["UnderMaintenance", "Disposed"],
        ["UnderMaintenance"] = ["Active", "Disposed"],
    };

    public async Task<Result> Handle(UpdateAssetCommand request, CancellationToken ct)
    {
        var entity = await context.Assets.FindAsync([request.Id], ct);
        if (entity is null)
            return Result.Failure(
                ErrorCodes.Assets.AssetNotFound,
                ErrorCategory.NotFound,
                $"الأصل بالمعرف {request.Id} غير موجود");

        entity.RowVersion = request.RowVersion;

        if (!await context.AssetGroups.AnyAsync(g => g.Id == request.AssetGroupId, ct))
            return Result.Failure(
                ErrorCodes.Assets.AssetGroupNotFound,
                ErrorCategory.NotFound,
                "المجموعة غير موجودة");

        if (request.ExchangeRateId.HasValue)
        {
            var exchangeRate = await context.ExchangeRates
                .Where(e => e.Id == request.ExchangeRateId.Value && e.IsActive)
                .Select(e => new { e.CurrencyId })
                .FirstOrDefaultAsync(ct);

            if (exchangeRate is null)
                return Result.Failure(
                    ErrorCodes.FinancialSettings.ExchangeRateNotFound,
                    ErrorCategory.NotFound,
                    "ط³ط¹ط± ط§ظ„طµط±ظپ ط؛ظٹط± ظ…ظˆط¬ظˆط¯");

            if (exchangeRate.CurrencyId != request.CurrencyId)
                return Result.Failure(
                    ErrorCodes.FinancialSettings.ExchangeRateNotFound,
                    ErrorCategory.Validation,
                    "ط³ط¹ط± ط§ظ„طµط±ظپ ظ„ط§ ظٹط·ط§ط¨ظ‚ ط¹ظ…ظ„ط© ط§ظ„ط£طµظ„");
        }

        if (!string.IsNullOrWhiteSpace(request.AssetTag))
        {
            if (await context.Assets.AnyAsync(a => a.AssetTag == request.AssetTag && a.Id != request.Id, ct))
                return Result.Failure(
                    ErrorCodes.Assets.DuplicateAssetTag,
                    ErrorCategory.Validation,
                    "الوسم مستخدم بالفعل");
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            if (await context.Assets.AnyAsync(a => a.Barcode == request.Barcode && a.Id != request.Id, ct))
                return Result.Failure(
                    ErrorCodes.Assets.DuplicateBarcode,
                    ErrorCategory.Validation,
                    "الباركود مستخدم بالفعل");
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            if (await context.Assets.AnyAsync(a => a.SerialNumber == request.SerialNumber && a.Id != request.Id, ct))
                return Result.Failure(
                    ErrorCodes.Assets.DuplicateSerialNumber,
                    ErrorCategory.Validation,
                    "الرقم التسلسلي مستخدم بالفعل");
        }

        if (LockedStatuses.Contains(entity.Status))
        {
            if (entity.AssetGroupId != request.AssetGroupId)
                return Result.Failure(
                    ErrorCodes.Assets.FieldLockedAfterActivation,
                    ErrorCategory.Validation,
                    "لا يمكن تغيير المجموعة بعد تفعيل الأصل");

            if (entity.OriginalValue != request.OriginalValue)
                return Result.Failure(
                    ErrorCodes.Assets.FieldLockedAfterActivation,
                    ErrorCategory.Validation,
                    "لا يمكن تغيير القيمة الأصلية بعد تفعيل الأصل");

            if (entity.PurchaseDate != request.PurchaseDate)
                return Result.Failure(
                    ErrorCodes.Assets.FieldLockedAfterActivation,
                    ErrorCategory.Validation,
                    "لا يمكن تغيير تاريخ الشراء بعد تفعيل الأصل");
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != entity.Status)
        {
            if (!AllowedTransitions.TryGetValue(entity.Status, out var allowed) || !allowed.Contains(request.Status))
                return Result.Failure(
                    ErrorCodes.Assets.InvalidStatusTransition,
                    ErrorCategory.Validation,
                    $"لا يمكن تغيير الحالة من {entity.Status} إلى {request.Status}");
            entity.Status = request.Status;
        }

        if (request.LocationId != entity.LocationId && request.LocationId.HasValue)
        {
            if (!await context.Locations.AnyAsync(l => l.Id == request.LocationId.Value, ct))
                return Result.Failure(
                    ErrorCodes.Inventory.LocationNotFound,
                    ErrorCategory.NotFound,
                    "الموقع غير موجود");
            if (!await context.Locations.AnyAsync(l => l.Id == request.LocationId.Value && l.IsActive, ct))
                return Result.Failure(
                    ErrorCodes.Inventory.LocationInactive,
                    ErrorCategory.Validation,
                    "الموقع غير نشط");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.AssetGroupId = request.AssetGroupId;
        entity.LocationId = request.LocationId;
        entity.EmployeeId = request.EmployeeId;
        entity.CurrencyId = request.CurrencyId;
        entity.ExchangeRateId = request.ExchangeRateId;
        entity.AssetTag = request.AssetTag;
        entity.Barcode = request.Barcode;
        entity.SerialNumber = request.SerialNumber;
        entity.OriginalValue = request.OriginalValue;
        entity.AcquisitionCost = request.AcquisitionCost;
        entity.PurchaseDate = request.PurchaseDate;
        entity.DepreciationStartDate = request.DepreciationStartDate;
        entity.AcquisitionType = request.AcquisitionType;
        entity.UsefulLifeYears = request.UsefulLifeYears;
        entity.Notes = request.Notes;

        if (request.AttributeValues is not null)
        {
            var attributeErrors = await AttributeValueValidator.ValidateAsync(context, request.AssetGroupId, request.AttributeValues, ct);
            if (attributeErrors.Count > 0)
                return Result.Failure(
                    ErrorCodes.Assets.InvalidAttributeValue,
                    ErrorCategory.Validation,
                    string.Join(" | ", attributeErrors));

            var existingValues = await context.AssetAttributeValues
                .Where(v => v.AssetId == request.Id)
                .ToDictionaryAsync(v => v.AssetAttributeDefinitionId, ct);

            var definitionIds = request.AttributeValues.Select(v => v.AssetAttributeDefinitionId).Distinct().ToList();
            var definitions = await context.AssetAttributeDefinitions
                .Where(d => definitionIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, ct);

            foreach (var input in request.AttributeValues)
            {
                if (!definitions.TryGetValue(input.AssetAttributeDefinitionId, out var definition)) continue;

                if (!AttributeValueValidator.HasValue(input))
                {
                    if (existingValues.TryGetValue(input.AssetAttributeDefinitionId, out var cleared))
                        context.AssetAttributeValues.Remove(cleared);
                    continue;
                }

                if (!existingValues.TryGetValue(input.AssetAttributeDefinitionId, out var value))
                {
                    value = new AssetAttributeValue
                    {
                        AssetId = request.Id,
                        AssetAttributeDefinitionId = input.AssetAttributeDefinitionId
                    };
                    context.AssetAttributeValues.Add(value);
                }

                AttributeValueValidator.Apply(input, definition.AttributeDataType, value);
            }
        }

        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف الأصل غير صالح");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الأصل مطلوب")
            .MaximumLength(200).WithMessage("اسم الأصل يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.AssetGroupId)
            .GreaterThan(0).WithMessage("المجموعة مطلوبة");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("العملة مطلوبة");

        RuleFor(x => x.OriginalValue)
            .GreaterThanOrEqualTo(0).WithMessage("القيمة الأصلية يجب أن تكون صفر أو أكثر");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("تاريخ الشراء مطلوب");

        RuleFor(x => x.DepreciationStartDate)
            .NotEmpty().WithMessage("تاريخ بدء الإهلاك مطلوب");

        RuleFor(x => x.AcquisitionType)
            .NotEmpty().WithMessage("نوع الاستحواذ مطلوب")
            .Must(AssetAcquisitionTypes.IsValid).WithMessage("نوع الاستحواذ غير صالح");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار السطر مطلوب للتحقق من التزامن");
    }
}
