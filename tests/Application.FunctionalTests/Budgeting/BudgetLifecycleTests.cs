using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Queries.Budgets;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class BudgetLifecycleTests : TestBase
{
    [Test]
    public async Task FullLifecycle_DraftToClosed_ShouldComplete()
    {
        await TestApp.RunAsAdministratorAsync();

        // Create budget in Draft
        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Lifecycle Budget", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        // Get the budget to obtain RowVersion
        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Draft);

        // Submit: Draft -> Submitted
        var submitResult = await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));
        submitResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Submitted);

        // Approve: Submitted -> Approved
        var approveResult = await TestApp.SendAsync(new ApproveBudgetCommand(budgetId, budget.RowVersion));
        approveResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Approved);

        // Activate: Approved -> Active
        var activateResult = await TestApp.SendAsync(new ActivateBudgetCommand(budgetId, budget.RowVersion));
        activateResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Active);

        // Suspend: Active -> Suspended
        var suspendResult = await TestApp.SendAsync(new SuspendBudgetCommand(budgetId, budget.RowVersion));
        suspendResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Suspended);

        // Re-activate: Suspended -> Active
        // Note: Re-activation from Suspended is not in the FSM. Close instead.
        // Close: Suspended -> Closed
        var closeResult = await TestApp.SendAsync(new CloseBudgetCommand(budgetId, budget.RowVersion));
        closeResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Closed);
    }

    [Test]
    public async Task CancelFromDraft_ShouldSucceed()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Cancel From Draft", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var cancelResult = await TestApp.SendAsync(new CancelBudgetCommand(budgetId, budget.RowVersion));
        cancelResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task CancelFromSubmitted_ShouldSucceed()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Cancel From Submitted", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var cancelResult = await TestApp.SendAsync(new CancelBudgetCommand(budgetId, budget.RowVersion));
        cancelResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task CancelFromSuspended_ShouldSucceed()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Cancel From Suspended", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        // Draft -> Submitted -> Approved -> Active -> Suspended
        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new ApproveBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new ActivateBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new SuspendBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var cancelResult = await TestApp.SendAsync(new CancelBudgetCommand(budgetId, budget.RowVersion));
        cancelResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task InvalidTransition_DraftToActive_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Invalid Transition", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var activateResult = await TestApp.SendAsync(new ActivateBudgetCommand(budgetId, budget.RowVersion));
        activateResult.Succeeded.ShouldBeFalse();
        activateResult.Errors.ShouldContain(e => e.Contains("Only Approved budgets can be activated"));
    }

    [Test]
    public async Task InvalidTransition_DraftToClosed_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Invalid Transition", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var closeResult = await TestApp.SendAsync(new CloseBudgetCommand(budgetId, budget.RowVersion));
        closeResult.Succeeded.ShouldBeFalse();
        closeResult.Errors.ShouldContain(e => e.Contains("Only Active or Suspended budgets can be closed"));
    }

    [Test]
    public async Task UpdateBudget_NonDraft_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Update Non-Draft", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        // Submit to move out of Draft
        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var updateResult = await TestApp.SendAsync(new UpdateBudgetCommand(
            budgetId, "UPDATED", "Updated Name", 1, 1, 1, budget.RowVersion));
        updateResult.Succeeded.ShouldBeFalse();
        updateResult.Errors.ShouldContain(e => e.Contains("Only Draft budgets can be updated"));
    }

    [Test]
    public async Task CreateBudget_WithDuplicateNumber_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result1 = await TestApp.SendAsync(new CreateBudgetCommand(
            "First Budget", 1, 1, 1));
        result1.Succeeded.ShouldBeTrue();

        var result2 = await TestApp.SendAsync(new CreateBudgetCommand(
            "Duplicate Budget", 1, 1, 1));
        result2.Succeeded.ShouldBeFalse();
        result2.Errors.ShouldContain(e => e.Contains("already exists"));
    }
}
