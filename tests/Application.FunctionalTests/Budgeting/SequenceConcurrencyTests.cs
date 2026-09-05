using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class SequenceConcurrencyTests : TestBase
{
    [Test]
    public async Task ParallelAppropriationCreates_ShouldYieldDistinctNumbers()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Concurrency Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        var budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = budgetId,
            ItemCode = "PS-001",
            ItemName = "Test Item"
        };
        await TestApp.AddAsync(item);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => TestApp.SendAsync(new CreateAppropriationCommand(
                budgetId, item.Id, AppropriationType.Original, "PO", 1, 1000m)))
            .ToList();

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            result.Succeeded.ShouldBeTrue();
        }

        var numbers = results.Select(r => r.Value).ToList();
        var distinctIds = numbers.Distinct().ToList();
        distinctIds.Count.ShouldBe(10, "All appropriation IDs should be distinct");
    }

    [Test]
    public async Task ParallelBudgetCreates_ShouldYieldDistinctNumbers()
    {
        await TestApp.RunAsAdministratorAsync();

        var tasks = Enumerable.Range(0, 5)
            .Select(i => TestApp.SendAsync(new CreateBudgetCommand(
                $"Budget {i}", 1, 1, 1)))
            .ToList();

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            result.Succeeded.ShouldBeTrue();
        }

        var budgetIds = results.Select(r => r.Value).ToList();
        budgetIds.Distinct().Count().ShouldBe(5, "All budget IDs should be distinct");
    }
}
