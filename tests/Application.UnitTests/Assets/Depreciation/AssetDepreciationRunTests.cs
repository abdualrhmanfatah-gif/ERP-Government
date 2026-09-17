using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Services;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Depreciation;

[TestFixture]
public class AssetDepreciationRunTests
{
    [Test]
    public void Calculate_CapturesGroupMethodRateAndOpeningClosingValues()
    {
        var asset = CreateAsset(originalValue: 1_200m, accumulatedDepreciation: 100m, rate: 0.1m);

        var schedule = DepreciationCalculator.Calculate(asset, ratePerPeriod: 0.1m, periodNumber: 3, totalPeriods: 120);

        schedule.Method.ShouldBe("StraightLine");
        schedule.Rate.ShouldBe(0.1m);
        schedule.OpeningBookValue.ShouldBe(1_100m);
        schedule.OpeningAccumulatedDepreciation.ShouldBe(100m);
        schedule.Amount.ShouldBe(120m);
        schedule.ClosingAccumulatedDepreciation.ShouldBe(220m);
        schedule.ClosingBookValue.ShouldBe(980m);
    }

    [Test]
    public void Calculate_DoesNotDepreciateBelowResidualValue()
    {
        var asset = CreateAsset(originalValue: 1_000m, accumulatedDepreciation: 890m, rate: 0.2m);
        asset.AssetGroup!.ResidualValuePercentage = 10m;

        var schedule = DepreciationCalculator.Calculate(asset, ratePerPeriod: 0.2m, periodNumber: 12, totalPeriods: 120);

        schedule.ResidualValue.ShouldBe(100m);
        schedule.Amount.ShouldBe(10m);
        schedule.ClosingAccumulatedDepreciation.ShouldBe(900m);
        schedule.ClosingBookValue.ShouldBe(100m);
    }

    private static Asset CreateAsset(decimal originalValue, decimal accumulatedDepreciation, decimal rate) =>
        new()
        {
            Id = 1,
            OriginalValue = originalValue,
            AccumulatedDepreciation = accumulatedDepreciation,
            UsefulLifeYears = 10,
            AssetGroup = new AssetGroup
            {
                DepreciationMethod = "StraightLine",
                DepreciationRate = rate,
                DefaultUsefulLifeYears = 10
            }
        };
}
