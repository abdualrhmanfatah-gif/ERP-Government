using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;

namespace ERP_Government.Application.Assets.Assets.Common;

public static class AssetMappingExtensions
{
    public static AssetResponse ToResponse(this Asset asset) => new(
        asset.Id,
        asset.Code,
        asset.Name,
        asset.AssetGroupId,
        asset.AssetGroup?.Name,
        asset.Status,
        asset.Location?.Name,
        asset.Employee?.Name,
        asset.Currency?.Code,
        asset.ExchangeRate?.Rate,
        asset.OriginalValue,
        asset.CurrentValue,
        asset.PurchaseDate,
        asset.AssetTag,
        asset.IsActive,
        asset.RowVersion);

    public static AssetDetailResponse ToDetailResponse(
        this Asset asset,
        IReadOnlyList<AssetAttributeValue>? attributeValues = null) => new(
        asset.Id,
        asset.Code,
        asset.Name,
        asset.Description,
        asset.AssetGroupId,
        asset.AssetGroup?.Name,
        asset.LocationId,
        asset.Location?.Name,
        asset.EmployeeId,
        asset.Employee?.Name,
        asset.AssetTag,
        asset.Barcode,
        asset.SerialNumber,
        asset.CurrencyId,
        asset.Currency?.Code,
        asset.ExchangeRateId,
        asset.ExchangeRate?.Rate,
        asset.OriginalValue,
        asset.AcquisitionCost,
        asset.AccumulatedDepreciation,
        asset.CurrentValue,
        asset.PurchaseDate,
        asset.ActivationDate,
        asset.DepreciationStartDate,
        asset.LastDepreciationDate,
        asset.Status,
        asset.AcquisitionType,
        asset.UsefulLifeYears,
        asset.IsFullyDepreciated,
        asset.Notes,
        asset.IsActive,
        asset.RowVersion,
        asset.Created,
        asset.CreatedBy,
        asset.LastModified,
        asset.LastModifiedBy,
        asset.Employee?.OrganizationalUnitId,
        asset.Employee?.OrganizationalUnit?.Name,
        attributeValues?.Select(v => new AssetAttributeValueResponse(
            v.AssetAttributeDefinitionId,
            v.AssetAttributeDefinition?.Code ?? string.Empty,
            v.AssetAttributeDefinition?.Name ?? string.Empty,
            v.AssetAttributeDefinition?.AttributeDataType ?? default,
            v.AssetAttributeDefinition?.IsActive ?? false,
            v.TextValue,
            v.IntegerValue,
            v.DecimalValue,
            v.DateValue,
            v.BooleanValue)).ToList() ?? []);
}
