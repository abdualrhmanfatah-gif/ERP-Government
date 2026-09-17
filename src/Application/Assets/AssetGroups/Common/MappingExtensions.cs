using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;

namespace ERP_Government.Application.Assets.AssetGroups.Common;

public record AssetGroupResponse(
    int Id,
    string Code,
    string Name,
    string? Description,
    int? ParentAssetGroupId,
    string? ParentName,
    bool IsActive,
    string AssetCategory,
    bool IsDepreciable,
    int? DefaultUsefulLifeYears,
    bool HasChildren,
    byte[] RowVersion);

public record AssetGroupDetailResponse(
    int Id,
    string Code,
    string Name,
    string? Description,
    int? ParentAssetGroupId,
    string? ParentName,
    bool IsActive,
    string AssetCategory,
    bool IsDepreciable,
    string DepreciationMethod,
    decimal? DepreciationRate,
    int? DefaultUsefulLifeYears,
    decimal? ResidualValuePercentage,
    int? AssetAccountId,
    int? AccumulatedDepreciationAccountId,
    int? DepreciationExpenseAccountId,
    int? DisposalAccountId,
    byte[] RowVersion,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy,
    IReadOnlyList<AssetGroupAttributeBindingResponse> AttributeBindings);

public record AssetGroupAttributeBindingResponse(
    int AssetAttributeDefinitionId,
    string Code,
    string Name,
    AssetAttributeDataType DataType,
    bool IsRequired,
    int? SortOrder);

public static class AssetGroupMappingExtensions
{
    public static AssetGroupResponse ToResponse(this AssetGroup group, bool hasChildren) => new(
        group.Id,
        group.Code,
        group.Name,
        group.Description,
        group.ParentAssetGroupId,
        group.ParentAssetGroup?.Name,
        group.IsActive,
        group.AssetCategory,
        group.IsDepreciable,
        group.DefaultUsefulLifeYears,
        hasChildren,
        group.RowVersion);

    public static AssetGroupDetailResponse ToDetailResponse(
        this AssetGroup group,
        IReadOnlyList<AssetGroupAttributeBindingResponse>? attributeBindings = null) => new(
        group.Id,
        group.Code,
        group.Name,
        group.Description,
        group.ParentAssetGroupId,
        group.ParentAssetGroup?.Name,
        group.IsActive,
        group.AssetCategory,
        group.IsDepreciable,
        group.DepreciationMethod,
        group.DepreciationRate,
        group.DefaultUsefulLifeYears,
        group.ResidualValuePercentage,
        group.AssetAccountId,
        group.AccumulatedDepreciationAccountId,
        group.DepreciationExpenseAccountId,
        group.DisposalAccountId,
        group.RowVersion,
        group.Created,
        group.CreatedBy,
        group.LastModified,
        group.LastModifiedBy,
        attributeBindings ?? []);
}
