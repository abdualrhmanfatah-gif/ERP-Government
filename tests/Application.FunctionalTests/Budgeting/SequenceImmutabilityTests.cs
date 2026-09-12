using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Queries.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class SequenceImmutabilityTests : TestBase
{
    [Test]
    public async Task BudgetNumber_ShouldNotChangeOnApprove()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Immutability Budget", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var originalNumber = budget.BudgetNumber;
        originalNumber.ShouldNotBeNullOrWhiteSpace();
        originalNumber.ShouldStartWith("BGT-");

        var submitResult = await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));
        submitResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.BudgetNumber.ShouldBe(originalNumber);

        var approveResult = await TestApp.SendAsync(new ApproveBudgetCommand(budgetId, budget.RowVersion));
        approveResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.BudgetNumber.ShouldBe(originalNumber);
    }

    [Test]
    public async Task BudgetNumber_ShouldNotChangeOnActivate()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Immutability Budget 2", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var originalNumber = budget.BudgetNumber;

        await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));
        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new ApproveBudgetCommand(budgetId, budget.RowVersion));
        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new ActivateBudgetCommand(budgetId, budget.RowVersion));

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.BudgetNumber.ShouldBe(originalNumber);
    }

    [Test]
    public async Task BudgetNumber_ShouldNotChangeOnUpdate()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Immutability Budget 3", 1, 1, 1));
        createResult.Succeeded.ShouldBeTrue();
        var budgetId = createResult.Value;

        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        var originalNumber = budget.BudgetNumber;

        var updateResult = await TestApp.SendAsync(new UpdateBudgetCommand(
            budgetId, "SHOULD-NOT-CHANGE", "Updated Name", 1, 1, 1, budget.RowVersion));
        updateResult.Succeeded.ShouldBeTrue();

        budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        budget.BudgetNumber.ShouldBe(originalNumber);
        budget.BudgetNumber.ShouldNotBe("SHOULD-NOT-CHANGE");
    }

    [Test]
    public async Task BudgetTransactionNumber_ShouldBeAssignedAtCreation()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Immutability Budget 4", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        var budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = budgetId,
            ItemCode = "IM-001",
            ItemName = "Immutability Item"
        };
        await TestApp.AddAsync(item);

        var createResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), "PO", 1, "Number test",
            [new BudgetTransactionLineRequest(item.Id, TransactionDirection.Increase, 5000m, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var transactionId = createResult.Value;

        var transaction = await TestApp.FindAsync<BudgetTransaction>(transactionId);
        transaction!.TransactionNumber.ShouldNotBeNullOrWhiteSpace();
        transaction.TransactionNumber.ShouldStartWith("BTR-");
    }
}
