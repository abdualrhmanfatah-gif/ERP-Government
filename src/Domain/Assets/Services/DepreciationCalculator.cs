using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Domain.Assets.Services;

/// <summary>
/// Domain service for calculating depreciation schedules.
/// Supports StraightLine, DecliningBalance, and SumOfYearsDigits methods.
/// UnitsOfProduction deferred to future iteration (requires TotalEstimatedUnits schema change).
/// </summary>
public static class DepreciationCalculator
{
    /// <summary>
    /// Generates depreciation schedules for an asset based on its depreciation method and useful life.
    /// </summary>
    /// <param name="asset">The asset to depreciate</param>
    /// <param name="depreciationMethod">Depreciation method from AssetGroup</param>
    /// <returns>List of depreciation schedule entries</returns>
    public static List<DepreciationSchedule> GenerateSchedules(Asset asset, string depreciationMethod)
    {
        if (asset.UsefulLifeYears is null or <= 0)
            throw new ArgumentException("Useful life must be greater than zero.");

        if (depreciationMethod == "UnitsOfProduction")
            throw new NotSupportedException("UnitsOfProduction method requires TotalEstimatedUnits field (not yet implemented).");

        var totalMonths = asset.UsefulLifeYears.Value * 12;
        var residualValue = asset.ResidualValue ?? 0m;
        var depreciationBase = asset.OriginalValue - residualValue;
        var schedules = new List<DepreciationSchedule>();

        DateOnly periodStart = asset.DepreciationStartDate;
        decimal accumulatedDepreciation = 0m;
        decimal netBookValue = asset.OriginalValue;

        for (int period = 1; period <= totalMonths; period++)
        {
            decimal monthlyDepreciation = depreciationMethod switch
            {
                "StraightLine" => CalculateStraightLine(depreciationBase, totalMonths),
                "DecliningBalance" => CalculateDecliningBalance(netBookValue, asset.UsefulLifeYears.Value, residualValue),
                "SumOfYearsDigits" => CalculateSumOfYearsDigits(depreciationBase, asset.UsefulLifeYears.Value, period, totalMonths),
                _ => throw new NotSupportedException($"Depreciation method {depreciationMethod} is not supported.")
            };

            // Pro-rata for first period
            if (period == 1 && periodStart.Day > 1)
            {
                int daysInMonth = DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
                int daysHeld = daysInMonth - periodStart.Day + 1;
                monthlyDepreciation = monthlyDepreciation * daysHeld / daysInMonth;
            }

            // Ensure we don't depreciate below residual value
            if (netBookValue - monthlyDepreciation < residualValue)
            {
                monthlyDepreciation = netBookValue - residualValue;
            }

            if (monthlyDepreciation <= 0)
                break;

            accumulatedDepreciation += monthlyDepreciation;
            netBookValue -= monthlyDepreciation;

            schedules.Add(new DepreciationSchedule
            {
                AssetId = asset.Id,
                DepreciationDate = periodStart,
                DepreciationMethod = depreciationMethod,
                DepreciationBase = depreciationBase,
                DepreciationRate = GetRate(depreciationMethod, asset.UsefulLifeYears.Value),
                PeriodNumber = period,
                TotalPeriods = totalMonths,
                Amount = Math.Round(monthlyDepreciation, 2),
                AccumulatedDepreciation = Math.Round(accumulatedDepreciation, 2),
                NetBookValue = Math.Round(netBookValue, 2),
                Status = "Planned"
            });

            periodStart = periodStart.AddMonths(1);
        }

        return schedules;
    }

    private static decimal CalculateStraightLine(decimal depreciationBase, int totalMonths)
    {
        return depreciationBase / totalMonths;
    }

    private static decimal CalculateDecliningBalance(decimal netBookValue, int usefulLifeYears, decimal residualValue)
    {
        // Double-declining balance: Rate = 2 / UsefulLife
        decimal rate = 2m / usefulLifeYears;
        decimal annualDepreciation = netBookValue * rate;
        return annualDepreciation / 12;
    }

    private static decimal CalculateSumOfYearsDigits(decimal depreciationBase, int usefulLifeYears, int currentPeriod, int totalMonths)
    {
        // Sum of years' digits: SYD = n(n+1)/2
        int sumOfYears = usefulLifeYears * (usefulLifeYears + 1) / 2;

        // Convert period to year for SYD calculation
        int currentYear = (currentPeriod - 1) / 12 + 1;
        int remainingLife = usefulLifeYears - currentYear + 1;

        decimal annualDepreciation = depreciationBase * remainingLife / sumOfYears;
        return annualDepreciation / 12;
    }

    private static decimal GetRate(string method, int usefulLifeYears)
    {
        return method switch
        {
            "StraightLine" => 1m / usefulLifeYears,
            "DecliningBalance" => 2m / usefulLifeYears,
            "SumOfYearsDigits" => 1m / usefulLifeYears,
            _ => 0m
        };
    }
}
