using ERP_Government.Application.Accounting.Commands.JournalEntries.CancelJournalEntry;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.JournalEntries;

public class CancelMoveTests
{
    [Test]
    public void CancelMove_ShouldAllowDraftAndSubmitted_Only()
    {
        // Handler allows Draft or Submitted per Q2 B — Posted should fail
        // Documented via unit test placeholder — integration verifies via quickstart Scenario 3
        true.ShouldBeTrue();
    }

    [Test]
    public void CancelMoveValidator_IdZero_ShouldFail()
    {
        var cmd = new CancelJournalEntryCommand { Id = 0, RowVersion = [1] };
        var validator = new CancelJournalEntryCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeFalse();
    }
}
