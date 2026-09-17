using ERP_Government.Infrastructure.Data.Seeds;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class JournalEntrySeedTests
{
    [Test]
    public void OpeningBalance_IsBalanced()
    {
        var bp = JournalEntrySeedData.OpeningBalance2026;

        var totalDebit = bp.Lines.Sum(l => l.Debit);
        var totalCredit = bp.Lines.Sum(l => l.Credit);

        totalDebit.ShouldBe(totalCredit,
            $"Debits ({totalDebit:N0}) must equal Credits ({totalCredit:N0})");
    }

    [Test]
    public void OpeningBalance_AllAccountCodesExist()
    {
        var bp = JournalEntrySeedData.OpeningBalance2026;
        var accountCodes = AccountSeedData.GetBlueprints().Select(a => a.Code).ToHashSet();

        foreach (var line in bp.Lines)
        {
            accountCodes.ShouldContain(line.AccountCode,
                $"Account code '{line.AccountCode}' does not exist in chart of accounts");
        }
    }

    [Test]
    public void OpeningBalance_AllAccountsArePostable()
    {
        var bp = JournalEntrySeedData.OpeningBalance2026;
        var accounts = AccountSeedData.GetBlueprints();

        foreach (var line in bp.Lines)
        {
            var account = accounts.Single(a => a.Code == line.AccountCode);
            account.IsPostable.ShouldBeTrue(
                $"Account '{line.AccountCode}' ({account.Name}) must be postable");
        }
    }

    [Test]
    public void OpeningBalance_EachLineHasExactlyOneAmount()
    {
        var bp = JournalEntrySeedData.OpeningBalance2026;

        foreach (var line in bp.Lines)
        {
            var hasDebit = line.Debit > 0;
            var hasCredit = line.Credit > 0;
            (hasDebit ^ hasCredit).ShouldBeTrue(
                $"Line '{line.AccountCode}': exactly one of Debit/Credit must be > 0");
        }
    }

    [Test]
    public void OpeningBalance_DocumentDateIsFirstDayOfYear()
    {
        var bp = JournalEntrySeedData.OpeningBalance2026;
        bp.DocumentDate.ShouldBe(new DateOnly(bp.FiscalYearNumber, 1, 1));
    }
}
