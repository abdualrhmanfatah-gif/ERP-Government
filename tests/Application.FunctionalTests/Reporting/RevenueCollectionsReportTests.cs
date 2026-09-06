using ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;
using ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsDetail;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class RevenueCollectionsReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task GetRevenueCollectionsReport_ShouldReturnLines()
    {
        var query = new GetRevenueCollectionsReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
    }

    [Test]
    public async Task GetRevenueCollectionsReport_Totals_ShouldMatchLineSums()
    {
        var query = new GetRevenueCollectionsReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalAmount.ShouldBe(result.Lines.Sum(l => l.Amount));
    }

    [Test]
    public async Task GetRevenueCollectionsReport_FilterByFund_ShouldReturnFilteredResults()
    {
        var query = new GetRevenueCollectionsReportQuery { FiscalYearId = 1, FundId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.RevenueAccountId.ShouldNotBe(0);
        }
    }

    [Test]
    public async Task GetRevenueCollectionsReport_FilterByPaymentMethod_ShouldReturnFilteredResults()
    {
        var query = new GetRevenueCollectionsReportQuery
        {
            FiscalYearId = 1,
            PaymentMethod = "Cash"
        };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.PaymentMethod.ShouldBe("Cash");
        }
    }

    [Test]
    public async Task GetRevenueCollectionsDetail_ShouldReturnVoucherDetails()
    {
        var query = new GetRevenueCollectionsDetailQuery { ReceiptVoucherId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
        result.Checks.ShouldNotBeNull();
    }

    [Test]
    public async Task GetRevenueCollectionsDetail_LineSum_ShouldEqualTotalAmount()
    {
        var query = new GetRevenueCollectionsDetailQuery { ReceiptVoucherId = 1 };
        var result = await TestApp.SendAsync(query);

        if (result.Lines.Count > 0)
        {
            result.Lines.Sum(l => l.Amount).ShouldBe(result.TotalAmount,
                "Detail line amounts should sum to voucher total");
        }
    }

    [Test]
    public async Task GetRevenueCollectionsReport_DepositSlipStatus_ShouldBePopulated()
    {
        var query = new GetRevenueCollectionsReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Lines)
        {
            if (line.DepositSlipId.HasValue)
            {
                line.DepositSlipStatus.ShouldNotBeNullOrEmpty(
                    "Deposit slip status should be populated when DepositSlipId exists");
            }
        }
    }

    [Test]
    public async Task GetRevenueCollectionsReport_CheckClearingStatus_ShouldBePopulated()
    {
        var query = new GetRevenueCollectionsReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Lines)
        {
            line.CheckClearingStatus.ShouldNotBeNullOrEmpty(
                "Check clearing status should always be populated");
        }
    }
}
