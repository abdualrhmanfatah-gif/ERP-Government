using ERP_Government.Application.Banking.Commands.BankStatements;
using ERP_Government.Application.Banking.Commands.BankReconciliations;
using ERP_Government.Application.Banking.Queries.BankStatements;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Banking;

[TestFixture]
public class BankStatementsEndpointTests : TestBase
{
    [Test]
    public async Task GetBankStatements_ReturnsOk()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new GetBankStatementsQuery());

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GetBankStatementById_WhenNotExists_ReturnsNull()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new GetBankStatementByIdQuery { Id = 99999 });

        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateBankStatement_WhenInvalid_ReturnsFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var command = new CreateBankStatementCommand
        {
            Name = "Test Statement",
            BankAccountId = 99999,
            JournalId = 99999,
            StatementDate = DateOnly.FromDateTime(DateTime.Today),
            BalanceStart = 10000m,
            BalanceEnd = 15000m
        };

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task ReconcileBankStatement_WhenNotExists_ReturnsFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new ReconcileBankStatementCommand { Id = 99999 });

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task CancelBankStatement_WhenNotExists_ReturnsFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CancelBankStatementCommand { Id = 99999 });

        result.Succeeded.ShouldBeFalse();
    }
}

[TestFixture]
public class BankReconciliationsEndpointTests : TestBase
{
    [Test]
    public async Task GetBankReconciliations_ReturnsOk()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new Application.Banking.Queries.BankReconciliations.GetBankReconciliationsQuery());

        result.ShouldNotBeNull();
    }

    [Test]
    public async Task CompleteBankReconciliation_WhenNotExists_ReturnsFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CompleteBankReconciliationCommand { Id = 99999 });

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task RejectBankReconciliation_WhenNotExists_ReturnsFailure()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new RejectBankReconciliationCommand { Id = 99999, Reason = "Test rejection" });

        result.Succeeded.ShouldBeFalse();
    }
}
