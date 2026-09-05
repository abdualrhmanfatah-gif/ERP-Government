using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class TransferPairTests : TestBase
{
    private int _budgetId;
    private int _sourceItemId;
    private int _targetItemId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Transfer Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var sourceItem = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "TR-SRC",
            ItemName = "Source Item"
        };
        await TestApp.AddAsync(sourceItem);
        _sourceItemId = sourceItem.Id;

        var targetItem = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "TR-TGT",
            ItemName = "Target Item"
        };
        await TestApp.AddAsync(targetItem);
        _targetItemId = targetItem.Id;

        var appResult = await TestApp.SendAsync(new CreateAppropriationCommand(
            _budgetId, _sourceItemId, AppropriationType.Original, "PO", 1, 50000m));
        appResult.Succeeded.ShouldBeTrue();

        var appropriation = await TestApp.FindAsync<Appropriation>(appResult.Value);
        appropriation!.Status = AppropriationStatus.Active;
        await TestApp.AddAsync(appropriation);
    }

    [Test]
    public async Task CreateTransfer_ShouldCreateTwoRows()
    {
        var result = await TestApp.SendAsync(new CreateTransferAppropriationCommand(
            _budgetId, _sourceItemId, _targetItemId, 10000m));

        result.Succeeded.ShouldBeTrue();

        var sourceApp = await TestApp.FindAsync<Appropriation>(result.Value.SourceId);
        sourceApp.ShouldNotBeNull();
        sourceApp.AppropriationType.ShouldBe(AppropriationType.Transfer);
        sourceApp.Amount.ShouldBe(-10000m);
        sourceApp.BudgetItemId.ShouldBe(_sourceItemId);
        sourceApp.TargetBudgetItemId.ShouldBe(_targetItemId);

        var targetApp = await TestApp.FindAsync<Appropriation>(result.Value.TargetId);
        targetApp.ShouldNotBeNull();
        targetApp.AppropriationType.ShouldBe(AppropriationType.Transfer);
        targetApp.Amount.ShouldBe(10000m);
        targetApp.BudgetItemId.ShouldBe(_targetItemId);
        targetApp.TargetBudgetItemId.ShouldBe(_sourceItemId);
    }

    [Test]
    public async Task CreateTransfer_DifferentBudgets_ShouldFail()
    {
        var otherBudgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Other Budget", 1, 1, 1));
        var otherItem = new BudgetItem
        {
            BudgetId = otherBudgetResult.Value,
            ItemCode = "OT-001",
            ItemName = "Other"
        };
        await TestApp.AddAsync(otherItem);

        var result = await TestApp.SendAsync(new CreateTransferAppropriationCommand(
            _budgetId, _sourceItemId, otherItem.Id, 10000m));

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateTransfer_SourceEqualsTarget_ShouldFail()
    {
        var result = await TestApp.SendAsync(new CreateTransferAppropriationCommand(
            _budgetId, _sourceItemId, _sourceItemId, 10000m));

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CreateTransfer_ExceedsAvailability_ShouldFail()
    {
        var result = await TestApp.SendAsync(new CreateTransferAppropriationCommand(
            _budgetId, _sourceItemId, _targetItemId, 100000m));

        result.Succeeded.ShouldBeFalse();
    }
}
