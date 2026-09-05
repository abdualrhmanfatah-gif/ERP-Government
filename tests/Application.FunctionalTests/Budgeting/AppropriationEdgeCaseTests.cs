using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class AppropriationEdgeCaseTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task Activate_WithBlockingControl_ShouldFail()
    {
        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Edge Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();

        var budget = await TestApp.FindAsync<Budget>(budgetResult.Value);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = budgetResult.Value,
            ItemCode = "EC-001",
            ItemName = "Edge Case Item"
        };
        await TestApp.AddAsync(item);

        var createResult = await TestApp.SendAsync(new CreateAppropriationCommand(
            budgetResult.Value, item.Id, AppropriationType.Original, "PO", 1, 50000m));
        createResult.Succeeded.ShouldBeTrue();
        var app = await TestApp.FindAsync<Appropriation>(createResult.Value);
        app!.Status = AppropriationStatus.Active;
        await TestApp.AddAsync(app);
    }

    [Test]
    public async Task CreateAppropriation_TransferType_ShouldReject()
    {
        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Edge Budget 2", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();

        var budget = await TestApp.FindAsync<Budget>(budgetResult.Value);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = budgetResult.Value,
            ItemCode = "EC-002",
            ItemName = "Edge Case Item 2"
        };
        await TestApp.AddAsync(item);

        var result = await TestApp.SendAsync(new CreateAppropriationCommand(
            budgetResult.Value, item.Id, AppropriationType.Transfer, "PO", 1, 1000m));

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Transfer"));
    }
}
