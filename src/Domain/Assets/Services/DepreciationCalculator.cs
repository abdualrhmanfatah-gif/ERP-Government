using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Domain.Assets.Services;

public static class DepreciationCalculator
{
    public static DepreciationScheduleLine Calculate(
        Asset asset,
        decimal ratePerPeriod,
        int periodNumber,
        int? totalPeriods)
    {
        var residualValue = CalculateResidualValue(asset);
        var baseValue = asset.OriginalValue - residualValue;
        var remainingDepreciableAmount = Math.Max(0, baseValue - asset.AccumulatedDepreciation);
        var amount = Math.Min(Math.Round(baseValue * ratePerPeriod, 2), remainingDepreciableAmount);

        var accumulatedAfter = asset.AccumulatedDepreciation + amount;
        var netBookValue = asset.OriginalValue - accumulatedAfter;

        return new DepreciationScheduleLine
        {
            AssetId = asset.Id,
            Method = asset.AssetGroup?.DepreciationMethod ?? string.Empty,
            Rate = ratePerPeriod,
            PeriodNumber = periodNumber,
            TotalPeriods = totalPeriods,
            DepreciationBase = baseValue,
            ResidualValue = residualValue,
            OpeningBookValue = asset.OriginalValue - asset.AccumulatedDepreciation,
            OpeningAccumulatedDepreciation = asset.AccumulatedDepreciation,
            Amount = amount,
            ClosingAccumulatedDepreciation = accumulatedAfter,
            ClosingBookValue = netBookValue
        };
    }

    public static decimal CalculateRatePerPeriod(Asset asset, DateOnly from, DateOnly to)
    {
        var usefulLifeYears = asset.UsefulLifeYears ?? asset.AssetGroup?.DefaultUsefulLifeYears;
        if (usefulLifeYears is null or <= 0) return 0;
        var months = ((to.Year - from.Year) * 12) + to.Month - from.Month;
        if (months <= 0) months = 1;
        return Math.Round(1m / usefulLifeYears.Value / 12 * months, 6);
    }

    public static bool IsFullyDepreciated(Asset asset, decimal accumulatedDepreciation)
    {
        var residualValue = CalculateResidualValue(asset);
        return accumulatedDepreciation >= (asset.OriginalValue - residualValue);
    }

    public static decimal CalculateAccumulatedDepreciation(Asset asset, DateOnly asOfDate)
    {
        return asset.AccumulatedDepreciation;
    }

    private static decimal CalculateResidualValue(Asset asset)
    {
        var residualPercentage = asset.AssetGroup?.ResidualValuePercentage ?? 0;
        return Math.Round(asset.OriginalValue * residualPercentage / 100, 2);
    }
}
