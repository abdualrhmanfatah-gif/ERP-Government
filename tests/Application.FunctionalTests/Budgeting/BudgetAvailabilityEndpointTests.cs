using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class BudgetAvailabilityEndpointTests : TestBase
{
    [Test]
    public async Task GetBudgetItemAvailability_WithSeedData_ShouldReturnCorrectFigures()
    {
        var budgetResult = await TestApp.SendAsync(new ERP_Government.Application.Budgeting.Commands.Budgets.CreateBudgetCommand(
            "Availability Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();

        var budget = await TestApp.FindAsync<ERP_Government.Domain.Budgeting.Entities.Budget>(budgetResult.Value);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new ERP_Government.Domain.Budgeting.Entities.BudgetItem
        {
            BudgetId = budgetResult.Value,
            ItemCode = "AV-001",
            ItemName = "Availability Item"
        };
        await TestApp.AddAsync(item);

        var appResult = await TestApp.SendAsync(new ERP_Government.Application.Budgeting.Commands.Appropriations.CreateAppropriationCommand(
            budgetResult.Value, item.Id, AppropriationType.Original, "PO", 1, 10000m));
        appResult.Succeeded.ShouldBeTrue();
        var app = await TestApp.FindAsync<ERP_Government.Domain.Budgeting.Entities.Appropriation>(appResult.Value);
        app!.Status = AppropriationStatus.Active;
        await TestApp.AddAsync(app);

        var service = new BudgetAvailabilityService(null!);
        var summary = await service.GetAvailabilitySummaryAsync(item.Id);

        summary.NetAppropriated.ShouldBe(10000m);
        summary.Encumbered.ShouldBe(0m);
        summary.Available.ShouldBe(10000m);
    }

    [Test]
    public async Task GetBudgetItemAvailability_UnknownId_ShouldReturnZeroSummary()
    {
        var service = new BudgetAvailabilityService(null!);
        var summary = await service.GetAvailabilitySummaryAsync(999999);

        summary.NetAppropriated.ShouldBe(0m);
        summary.Encumbered.ShouldBe(0m);
        summary.Available.ShouldBe(0m);
    }
}
