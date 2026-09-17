using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Assets.AssetAttributes.Values;

public record AttributeValueInput(
    int AssetAttributeDefinitionId,
    string? TextValue,
    int? IntegerValue,
    decimal? DecimalValue,
    DateOnly? DateValue,
    bool? BooleanValue);

public static class AttributeValueValidator
{
    public static async Task<List<string>> ValidateAsync(
        IApplicationDbContext context,
        int assetGroupId,
        List<AttributeValueInput> values,
        CancellationToken ct)
    {
        var errors = new List<string>();

        var bindings = await context.AssetGroupAttributes
            .Where(b => b.AssetGroupId == assetGroupId)
            .ToListAsync(ct);

        var definitions = await context.AssetAttributeDefinitions
            .Where(d => bindings.Select(b => b.AssetAttributeDefinitionId).Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        var requiredDefIds = bindings
            .Where(b => b.IsRequired)
            .Select(b => b.AssetAttributeDefinitionId)
            .ToHashSet();

        var providedDefIds = values.Select(v => v.AssetAttributeDefinitionId).ToHashSet();

        foreach (var reqDefId in requiredDefIds)
        {
            if (!providedDefIds.Contains(reqDefId))
            {
                var def = definitions[reqDefId];
                errors.Add($"المواصفة '{def.Name}' مطلوبة");
            }
        }

        foreach (var value in values)
        {
            if (!definitions.TryGetValue(value.AssetAttributeDefinitionId, out var def))
            {
                errors.Add($"المواصفة بالمعرف {value.AssetAttributeDefinitionId} غير موجودة");
                continue;
            }

            var hasValue = value.TextValue is not null
                || value.IntegerValue is not null
                || value.DecimalValue is not null
                || value.DateValue is not null
                || value.BooleanValue is not null;

            if (requiredDefIds.Contains(value.AssetAttributeDefinitionId) && !hasValue)
                errors.Add($"المواصفة '{def.Name}' مطلوبة");

            if (hasValue)
            {
                var typeError = def.AttributeDataType switch
                {
                    AssetAttributeDataType.Text when value.TextValue is null => $"المواصفة '{def.Name}' تتطلب قيمة نصية",
                    AssetAttributeDataType.Integer when value.IntegerValue is null => $"المواصفة '{def.Name}' تتطلب قيمة عددية صحيحة",
                    AssetAttributeDataType.Decimal when value.DecimalValue is null => $"المواصفة '{def.Name}' تتطلب قيمة عددية عشرية",
                    AssetAttributeDataType.Date when value.DateValue is null => $"المواصفة '{def.Name}' تتطلب قيمة تاريخية",
                    AssetAttributeDataType.Boolean when value.BooleanValue is null => $"المواصفة '{def.Name}' تتطلب قيمة منطقية",
                    AssetAttributeDataType.Text when value.IntegerValue is not null || value.DecimalValue is not null || value.DateValue is not null || value.BooleanValue is not null => $"المواصفة '{def.Name}' تتطلب قيمة نصية فقط",
                    AssetAttributeDataType.Integer when value.TextValue is not null || value.DecimalValue is not null || value.DateValue is not null || value.BooleanValue is not null => $"المواصفة '{def.Name}' تتطلب قيمة عددية صحيحة فقط",
                    AssetAttributeDataType.Decimal when value.TextValue is not null || value.IntegerValue is not null || value.DateValue is not null || value.BooleanValue is not null => $"المواصفة '{def.Name}' تتطلب قيمة عددية عشرية فقط",
                    AssetAttributeDataType.Date when value.TextValue is not null || value.IntegerValue is not null || value.DecimalValue is not null || value.BooleanValue is not null => $"المواصفة '{def.Name}' تتطلب قيمة تاريخية فقط",
                    AssetAttributeDataType.Boolean when value.TextValue is not null || value.IntegerValue is not null || value.DecimalValue is not null || value.DateValue is not null => $"المواصفة '{def.Name}' تتطلب قيمة منطقية فقط",
                    _ => null
                };

                if (typeError is not null)
                    errors.Add(typeError);
            }
        }

        return errors;
    }

    public static bool HasValue(AttributeValueInput input) =>
        input.TextValue is not null
        || input.IntegerValue is not null
        || input.DecimalValue is not null
        || input.DateValue is not null
        || input.BooleanValue is not null;

    public static void Apply(AttributeValueInput input, AssetAttributeDataType dataType, AssetAttributeValue target)
    {
        target.TextValue = dataType == AssetAttributeDataType.Text ? input.TextValue : null;
        target.IntegerValue = dataType == AssetAttributeDataType.Integer ? input.IntegerValue : null;
        target.DecimalValue = dataType == AssetAttributeDataType.Decimal ? input.DecimalValue : null;
        target.DateValue = dataType == AssetAttributeDataType.Date ? input.DateValue : null;
        target.BooleanValue = dataType == AssetAttributeDataType.Boolean ? input.BooleanValue : null;
    }
}
