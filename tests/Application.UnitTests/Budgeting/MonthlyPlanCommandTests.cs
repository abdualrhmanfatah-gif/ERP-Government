using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class MonthlyPlanCommandTests
{
    [Test]
    public void Month_MustBeBetween1And12_InvalidMonth0_ShouldFail()
    {
        var errors = ValidateMonthlyPlanEntry(0, 1000m);
        errors.ShouldContain(e => e.Contains("1") && e.Contains("12"));
    }

    [Test]
    public void Month_MustBeBetween1And12_InvalidMonth13_ShouldFail()
    {
        var errors = ValidateMonthlyPlanEntry(13, 1000m);
        errors.ShouldContain(e => e.Contains("1") && e.Contains("12"));
    }

    [Test]
    public void PlannedAmount_MustBeNonNegative_Negative_ShouldFail()
    {
        var errors = ValidateMonthlyPlanEntry(1, -500m);
        errors.ShouldContain(e => e.Contains("greater than or equal to 0"));
    }

    [Test]
    public void PlannedAmount_Zero_ShouldPass()
    {
        var errors = ValidateMonthlyPlanEntry(1, 0m);
        errors.ShouldBeEmpty();
    }

    [Test]
    public void PlannedAmount_Positive_ShouldPass()
    {
        var errors = ValidateMonthlyPlanEntry(6, 5000m);
        errors.ShouldBeEmpty();
    }

    [Test]
    public void DuplicateMonths_ShouldFail()
    {
        var entries = new[] { (1, 1000m), (2, 2000m), (1, 3000m) };
        var errors = ValidateMonthlyPlanBatch(entries);
        errors.ShouldContain(e => e.Contains("duplicate"));
    }

    [Test]
    public void Batch_MoreThan12_ShouldFail()
    {
        var entries = Enumerable.Range(1, 13).Select(m => (m, 1000m)).ToArray();
        var errors = ValidateMonthlyPlanBatch(entries);
        errors.ShouldContain(e => e.Contains("12"));
    }

    [Test]
    public void ValidBatch_12Months_ShouldPass()
    {
        var entries = Enumerable.Range(1, 12).Select(m => (m, m * 100m)).ToArray();
        var errors = ValidateMonthlyPlanBatch(entries);
        errors.ShouldBeEmpty();
    }

    private static List<string> ValidateMonthlyPlanEntry(int month, decimal amount)
    {
        var errors = new List<string>();
        if (month < 1 || month > 12)
            errors.Add("Month must be between 1 and 12.");
        if (amount < 0)
            errors.Add("Planned amount must be greater than or equal to 0.");
        return errors;
    }

    private static List<string> ValidateMonthlyPlanBatch((int Month, decimal Amount)[] entries)
    {
        var errors = new List<string>();
        if (entries.Length > 12)
            errors.Add("Batch cannot exceed 12 entries.");
        var months = entries.Select(e => e.Month).ToList();
        if (months.Distinct().Count() != months.Count)
            errors.Add("Duplicate months are not allowed.");
        foreach (var entry in entries)
        {
            errors.AddRange(ValidateMonthlyPlanEntry(entry.Month, entry.Amount));
        }
        return errors;
    }
}
