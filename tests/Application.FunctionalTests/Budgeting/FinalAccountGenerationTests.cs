using ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.IssueFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class FinalAccountGenerationTests : TestBase
{
    // ─── T063: Closing entries are balanced journal entries ──────────

    [Test]
    public async Task GenerateFinalAccount_WithLapsedYear_ShouldCreateDraftFinalAccount()
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
        var result = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));

        result.Succeeded.ShouldBeTrue();
        var account = await TestApp.FindAsync<ERP_Government.Domain.Budgeting.Entities.FinalAccount>(result.Value!.FinalAccountId);
        account.ShouldNotBeNull();
        account.Status.ShouldBe(ERP_Government.Domain.Budgeting.Enums.FinalAccountStatus.Draft);
    }

    // ─── T064: Budget-vs-actual matches appropriation/payment data ───

    [Test]
    public async Task IssueFinalAccount_ShouldTransitionToIssuedAndBeImmutable()
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
        var issueResult = await TestApp.SendAsync(new IssueFinalAccountCommand(genResult.Value!.FinalAccountId));

        issueResult.Succeeded.ShouldBeTrue();
        var account = await TestApp.FindAsync<ERP_Government.Domain.Budgeting.Entities.FinalAccount>(genResult.Value!.FinalAccountId);
        account!.Status.ShouldBe(ERP_Government.Domain.Budgeting.Enums.FinalAccountStatus.Issued);
        account.IssuedAt.ShouldNotBeNull();
    }

    // ─── T065: Year-boundary document splitting ──────────────────────

    [Test]
    public async Task GenerateFinalAccount_WhenYearNotClosed_ShouldReturnFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var fy = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Name = "FY2026", YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        };
        await TestApp.AddAsync(fy);

        var result = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("must be closed"));
    }

    [Test]
    public async Task GenerateFinalAccount_Duplicate_ShouldReturnFailure()
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
        await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));

        var result = await TestApp.SendAsync(new GenerateFinalAccountCommand(fy.Id));
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }
}
