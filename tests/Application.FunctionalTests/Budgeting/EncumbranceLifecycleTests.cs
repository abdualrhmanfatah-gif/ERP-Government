using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.Encumbrances;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class EncumbranceLifecycleTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Encumbrance Lifecycle Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "EC-001",
            ItemName = "Encumbrance Test Item"
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
    public async Task FullLifecycle_DraftToActive_ShouldHaveOneApprovalHistoryPerTransition()
    {
        var createResult = await TestApp.SendAsync(new CreateEncumbranceCommand(
            EncumbranceType.Commitment, null, null,
            "PO", 1, "Test encumbrance", DateOnly.FromDateTime(DateTime.UtcNow),
            [new EncumbranceLineRequest(_budgetItemId, 10000m, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var encumbranceId = createResult.Value;

        var encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        encumbrance!.Status.ShouldBe(EncumbranceStatus.Draft);

        var submitResult = await TestApp.SendAsync(new SubmitEncumbranceCommand(
            encumbranceId, encumbrance.RowVersion));
        submitResult.Succeeded.ShouldBeTrue();

        encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        encumbrance!.Status.ShouldBe(EncumbranceStatus.PendingApproval);

        var approveResult = await TestApp.SendAsync(new ApproveEncumbranceCommand(
            encumbranceId, encumbrance.RowVersion, null));
        approveResult.Succeeded.ShouldBeTrue();

        encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        encumbrance!.Status.ShouldBe(EncumbranceStatus.Approved);

        var activateResult = await TestApp.SendAsync(new ActivateEncumbranceCommand(
            encumbranceId, encumbrance.RowVersion));
        activateResult.Succeeded.ShouldBeTrue();

        encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        encumbrance!.Status.ShouldBe(EncumbranceStatus.Active);

        var history = await TestApp.FindAsync<ApprovalHistory>(1);
        history.ShouldNotBeNull();
        history.DocumentType.ShouldBe("Encumbrance");
        history.DocumentId.ShouldBe(encumbranceId);
    }

    [Test]
    public async Task DraftOnly_UpdateAndDelete_ShouldOnlyWorkOnDraft()
    {
        var createResult = await TestApp.SendAsync(new CreateEncumbranceCommand(
            EncumbranceType.Commitment, null, null,
            "PO", 1, "Draft encumbrance", DateOnly.FromDateTime(DateTime.UtcNow),
            [new EncumbranceLineRequest(_budgetItemId, 5000m, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var encumbranceId = createResult.Value;

        var encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);

        var updateResult = await TestApp.SendAsync(new UpdateEncumbranceCommand(
            encumbranceId, "Updated desc", null, null, encumbrance!.RowVersion));
        updateResult.Succeeded.ShouldBeTrue();

        var submitResult = await TestApp.SendAsync(new SubmitEncumbranceCommand(
            encumbranceId, (await TestApp.FindAsync<Encumbrance>(encumbranceId))!.RowVersion));
        submitResult.Succeeded.ShouldBeTrue();

        encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        var updateAfterSubmit = await TestApp.SendAsync(new UpdateEncumbranceCommand(
            encumbranceId, "Should fail", null, null, encumbrance!.RowVersion));
        updateAfterSubmit.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Cancel_ShouldWorkFromDraftAndActive()
    {
        var createResult = await TestApp.SendAsync(new CreateEncumbranceCommand(
            EncumbranceType.Commitment, null, null,
            "PO", 1, "Cancel test", DateOnly.FromDateTime(DateTime.UtcNow),
            [new EncumbranceLineRequest(_budgetItemId, 2000m, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var encumbranceId = createResult.Value;

        var encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        var cancelResult = await TestApp.SendAsync(new CancelEncumbranceCommand(
            encumbranceId, encumbrance!.RowVersion, null));
        cancelResult.Succeeded.ShouldBeTrue();

        encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        encumbrance!.Status.ShouldBe(EncumbranceStatus.Cancelled);
    }
}
