using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CancelBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ReverseBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class BudgetTransactionLifecycleTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId1;
    private int _budgetItemId2;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Lifecycle Test Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item1 = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "LC-001",
            ItemName = "Lifecycle Test Item 1"
        };
        await TestApp.AddAsync(item1);
        _budgetItemId1 = item1.Id;

        var item2 = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "LC-002",
            ItemName = "Lifecycle Test Item 2"
        };
        await TestApp.AddAsync(item2);
        _budgetItemId2 = item2.Id;
    }

    [Test]
    public async Task FullLifecycle_DraftToPosted_ShouldCompleteWithTimestamps()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), "PO", 1, "Initial appropriation",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 50000m, "Line 1"),
                new BudgetTransactionLineRequest(_budgetItemId2, TransactionDirection.Increase, 30000m, "Line 2")
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Draft);
        transaction.ApprovedAt.ShouldBeNull();
        transaction.PostedAt.ShouldBeNull();

        var submitResult = await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        submitResult.Succeeded.ShouldBeTrue();

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Submitted);

        var approveResult = await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));
        approveResult.Succeeded.ShouldBeTrue();

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Approved);
        transaction.ApprovedAt.ShouldNotBeNull();

        var postResult = await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));
        postResult.Succeeded.ShouldBeTrue();

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Posted);
        transaction.PostedAt.ShouldNotBeNull();
        transaction.PostedBy.ShouldNotBeNull();
        transaction.TransactionNumber.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task FullLifecycle_ShouldCreateApprovalHistoryForEachTransition()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Approval history test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 10000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;

        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        var history = await TestApp.WhereAsync<ApprovalHistory>(
            h => h.DocumentType == "BudgetTransaction" && h.DocumentId == transactionId);
        history.Count.ShouldBe(3);
        history.ShouldContain(h => h.Decision == "Draft -> Submitted");
        history.ShouldContain(h => h.Decision == "Submitted -> Approved");
        history.ShouldContain(h => h.Decision == "Approved -> Posted");
    }

    [Test]
    public async Task Cancel_FromSubmitted_ShouldTransitionToRejected()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Cancel test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 5000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var cancelResult = await TestApp.SendAsync(new CancelBudgetTransactionCommand(
            transactionId, transaction.RowVersion, "No longer needed"));
        cancelResult.Succeeded.ShouldBeTrue();

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Rejected);

        var history = await TestApp.WhereAsync<ApprovalHistory>(
            h => h.DocumentType == "BudgetTransaction"
                && h.DocumentId == transactionId
                && h.Decision == "Submitted -> Rejected");
        history.Count.ShouldBe(1);
        history[0].Reason.ShouldBe("No longer needed");
    }

    [Test]
    public async Task Reverse_FromPosted_ShouldCreateReversalWithFlippedDirections()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), "PO", 100, "Reverse test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 20000m, "Inc line"),
                new BudgetTransactionLineRequest(_budgetItemId2, TransactionDirection.Increase, 10000m, "Inc line 2")
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var reverseResult = await TestApp.SendAsync(new ReverseBudgetTransactionCommand(
            transactionId, transaction.RowVersion, "Incorrect amounts"));
        reverseResult.Succeeded.ShouldBeTrue();
        var reversalId = reverseResult.Value;

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.Status.ShouldBe(BudgetTransactionStatus.Reversed);

        var reversal = (await TestApp.FindAsync<BudgetTransaction>(reversalId))!;
        reversal.TransactionType.ShouldBe(BudgetTransactionType.Reversal);
        reversal.Status.ShouldBe(BudgetTransactionStatus.Posted);
        reversal.ReversalOfId.ShouldBe(transactionId);
        reversal.Description.ShouldNotBeNull();
        reversal.Description!.ShouldContain("Reversal of");
        reversal.PostedAt.ShouldNotBeNull();

        var originalLines = await TestApp.WhereAsync<BudgetTransactionLine>(
            l => l.BudgetTransactionId == transactionId);
        var reversalLines = await TestApp.WhereAsync<BudgetTransactionLine>(
            l => l.BudgetTransactionId == reversalId);

        reversalLines.Count.ShouldBe(originalLines.Count);

        foreach (var origLine in originalLines)
        {
            var revLine = reversalLines.First(l => l.BudgetItemId == origLine.BudgetItemId);
            revLine.Direction.ShouldNotBe(origLine.Direction);
            revLine.Amount.ShouldBe(origLine.Amount);
        }
    }

    [Test]
    public async Task Reverse_ReversalTransaction_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Double reverse test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 5000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var reverseResult = await TestApp.SendAsync(new ReverseBudgetTransactionCommand(
            transactionId, transaction.RowVersion, "First reversal"));
        reverseResult.Succeeded.ShouldBeTrue();
        var reversalId = reverseResult.Value;

        var reversal = (await TestApp.FindAsync<BudgetTransaction>(reversalId))!;
        var doubleReverseResult = await TestApp.SendAsync(new ReverseBudgetTransactionCommand(
            reversalId, reversal.RowVersion, "Attempt double reversal"));
        doubleReverseResult.Succeeded.ShouldBeFalse();
        doubleReverseResult.Errors.ShouldContain(e => e.Contains("cannot be reversed"));
    }

    [Test]
    public async Task Reverse_WithoutReason_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "No reason test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 5000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var reverseResult = await TestApp.SendAsync(new ReverseBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));
        reverseResult.Succeeded.ShouldBeFalse();
        reverseResult.Errors.ShouldContain(e => e.Contains("reason"));
    }

    [Test]
    public async Task Post_ShouldGenerateTransactionNumber()
    {
        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), null, null, "Number test",
            [
                new BudgetTransactionLineRequest(_budgetItemId1, TransactionDirection.Increase, 1000m, null)
            ]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.TransactionNumber.ShouldNotBeNullOrEmpty();

        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(
            transactionId, transaction.RowVersion, null));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        var originalNumber = transaction.TransactionNumber;
        await TestApp.SendAsync(new PostBudgetTransactionCommand(
            transactionId, transaction.RowVersion));

        transaction = (await TestApp.FindAsync<BudgetTransaction>(transactionId))!;
        transaction.TransactionNumber.ShouldBe(originalNumber);
    }
}
