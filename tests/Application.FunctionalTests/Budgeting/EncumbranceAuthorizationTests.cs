using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.Encumbrances;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class EncumbranceAuthorizationTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Auth Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "AU-001",
            ItemName = "Auth Test Item"
        };
        await TestApp.AddAsync(item);
        _budgetItemId = item.Id;

        var txResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), "PO", 1, "Seed appropriation",
            [new BudgetTransactionLineRequest(_budgetItemId, TransactionDirection.Increase, 50000m, null)]));
        txResult.Succeeded.ShouldBeTrue();

        var tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(tx!.Id, tx.RowVersion));
        tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(tx!.Id, tx.RowVersion, null));
        tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new PostBudgetTransactionCommand(tx!.Id, tx.RowVersion));
    }

    [Test]
    public async Task Unauthenticated_CreateEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new CreateEncumbranceCommand(
            EncumbranceType.Commitment, null, null,
            "PO", 1, "Unauthorized", DateOnly.FromDateTime(DateTime.UtcNow),
            [new EncumbranceLineRequest(_budgetItemId, 1000m, null)]));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_SubmitEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new SubmitEncumbranceCommand(1, [1, 2, 3]));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_ApproveEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new ApproveEncumbranceCommand(1, [1, 2, 3], null));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_ActivateEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new ActivateEncumbranceCommand(1, [1, 2, 3]));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_ReverseEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new ReverseEncumbranceCommand(1, [1, 2, 3], "reason"));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_CancelEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new CancelEncumbranceCommand(1, [1, 2, 3], null));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_SuspendEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new SuspendEncumbranceCommand(1, [1, 2, 3], null));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Unauthenticated_CloseEncumbrance_ShouldFail()
    {
        await TestApp.ResetState();

        var act = () => TestApp.SendAsync(new CloseEncumbranceCommand(1, [1, 2, 3], null));

        var result = await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }
}
