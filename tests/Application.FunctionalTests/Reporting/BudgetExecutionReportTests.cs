using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;
using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class BudgetExecutionReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task GetBudgetExecutionReport_ShouldReturnLines()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
    }

    [Test]
    public async Task GetBudgetExecutionReport_Reconciliation_ShouldBalance()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Lines)
        {
            line.AvailableAmount.ShouldBe(
                line.AppropriatedAmount - line.EncumberedAmount - line.PaidAmount,
                $"Budget line {line.ItemCode}: Available should equal Appropriated - Encumbered - Paid");
        }
    }

    [Test]
    public async Task GetBudgetExecutionReport_Totals_ShouldMatchLineSums()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.AppropriatedAmount.ShouldBe(result.Lines.Sum(l => l.AppropriatedAmount));
        result.Totals.EncumberedAmount.ShouldBe(result.Lines.Sum(l => l.EncumberedAmount));
        result.Totals.PaidAmount.ShouldBe(result.Lines.Sum(l => l.PaidAmount));
        result.Totals.AvailableAmount.ShouldBe(result.Lines.Sum(l => l.AvailableAmount));
    }

    [Test]
    public async Task GetBudgetExecutionReport_FilterByFund_ShouldReturnFilteredResults()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1, FundId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.FundId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetBudgetExecutionReport_FilterByBudgetItem_ShouldReturnFilteredResults()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.BudgetItemId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetBudgetExecutionDetail_ShouldReturnEncumbrancesAndPayments()
    {
        var query = new GetBudgetExecutionDetailQuery { BudgetItemId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Encumbrances.ShouldNotBeNull();
        result.Payments.ShouldNotBeNull();
    }

    [Test]
    public async Task GetBudgetExecutionDetail_EncumbranceSum_ShouldMatchReport()
    {
        var reportQuery = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var report = await TestApp.SendAsync(reportQuery);

        if (report.Lines.Count > 0)
        {
            var firstLine = report.Lines.First();
            var detailQuery = new GetBudgetExecutionDetailQuery
            {
                BudgetItemId = firstLine.BudgetItemId,
                FiscalYearId = 1
            };
            var detail = await TestApp.SendAsync(detailQuery);

            detail.Encumbrances.Sum(e => e.Amount).ShouldBe(firstLine.EncumberedAmount,
                "Detail encumbrance sum should match report line");
        }
    }

    [Test]
    public async Task GetBudgetExecutionDetail_PaymentSum_ShouldMatchReport()
    {
        var reportQuery = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var report = await TestApp.SendAsync(reportQuery);

        if (report.Lines.Count > 0)
        {
            var firstLine = report.Lines.First();
            var detailQuery = new GetBudgetExecutionDetailQuery
            {
                BudgetItemId = firstLine.BudgetItemId,
                FiscalYearId = 1
            };
            var detail = await TestApp.SendAsync(detailQuery);

            detail.Payments.Sum(p => p.Amount).ShouldBe(firstLine.PaidAmount,
                "Detail payment sum should match report line");
        }
    }
}
