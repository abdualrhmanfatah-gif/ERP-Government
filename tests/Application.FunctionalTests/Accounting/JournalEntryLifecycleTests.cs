using ERP_Government.Application.Accounting.Commands.JournalEntries.ApproveJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.CancelJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.CreateJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.PostJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.ReverseJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.SubmitJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.UpdateJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntryLines.CreateJournalEntryLine;
using ERP_Government.Application.Accounting.Queries.JournalEntries.GetJournalEntryById;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Accounting;

/// <summary>
/// Functional tests walking full lifecycle per quickstart.md scenarios 1-4.
/// Requires TestApp WebApiFactory — uses TestBase for integration testing.
/// </summary>
[TestFixture]
public class JournalEntryLifecycleTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task T080_Create_ShouldReturnEntryNumber()
    {
        var result = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
            Narration = "Test lifecycle entry",
        });

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);

        var entry = await TestApp.SendAsync(new GetJournalEntryByIdQuery { Id = result.Value });
        entry.ShouldNotBeNull();
        entry.EntryNumber.ShouldStartWith("JRN-");
        entry.EntryStatus.ShouldBe("Draft");
        entry.Narration.ShouldBe("Test lifecycle entry");
    }

    [Test]
    public async Task T080_Submit_ShouldTransitionDraftToSubmitted()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        // Add a line so submit validation passes
        await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 1,
            CurrencyId = 1,
            Debit = 100,
            Credit = 0,
        });

        var submitResult = await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });
        submitResult.Succeeded.ShouldBeTrue();

        var entry = await TestApp.SendAsync(new GetJournalEntryByIdQuery { Id = createResult.Value });
        entry.ShouldNotBeNull();
        entry.EntryStatus.ShouldBe("Submitted");
    }

    [Test]
    public async Task T080_Submit_NonDraft_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 1,
            CurrencyId = 1,
            Debit = 100,
            Credit = 0,
        });

        await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });

        // Second submit should fail
        var result = await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only draft"));
    }

    [Test]
    public async Task T080_Cancel_Draft_ShouldTransitionToCancelled()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        var cancelResult = await TestApp.SendAsync(new CancelJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });
        cancelResult.Succeeded.ShouldBeTrue();

        var entry = await TestApp.SendAsync(new GetJournalEntryByIdQuery { Id = createResult.Value });
        entry.ShouldNotBeNull();
        entry.EntryStatus.ShouldBe("Cancelled");
    }

    [Test]
    public async Task T081_Approve_NonSubmitted_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        // Approve without submit should fail
        var result = await TestApp.SendAsync(new ApproveJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only submitted"));
    }

    [Test]
    public async Task T081_Cancel_Approved_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 1,
            CurrencyId = 1,
            Debit = 100,
            Credit = 0,
        });

        await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });

        await TestApp.SendAsync(new ApproveJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });

        // Cancel on approved should fail — approved must reverse, never cancel
        var result = await TestApp.SendAsync(new CancelJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only draft or submitted"));
    }

    [Test]
    public async Task T082_Reverse_NonPosted_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        // Reverse without posting should fail
        var result = await TestApp.SendAsync(new ReverseJournalEntryCommand
        {
            Id = createResult.Value,
            ReversalReason = "Test",
            RowVersion = [],
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only posted"));
    }

    [Test]
    public async Task T082_Reverse_ReasonEmpty_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        var result = await TestApp.SendAsync(new ReverseJournalEntryCommand
        {
            Id = createResult.Value,
            ReversalReason = "",
            RowVersion = [],
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Reversal reason is required"));
    }

    [Test]
    public async Task T083_UpdateLines_NonDraft_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
        });
        createResult.Succeeded.ShouldBeTrue();

        await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 1,
            CurrencyId = 1,
            Debit = 100,
            Credit = 0,
        });

        // Submit first
        await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });

        // Try adding line to submitted entry — should fail
        var result = await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 2,
            CurrencyId = 1,
            Debit = 50,
            Credit = 0,
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only draft"));
    }

    [Test]
    public async Task T083_EditHeader_NonDraft_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateJournalEntryCommand
        {
            DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodId = 1,
            FiscalYearId = 1,
            Narration = "Original",
        });
        createResult.Succeeded.ShouldBeTrue();

        await TestApp.SendAsync(new CreateJournalEntryLineCommand
        {
            JournalEntryId = createResult.Value,
            AccountId = 1,
            CurrencyId = 1,
            Debit = 100,
            Credit = 0,
        });

        await TestApp.SendAsync(new SubmitJournalEntryCommand
        {
            Id = createResult.Value,
            RowVersion = [],
        });

        // Try updating header on submitted entry — should fail
        var result = await TestApp.SendAsync(new UpdateJournalEntryCommand
        {
            Id = createResult.Value,
            Narration = "Modified",
        });
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only draft"));
    }
}
