using ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;
using ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class AvailabilitySnapshotReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task GetAvailabilitySnapshot_ShouldReturnBreakdown()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Breakdown.ShouldNotBeNull();
    }

    [Test]
    public async Task GetAvailabilitySnapshot_Availability_ShouldCalculateCorrectly()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Breakdown)
        {
            line.AvailableAmount.ShouldBe(
                line.AppropriationAmount - line.EncumberedAmount - line.PaidAmount,
                $"Availability line {line.FundCode}: Available should equal Appropriated - Encumbered - Paid");
        }
    }

    [Test]
    public async Task GetAvailabilitySnapshot_Totals_ShouldMatchBreakdownSums()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.AppropriationAmount.ShouldBe(result.Breakdown.Sum(b => b.AppropriationAmount));
        result.Totals.EncumberedAmount.ShouldBe(result.Breakdown.Sum(b => b.EncumberedAmount));
        result.Totals.PaidAmount.ShouldBe(result.Breakdown.Sum(b => b.PaidAmount));
        result.Totals.AvailableAmount.ShouldBe(result.Breakdown.Sum(b => b.AvailableAmount));
    }

    [Test]
    public async Task GetAvailabilitySnapshot_ControlState_ShouldBePopulated()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);

        result.ControlState.ShouldNotBeNullOrEmpty(
            "Control state should be populated (e.g., Active, Frozen, Closed)");
    }

    [Test]
    public async Task GetAvailabilitySnapshot_ControlState_ShouldBeValidValue()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);

        var validStates = new[] { "Active", "Frozen", "Closed", "Exhausted" };
        validStates.ShouldContain(result.ControlState,
            $"Control state '{result.ControlState}' should be one of: {string.Join(", ", validStates)}");
    }

    [Test]
    public async Task GetAvailabilitySnapshotDetail_ShouldReturnLineItems()
    {
        var query = new GetAvailabilitySnapshotDetailQuery { BudgetItemId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Appropriations.ShouldNotBeNull();
        result.Encumbrances.ShouldNotBeNull();
        result.Payments.ShouldNotBeNull();
    }

    [Test]
    public async Task GetAvailabilitySnapshot_FilterByFund_ShouldReturnFilteredResults()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1, FundId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Breakdown)
        {
            line.FundId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetAvailabilitySnapshot_AvailableShouldBeNonNegative()
    {
        var query = new GetAvailabilitySnapshotQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Breakdown)
        {
            line.AvailableAmount.ShouldBeGreaterThanOrEqualTo(0,
                $"Availability line {line.FundCode}: Available should not be negative");
        }
    }
}
