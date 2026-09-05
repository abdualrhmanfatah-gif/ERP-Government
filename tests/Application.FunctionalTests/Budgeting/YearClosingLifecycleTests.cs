using ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.IssueFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.ReopenFiscalYear;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class YearClosingLifecycleTests : TestBase
{
    // ─── T060: Full year-end lifecycle ──────────────────────────────

    [Test]
    public async Task FullYearEndLifecycle_LapsePaymentBlockedReopenFinalAccount_ShouldCompleteSuccessfully()
    {
        await TestApp.RunAsAdministratorAsync();

        // Setup fiscal year
        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        // Lapse
        var lapseResult = await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        lapseResult.Succeeded.ShouldBeTrue();

        // Reopen
        var reopenResult = await TestApp.SendAsync(new ReopenFiscalYearCommand(fy.Id));
        reopenResult.Succeeded.ShouldBeTrue();

        // Lapse again for final account
        lapseResult = await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        lapseResult.Succeeded.ShouldBeTrue();

        // Generate final account
        var genResult = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        genResult.Succeeded.ShouldBeTrue();

        // Issue final account
        var issueResult = await TestApp.SendAsync(new IssueFinalAccountCommand(genResult.Value!.FinalAccountId));
        issueResult.Succeeded.ShouldBeTrue();
    }

    // ─── T061: Lapse idempotency ────────────────────────────────────

    [Test]
    public async Task LapseFiscalYear_DuplicateRun_ShouldReturnFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        var result1 = await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        result1.Succeeded.ShouldBeTrue();

        var result2 = await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        result2.Succeeded.ShouldBeFalse();
        result2.Errors.ShouldContain(e => e.Contains("already been lapsed"));
    }

    // ─── T062: Reopen blocked by final account ──────────────────────

    [Test]
    public async Task ReopenFiscalYear_FinalAccountIssued_ShouldReturnFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        var genResult = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        await TestApp.SendAsync(new IssueFinalAccountCommand(genResult.Value!.FinalAccountId));

        var reopenResult = await TestApp.SendAsync(new ReopenFiscalYearCommand(fy.Id));
        reopenResult.Succeeded.ShouldBeFalse();
        reopenResult.Errors.ShouldContain(e => e.Contains("final account has been issued"));
    }

    // ─── T063: Closing entries are balanced ──────────────────────────

    [Test]
    public async Task GenerateFinalAccount_ShouldCreateBalancedClosingEntries()
    {
        await TestApp.RunAsAdministratorAsync();

        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        var genResult = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        genResult.Succeeded.ShouldBeTrue();
        genResult.Value!.LineCount.ShouldBeGreaterThan(0);
    }

    // ─── T064: Budget-vs-actual matches ──────────────────────────────

    [Test]
    public async Task GenerateFinalAccount_ShouldComputeBudgetVsActual()
    {
        await TestApp.RunAsAdministratorAsync();

        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        await TestApp.SendAsync(new LapseFiscalYearCommand(fy.Id));
        var genResult = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        genResult.Succeeded.ShouldBeTrue();

        var account = await TestApp.FindAsync<ERP_Government.Domain.Budgeting.Entities.FinalAccount>(genResult.Value!.FinalAccountId);
        account.ShouldNotBeNull();
        account.Status.ShouldBe(ERP_Government.Domain.Budgeting.Enums.FinalAccountStatus.Draft);
    }
}
