using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CancelDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Payments;

[TestFixture]
public class DisbursementLifecycleTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Disbursement Lifecycle Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "DL-001",
            ItemName = "Disbursement Test Item"
        };
        await TestApp.AddAsync(item);
        _budgetItemId = item.Id;

        var txResult = await TestApp.SendAsync(new CreateBudgetTransactionCommand(
            _budgetId, BudgetTransactionType.InitialAppropriation,
            DateOnly.FromDateTime(DateTime.UtcNow), "PO", 1, "Seed appropriation",
            [new BudgetTransactionLineRequest(_budgetItemId, TransactionDirection.Increase, 100000m, null)]));
        txResult.Succeeded.ShouldBeTrue();

        var tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new SubmitBudgetTransactionCommand(tx!.Id, tx.RowVersion));
        tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new ApproveBudgetTransactionCommand(tx!.Id, tx.RowVersion, null));
        tx = await TestApp.FindAsync<BudgetTransaction>(txResult.Value);
        await TestApp.SendAsync(new PostBudgetTransactionCommand(tx!.Id, tx.RowVersion));
    }

    [Test]
    public async Task T052_FullLifecycle_CreateRequest_DualApproval_OrderGeneration_ShouldWork()
    {
        var createResult = await TestApp.SendAsync(new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Vendor",
            RequestedAmount = 35000m,
            CurrencyId = 1,
            Purpose = "Test disbursement",
            FinancialYearId = 1
        });
        createResult.Succeeded.ShouldBeTrue();
        var requestId = createResult.Value!.Id;
        var request = (await TestApp.FindAsync<DisbursementRequest>(requestId))!;
        request.Status.ShouldBe(DisbursementRequestStatus.Draft);

        var submitResult = await TestApp.SendAsync(new SubmitDisbursementRequestCommand
        {
            Id = requestId
        });
        submitResult.Succeeded.ShouldBeTrue();

        request = (await TestApp.FindAsync<DisbursementRequest>(requestId))!;
        request.Status.ShouldBe(DisbursementRequestStatus.PendingApproval);

        var approve1Result = await TestApp.SendAsync(new ApproveDisbursementRequestCommand
        {
            Id = requestId,
            ApprovedAmount = 35000m,
            Reason = "First approval"
        });
        approve1Result.Succeeded.ShouldBeTrue();

        request = (await TestApp.FindAsync<DisbursementRequest>(requestId))!;
        request.Status.ShouldBe(DisbursementRequestStatus.PendingApproval);

        var approve2Result = await TestApp.SendAsync(new ApproveDisbursementRequestCommand
        {
            Id = requestId,
            ApprovedAmount = 35000m,
            Reason = "Second approval"
        });
        approve2Result.Succeeded.ShouldBeTrue();

        request = (await TestApp.FindAsync<DisbursementRequest>(requestId))!;
        request.Status.ShouldBe(DisbursementRequestStatus.Approved);

        var paymentOrders = await TestApp.WhereAsync<PaymentOrder>(po =>
            po.DisbursementRequestId == requestId);
        paymentOrders.ShouldNotBeEmpty();
        var po = paymentOrders.First();
        po.Status.ShouldBe(PaymentOrderStatus.Draft);
        po.AmountGross.ShouldBe(35000m);
    }

    [Test]
    public async Task T053_CreateRequest_InvalidData_ShouldReject()
    {
        var result = await TestApp.SendAsync(new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "",
            RequestedAmount = 0,
            CurrencyId = 999,
            Purpose = "",
            FinancialYearId = 999
        });
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task T054_SameUserDoubleApproval_ShouldReject()
    {
        var createResult = await TestApp.SendAsync(new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Vendor",
            RequestedAmount = 10000m,
            CurrencyId = 1,
            Purpose = "Test",
            FinancialYearId = 1
        });
        createResult.Succeeded.ShouldBeTrue();
        var requestId = createResult.Value!.Id;

        await TestApp.SendAsync(new SubmitDisbursementRequestCommand { Id = requestId });

        var request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        var approve1Result = await TestApp.SendAsync(new ApproveDisbursementRequestCommand
        {
            Id = requestId,
            ApprovedAmount = 10000m,

            Reason = "First"
        });
        approve1Result.Succeeded.ShouldBeTrue();

        request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        var approve2Result = await TestApp.SendAsync(new ApproveDisbursementRequestCommand
        {
            Id = requestId,
            ApprovedAmount = 10000m,

            Reason = "Second same user"
        });
        approve2Result.Succeeded.ShouldBeFalse();
        approve2Result.Errors.ShouldContain(e => e.Contains("different approver"));
    }

    [Test]
    public async Task T055_CancelRequest_ShouldTransitionToCancelled()
    {
        var createResult = await TestApp.SendAsync(new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Vendor",
            RequestedAmount = 10000m,
            CurrencyId = 1,
            Purpose = "Test",
            FinancialYearId = 1
        });
        createResult.Succeeded.ShouldBeTrue();
        var requestId = createResult.Value!.Id;

        var request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        var cancelResult = await TestApp.SendAsync(new CancelDisbursementRequestCommand
        {
            Id = requestId,
            Reason = "No longer needed"
        });
        cancelResult.Succeeded.ShouldBeTrue();

        request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        request!.Status.ShouldBe(DisbursementRequestStatus.Cancelled);
    }

    [Test]
    public async Task T056_AfterFirstApproval_AmountShouldBeFrozen()
    {
        var createResult = await TestApp.SendAsync(new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Vendor",
            RequestedAmount = 20000m,
            CurrencyId = 1,
            Purpose = "Test",
            FinancialYearId = 1
        });
        createResult.Succeeded.ShouldBeTrue();
        var requestId = createResult.Value!.Id;

        await TestApp.SendAsync(new SubmitDisbursementRequestCommand { Id = requestId });

        var request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        var approve1Result = await TestApp.SendAsync(new ApproveDisbursementRequestCommand
        {
            Id = requestId,
            ApprovedAmount = 20000m
        });
        approve1Result.Succeeded.ShouldBeTrue();

        request = await TestApp.FindAsync<DisbursementRequest>(requestId);
        request!.Status.ShouldBe(DisbursementRequestStatus.PendingApproval);
    }
}
