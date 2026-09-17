using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Domain.Assets.Services;

public static class DepreciationEligibilityFilter
{
    public static bool IsEligible(Asset asset)
    {
        if (!string.Equals(asset.Status, "Active", StringComparison.OrdinalIgnoreCase)) return false;
        if (!asset.IsActive) return false;
        if (asset.AssetGroup is null || !asset.AssetGroup.IsActive || !asset.AssetGroup.IsDepreciable) return false;
        var usefulLife = asset.UsefulLifeYears ?? asset.AssetGroup.DefaultUsefulLifeYears;
        if (usefulLife is null or <= 0) return false;
        if (asset.OriginalValue <= 0) return false;
        if (asset.IsFullyDepreciated) return false;
        var residualValue = CalculateResidualValue(asset);
        if (asset.AccumulatedDepreciation >= (asset.OriginalValue - residualValue)) return false;
        return true;
    }

    private static decimal CalculateResidualValue(Asset asset)
    {
        var residualPercentage = asset.AssetGroup?.ResidualValuePercentage ?? 0;
        return Math.Round(asset.OriginalValue * residualPercentage / 100, 2);
    }
}
