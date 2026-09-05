using ERP_Government.Application.Accounting.Commands.JournalEntryLines.CreateJournalEntryLine;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.JournalEntryLines;

public class JournalEntryLineTests
{
    [Test]
    public void CreateMoveLineValidator_ExchangeRateZero_ShouldFail()
    {
        var cmd = new CreateJournalEntryLineCommand { JournalEntryId = 1, AccountId = 1, CurrencyId = 1, ExchangeRate = 0, Debit = 100, Credit = 0 };
        var validator = new CreateJournalEntryLineCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void CreateMoveLineValidator_XorViolation_ShouldBeHandledByHandler()
    {
        // Handler checks debit/credit XOR — validator allows non-negative but handler rejects both >0
        var cmd = new CreateJournalEntryLineCommand { JournalEntryId = 1, AccountId = 1, CurrencyId = 1, ExchangeRate = 1, Debit = 100, Credit = 100 };
        var validator = new CreateJournalEntryLineCommandValidator();
        // Validator only checks >=0, so this passes validator but fails handler — correct
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeTrue();
    }
}
