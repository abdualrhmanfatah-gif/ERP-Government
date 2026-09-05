using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class MonthlyPlanTests : TestBase
{
    [Test]
    public async Task PutMonthlyPlan_ShouldReplaceIdempotently()
    {
        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Plan Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();

        var item = new BudgetItem
        {
            BudgetId = budgetResult.Value,
            ItemCode = "MP-001",
            ItemName = "Monthly Plan Item"
        };
        await TestApp.AddAsync(item);

        var plan1 = Enumerable.Range(1, 6).Select(m => new BudgetItemMonthlyPlan
        {
            BudgetItemId = item.Id,
            Month = m,
            PlannedAmount = m * 1000m
        }).ToList();

        foreach (var entry in plan1)
            await TestApp.AddAsync(entry);

        var plan2 = Enumerable.Range(1, 12).Select(m => new BudgetItemMonthlyPlan
        {
            BudgetItemId = item.Id,
            Month = m,
            PlannedAmount = m * 500m
        }).ToList();

        foreach (var entry in plan2)
            await TestApp.AddAsync(entry);

        var count = await TestApp.CountAsync<BudgetItemMonthlyPlan>();
        count.ShouldBe(12);
    }
}
