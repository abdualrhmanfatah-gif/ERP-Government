using ERP_Government.Application.Assets.AssetAttributes.Values;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Constants;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Assets.Commands.CreateAsset;

[Authorize(Policy = PermissionCodes.AssetsCreate)]
public record CreateAssetCommand(
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
    List<AttributeValueInput>? AttributeValues = null) : IRequest<Result<int>>;

public class CreateAssetCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        string code;
        try
        {
            code = await sequenceService.GenerateNextNumberAsync("Asset", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(
                ErrorCodes.Request.InternalError,
                ErrorCategory.Internal,
                "فشل في توليد رقم الأصل");
        }

        if (!await context.AssetGroups.AnyAsync(g => g.Id == request.AssetGroupId, ct))
            return Result<int>.Failure(
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
                return Result<int>.Failure(
                    ErrorCodes.FinancialSettings.ExchangeRateNotFound,
                    ErrorCategory.NotFound,
                    "ط³ط¹ط± ط§ظ„طµط±ظپ ط؛ظٹط± ظ…ظˆط¬ظˆط¯");

            if (exchangeRate.CurrencyId != request.CurrencyId)
                return Result<int>.Failure(
                    ErrorCodes.FinancialSettings.ExchangeRateNotFound,
                    ErrorCategory.Validation,
                    "ط³ط¹ط± ط§ظ„طµط±ظپ ظ„ط§ ظٹط·ط§ط¨ظ‚ ط¹ظ…ظ„ط© ط§ظ„ط£طµظ„");
        }

        if (request.LocationId.HasValue)
        {
            if (!await context.Locations.AnyAsync(l => l.Id == request.LocationId.Value, ct))
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationNotFound,
                    ErrorCategory.NotFound,
                    "الموقع غير موجود");
            if (!await context.Locations.AnyAsync(l => l.Id == request.LocationId.Value && l.IsActive, ct))
                return Result<int>.Failure(
                    ErrorCodes.Inventory.LocationInactive,
                    ErrorCategory.Validation,
                    "الموقع غير نشط");
        }

        if (!string.IsNullOrWhiteSpace(request.AssetTag))
        {
            if (await context.Assets.AnyAsync(a => a.AssetTag == request.AssetTag, ct))
                return Result<int>.Failure(
                    ErrorCodes.Assets.DuplicateAssetTag,
                    ErrorCategory.Validation,
                    "الوسم مستخدم بالفعل");
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            if (await context.Assets.AnyAsync(a => a.Barcode == request.Barcode, ct))
                return Result<int>.Failure(
                    ErrorCodes.Assets.DuplicateBarcode,
                    ErrorCategory.Validation,
                    "الباركود مستخدم بالفعل");
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            if (await context.Assets.AnyAsync(a => a.SerialNumber == request.SerialNumber, ct))
                return Result<int>.Failure(
                    ErrorCodes.Assets.DuplicateSerialNumber,
                    ErrorCategory.Validation,
                    "الرقم التسلسلي مستخدم بالفعل");
        }

        var attributeInputs = request.AttributeValues ?? [];
        var attributeErrors = await AttributeValueValidator.ValidateAsync(context, request.AssetGroupId, attributeInputs, ct);
        if (attributeErrors.Count > 0)
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidAttributeValue,
                ErrorCategory.Validation,
                string.Join(" | ", attributeErrors));

        var entity = new Asset
        {
            Code = code,
            Name = request.Name,
            Description = request.Description,
            AssetGroupId = request.AssetGroupId,
            LocationId = request.LocationId,
            EmployeeId = request.EmployeeId,
            CurrencyId = request.CurrencyId,
            ExchangeRateId = request.ExchangeRateId,
            AssetTag = request.AssetTag,
            Barcode = request.Barcode,
            SerialNumber = request.SerialNumber,
            OriginalValue = request.OriginalValue,
            AcquisitionCost = request.AcquisitionCost,
            CurrentValue = request.OriginalValue,
            PurchaseDate = request.PurchaseDate,
            DepreciationStartDate = request.DepreciationStartDate,
            Status = "Draft",
            AcquisitionType = request.AcquisitionType,
            UsefulLifeYears = request.UsefulLifeYears,
            Notes = request.Notes,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Assets.Add(entity);

        var definitionIds = attributeInputs.Select(v => v.AssetAttributeDefinitionId).Distinct().ToList();
        var definitions = await context.AssetAttributeDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        foreach (var input in attributeInputs.Where(AttributeValueValidator.HasValue))
        {
            if (!definitions.TryGetValue(input.AssetAttributeDefinitionId, out var definition)) continue;

            var value = new AssetAttributeValue
            {
                Asset = entity,
                AssetAttributeDefinitionId = input.AssetAttributeDefinitionId
            };
            AttributeValueValidator.Apply(input, definition.AttributeDataType, value);
            context.AssetAttributeValues.Add(value);
        }

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
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
    }
}
