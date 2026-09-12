using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CancelBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.DeleteBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ReverseBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.UpdateBudgetTransaction;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class BudgetTransactionEdgeCaseTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId1;
    private int _budgetItemId2;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Edge Case Test Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item1 = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "EC-001",
            ItemName = "Edge Case Item 1"
        };
        await TestApp.AddAsync(item1);
        _budgetItemId1 = item1.Id;

        var item2 = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "EC-002",
            ItemName = "Edge Case Item 2"
        };
        await TestApp.AddAsync(item2);
        _budgetItemId2 = item2.Id;
    }

    [Test]
    public async Task Create_WithNoLines_ShouldReject()
    {
        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "No lines",
            []));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("line"));
    }

    [Test]
    public async Task Create_WithInvalidBudgetItemId_ShouldReject()
    {
        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Invalid item",
            [
                new BudgetTransactionLineRequest(99999, TransactionDirection.Increase, 1000m, null)
            ]));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Create_OnNonActiveBudget_ShouldReject()
    {
        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Inactive Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        var inactiveBudgetId = budgetResult.Value;

        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            inactiveBudgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Inactive budget",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Active"));
    }

    [Test]
    public async Task Submit_NonDraft_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Submit non-draft",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var submitAgain = await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        submitAgain.Succeeded.ShouldBeFalse();
        submitAgain.Errors.ShouldContain(e => e.Contains("Only Draft"));
    }

    [Test]
    public async Task Approve_NonSubmitted_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Approve non-submitted",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var approveResult = await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));
        approveResult.Succeeded.ShouldBeFalse();
        approveResult.Errors.ShouldContain(e => e.Contains("Only Submitted"));
    }

    [Test]
    public async Task Post_NonApproved_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Post non-approved",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var postResult = await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        postResult.Succeeded.ShouldBeFalse();
        postResult.Errors.ShouldContain(e => e.Contains("Only Approved"));
    }

    [Test]
    public async Task Update_NonDraft_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Update non-draft",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var updateResult = await TestApp.SendAsync(new UpdateBudgetTransactionCommand(
            transactionId, null, null, null, "Updated", null, transaction.RowVersion));
        updateResult.Succeeded.ShouldBeFalse();
        updateResult.Errors.ShouldContain(e => e.Contains("Only Draft"));
    }

    [Test]
    public async Task Delete_NonDraft_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Delete non-draft",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var deleteResult = await TestApp.SendAsync(new DeleteBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        deleteResult.Succeeded.ShouldBeFalse();
        deleteResult.Errors.ShouldContain(e => e.Contains("Only Draft"));
    }

    [Test]
    public async Task Cancel_NonSubmitted_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Cancel non-submitted",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var cancelResult = await TestApp.SendAsync(new CancelBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));
        cancelResult.Succeeded.ShouldBeFalse();
        cancelResult.Errors.ShouldContain(e => e.Contains("Only Submitted"));
    }

    [Test]
    public async Task Reverse_NonPosted_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Reverse non-posted",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var reverseResult = await TestApp.SendAsync(new ReverseBudgetTransactionCommand(
            transactionId, transaction.RowVersion, "reason"));
        reverseResult.Succeeded.ShouldBeFalse();
        reverseResult.Errors.ShouldContain(e => e.Contains("Only Posted"));
    }

    [Test]
    public async Task Transfer_UnbalancedLines_ShouldReject()
    {
        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.Transfer,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Unbalanced transfer",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Decrease, 5000m, "Source"),
                new BudgetTransactionLineRequest(_budgetItemId2, TransactionDirection.Increase, 4000m, "Target")
            ]));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("balance"));
    }

    [Test]
    public async Task Transfer_BalancedLines_ShouldSucceed()
    {
        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.Transfer,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Balanced transfer",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Decrease, 5000m, "Source"),
                new BudgetTransactionLineRequest(_budgetItemId2, TransactionDirection.Increase, 5000m, "Target")
            ]));
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ConcurrencyConflict_Submit_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Concurrency test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var staleVersion = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        var submitResult = await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, staleVersion));
        submitResult.Succeeded.ShouldBeFalse();
        submitResult.Errors.ShouldContain(e => e.Contains("Concurrency"));
    }

    [Test]
    public async Task CarryForward_WithoutTargetBudgetId_ShouldReject()
    {
        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.CarryForward,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Carry forward no target",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Decrease, 1000m, null)
            ]));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Target budget"));
    }

    [Test]
    public async Task CarryForward_SameFiscalYear_ShouldReject()
    {
        var targetBudgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Target Budget Same FY", 1, 1, 1));
        targetBudgetResult.Succeeded.ShouldBeTrue();
        var targetBudgetId = targetBudgetResult.Value;

        var targetBudget = (await TestApp.FindAsync<Budget>(targetBudgetId))!;
        targetBudget.Status = BudgetStatus.Active;
        await TestApp.AddAsync(targetBudget);

        var targetItem = new BudgetItem
        {
            BudgetId = targetBudgetId,
            ItemCode = "CF-001",
            ItemName = "Carry Forward Target Item"
        };
        await TestApp.AddAsync(targetItem);

        var result = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.CarryForward,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Same FY carry forward",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Decrease, 1000m, null),
                new BudgetTransactionLineRequest(targetItem.Id, TransactionDirection.Increase, 1000m, null)
            ],
            TargetBudgetId: targetBudgetId));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("fiscal year"));
    }

    [Test]
    public async Task Update_DraftWithNewLines_ShouldReplaceLines()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Update lines test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 5000m, "Original")
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var updateResult = await TestApp.SendAsync(new UpdateBudgetTransactionCommand(
            transactionId, null, null, null, "Updated description",
            [
                new UpdateBudgetTransactionLineRequest(null, _budgetItemId2, TransactionDirection.Increase, 3000m, "New line")
            ],
            transaction.RowVersion));
        updateResult.Succeeded.ShouldBeTrue();

        var lines = await TestApp.WhereAsync<BudgetTransactionLine>(
            l => l.BudgetTransactionId == transactionId);
        lines.Count.ShouldBe(1);
        lines[0].BudgetItemId.ShouldBe(_budgetItemId2);
        lines[0].Amount.ShouldBe(3000m);
    }

    [Test]
    public async Task Delete_Draft_ShouldRemoveTransactionAndLines()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Delete draft test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 5000m, null),
                new BudgetTransactionLineRequest(_budgetItemId2, TransactionDirection.Increase, 3000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var deleteResult = await TestApp.SendAsync(new DeleteBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        deleteResult.Succeeded.ShouldBeTrue();

        var deleted = await TestApp.FindAsync<BudgetTransaction>(transactionId);
        deleted.ShouldBeNull();

        var lines = await TestApp.WhereAsync<BudgetTransactionLine>(
            l => l.BudgetTransactionId == transactionId);
        lines.ShouldBeEmpty();
    }
}
