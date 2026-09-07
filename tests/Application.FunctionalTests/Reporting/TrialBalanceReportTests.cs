using ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;
using ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class TrialBalanceReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task GetTrialBalanceReport_ShouldReturnLines()
    {
        var query = new GetTrialBalanceReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
    }

    [Test]
    public async Task GetTrialBalanceReport_ShouldBalance()
    {
        var query = new GetTrialBalanceReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalDebits.ShouldBe(result.Totals.TotalCredits,
            "Trial balance: Total debits must equal total credits");
    }

    [Test]
    public async Task GetTrialBalanceReport_Totals_ShouldMatchLineSums()
    {
        var query = new GetTrialBalanceReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalDebits.ShouldBe(result.Lines.Sum(l => l.DebitTotal));
        result.Totals.TotalCredits.ShouldBe(result.Lines.Sum(l => l.CreditTotal));
        result.Totals.TotalOpeningBalance.ShouldBe(result.Lines.Sum(l => l.OpeningBalance));
        result.Totals.TotalClosingBalance.ShouldBe(result.Lines.Sum(l => l.ClosingBalance));
    }

    [Test]
    public async Task GetTrialBalanceReport_ClosingBalance_ShouldCalculateCorrectly()
    {
        var query = new GetTrialBalanceReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Lines)
        {
            var expectedClosing = line.OpeningBalance + line.DebitTotal - line.CreditTotal;
            line.ClosingBalance.ShouldBe(expectedClosing,
                $"Account {line.AccountCode}: Closing should equal Opening + Debit - Credit");
        }
    }

    // TRACKED DEBT (TRE-01 session 2026-09-07): disabled — FundId/ProjectId filters were never
    // implemented on GetTrialBalanceReportQuery (ledger carries no fund/project dimensions).
    // Re-enable when the reporting-dimensions spec lands.
    // [Test]
    // public async Task GetTrialBalanceReport_FilterByFund_ShouldReturnFilteredResults()
    // {
    //     var query = new GetTrialBalanceReportQuery { FiscalYearId = 1, FundId = 1 };
    //     var result = await TestApp.SendAsync(query);
    //     result.ShouldNotBeNull();
    // }

    // [Test]
    // public async Task GetTrialBalanceReport_FilterByProject_ShouldReturnFilteredResults()
    // {
    //     var query = new GetTrialBalanceReportQuery { FiscalYearId = 1, ProjectId = 1 };
    //     var result = await TestApp.SendAsync(query);
    //     result.ShouldNotBeNull();
    // }

    [Test]
    public async Task GetLedgerMovement_ShouldReturnEntries()
    {
        var query = new GetLedgerMovementQuery { AccountId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeNull();
    }

    [Test]
    public async Task GetLedgerMovement_RunningBalance_ShouldBeCumulative()
    {
        var query = new GetLedgerMovementQuery { AccountId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        decimal runningBalance = 0;
        foreach (var entry in result.Entries)
        {
            runningBalance += entry.Debit - entry.Credit;
            entry.RunningBalance.ShouldBe(runningBalance,
                $"Entry {entry.EntryNumber}: Running balance should be cumulative");
        }
    }

    [Test]
    public async Task GetLedgerMovement_Totals_ShouldMatchEntrySums()
    {
        var query = new GetLedgerMovementQuery { AccountId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.TotalDebits.ShouldBe(result.Entries.Sum(e => e.Debit));
        result.Totals.TotalCredits.ShouldBe(result.Entries.Sum(e => e.Credit));
    }

    [Test]
    public async Task GetLedgerMovement_FinalBalance_ShouldMatchLastRunningBalance()
    {
        var query = new GetLedgerMovementQuery { AccountId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        if (result.Entries.Count > 0)
        {
            result.Totals.FinalBalance.ShouldBe(result.Entries.Last().RunningBalance,
                "Final balance should match last entry's running balance");
        }
    }
}
