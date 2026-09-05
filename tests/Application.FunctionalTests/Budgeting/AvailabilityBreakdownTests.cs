using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetAvailabilityBreakdown;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class AvailabilityBreakdownTests : TestBase
{
    // ─── T059: Multi-dimension availability query ────────────────────

    [Test]
    public async Task GetAvailabilityBreakdown_WithSeedData_ShouldReturnBreakdown()
    {
        await TestApp.RunAsAdministratorAsync();

        var budget = new ERP_Government.Domain.Budgeting.Entities.Budget
        {
            BudgetNumber = "B-001", BudgetName = "Test Budget",
            BudgetTypeId = 1, FiscalYearId = 1, FundId = 1,
            TotalAmount = 100000m, Status = BudgetStatus.Active,
            EffectiveFrom = new DateOnly(2026, 1, 1)
        };
        await TestApp.AddAsync(budget);

        var item = new ERP_Government.Domain.Budgeting.Entities.BudgetItem
        {
            BudgetId = budget.Id, ItemCode = "ITEM-001", ItemName = "Test Item"
        };
        await TestApp.AddAsync(item);

        var appropriation = new ERP_Government.Domain.Budgeting.Entities.Appropriation
        {
            BudgetItemId = item.Id, BudgetId = budget.Id,
            Amount = 50000m, AppropriationType = AppropriationType.Original,
            Status = AppropriationStatus.Active,
            AppropriationNumber = "APP-001", DocumentType = "Test", DocumentId = 1
        };
        await TestApp.AddAsync(appropriation);

        var handler = new ERP_Government.Application.Budgeting.Common.BudgetAvailabilityService(null!);
        var breakdown = await handler.GetAvailabilityBreakdownAsync(item.Id, 1);

        breakdown.ShouldNotBeEmpty();
        breakdown[0].AppropriationAmount.ShouldBe(50000m);
    }
}
