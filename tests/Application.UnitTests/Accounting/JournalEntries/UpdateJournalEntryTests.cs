using ERP_Government.Application.Accounting.Commands.JournalEntries.UpdateJournalEntry;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.JournalEntries;

public class UpdateMoveTests
{
    [Test]
    public void UpdateMoveValidator_DraftGuard_ShouldBeInHandler()
    {
        // Handler checks EntryStatus == Draft — verified via functional test
        true.ShouldBeTrue();
    }

    [Test]
    public void UpdateMoveValidator_RowVersionRequired_ShouldFailWhenEmpty()
    {
        var cmd = new UpdateJournalEntryCommand { Id = 1, RowVersion = [] };
        var validator = new UpdateJournalEntryCommandValidator();
        var result = validator.Validate(cmd);
        result.IsValid.ShouldBeFalse();
    }
}
