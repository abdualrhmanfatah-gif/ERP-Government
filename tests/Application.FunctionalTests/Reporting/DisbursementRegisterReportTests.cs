using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class DisbursementRegisterReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task GetDisbursementRegister_ShouldReturnLines()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
    }

    [Test]
    public async Task GetDisbursementRegister_TotalRequests_ShouldMatchLineCount()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalRequests.ShouldBe(result.Lines.Count);
    }

    [Test]
    public async Task GetDisbursementRegister_StatusCounts_ShouldSumToTotal()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        var statusSum = result.Totals.DraftCount
            + result.Totals.SubmittedCount
            + result.Totals.ApprovedCount
            + result.Totals.PaidCount
            + result.Totals.RejectedCount;

        statusSum.ShouldBe(result.Totals.TotalRequests,
            "Status counts should sum to total requests");
    }

    [Test]
    public async Task GetDisbursementRegister_FilterByStatus_ShouldReturnFilteredResults()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1, Status = "Paid" };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.Status.ShouldBe("Paid");
        }
    }

    [Test]
    public async Task GetDisbursementRegister_FilterByFund_ShouldReturnFilteredResults()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1, FundId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.FundId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetDisbursementRegisterDetail_ShouldReturnPaymentDetails()
    {
        var query = new GetDisbursementRegisterDetailQuery { PaymentOrderId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Payments.ShouldNotBeNull();
    }

    [Test]
    public async Task GetDisbursementRegister_PaidAmount_ShouldSumPaidLines()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        var expectedPaidAmount = result.Lines
            .Where(l => l.Status == "Paid")
            .Sum(l => l.Amount);

        result.Totals.PaidAmount.ShouldBe(expectedPaidAmount,
            "PaidAmount should equal sum of paid lines");
    }

    [Test]
    public async Task GetDisbursementRegister_TotalAmount_ShouldSumAllLines()
    {
        var query = new GetDisbursementRegisterQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalAmount.ShouldBe(result.Lines.Sum(l => l.Amount),
            "TotalAmount should equal sum of all line amounts");
    }
}
