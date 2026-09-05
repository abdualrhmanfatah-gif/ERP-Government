using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// AssetGroup seed data — 3 groups.
/// </summary>
public static class AssetGroupSeedData
{
    public static List<AssetGroup> GetAssetGroups() =>
    [
        new() { Code="AG-001", Name="مباني وإنشاءات", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=50, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-002", Name="أثاث ومعدات", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=10, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-003", Name="مركبات", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
    ];
}
