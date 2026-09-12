using ERP_Government.Application.Accounting.Commands.RecurringEntries.CancelRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.CreateRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.PauseRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.ResumeRecurringEntry;
using ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntryById;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Accounting;

[TestFixture]
public class RecurringEntryLifecycleTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        // Seed a journal
        await TestApp.AddAsync(new Journal
        {
            Id = 1,
            Name = "General Journal",
            IsActive = true
        });
    }

    [Test]
    public async Task T032_Create_ShouldReturnEntryNumberStartingWithREC()
    {
        var result = await TestApp.SendAsync(new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Monthly Rent",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            Amount = 5000m
        });

        result.Succeeded.ShouldBeTrue();

        // Query the created entry
        var entries = await TestApp.FindAsync<RecurringEntry>(1);
        entries.ShouldNotBeNull();
        entries.EntryNumber.ShouldStartWith("REC-");
        entries.Status.ShouldBe(RecurringEntryStatus.Active);
        entries.Name.ShouldBe("Monthly Rent");
    }

    [Test]
    public async Task T033_Create_WithoutTemplateAndWithoutAmount_ShouldFail()
    {
        var result = await TestApp.SendAsync(new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "No Amount Entry",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1)
        });

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task T034_PauseResumeCancel_ShouldLogDocumentStatus()
    {
        // Create
        var createResult = await TestApp.SendAsync(new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Audit Trail Test",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            Amount = 1000m
        });
        createResult.Succeeded.ShouldBeTrue();

        var entry = await TestApp.FindAsync<RecurringEntry>(1);
        entry.ShouldNotBeNull();
        var entryId = entry.Id;

        // Pause
        var pauseResult = await TestApp.SendAsync(new PauseRecurringEntryCommand
        {
            Id = entryId,
            Reason = "Budget review",
            RowVersion = entry.RowVersion
        });
        pauseResult.Succeeded.ShouldBeTrue();

        var paused = await TestApp.SendAsync(new GetRecurringEntryByIdQuery { Id = entryId });
        paused.ShouldNotBeNull();
        paused.Status.ShouldBe(RecurringEntryStatus.Paused.ToString());

        // Resume
        var resumeEntry = await TestApp.FindAsync<RecurringEntry>(entryId);
        resumeEntry.ShouldNotBeNull();
        var resumeResult = await TestApp.SendAsync(new ResumeRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = resumeEntry.RowVersion
        });
        resumeResult.Succeeded.ShouldBeTrue();

        var resumed = await TestApp.SendAsync(new GetRecurringEntryByIdQuery { Id = entryId });
        resumed.ShouldNotBeNull();
        resumed.Status.ShouldBe(RecurringEntryStatus.Active.ToString());

        // Cancel
        var cancelEntry = await TestApp.FindAsync<RecurringEntry>(entryId);
        cancelEntry.ShouldNotBeNull();
        var cancelResult = await TestApp.SendAsync(new CancelRecurringEntryCommand
        {
            Id = entryId,
            Reason = "No longer needed",
            RowVersion = cancelEntry.RowVersion
        });
        cancelResult.Succeeded.ShouldBeTrue();

        var cancelled = await TestApp.SendAsync(new GetRecurringEntryByIdQuery { Id = entryId });
        cancelled.ShouldNotBeNull();
        cancelled.Status.ShouldBe(RecurringEntryStatus.Cancelled.ToString());
    }

    [Test]
    public async Task T035_InvalidTransitions_ShouldBeRejected()
    {
        // Create
        var createResult = await TestApp.SendAsync(new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Invalid Transition Test",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            Amount = 1000m
        });
        createResult.Succeeded.ShouldBeTrue();

        var entry = await TestApp.FindAsync<RecurringEntry>(1);
        entry.ShouldNotBeNull();
        var entryId = entry.Id;

        // Pause on Active (should succeed)
        var pauseResult = await TestApp.SendAsync(new PauseRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry.RowVersion
        });
        pauseResult.Succeeded.ShouldBeTrue();

        // Pause again on Paused (should fail)
        entry = await TestApp.FindAsync<RecurringEntry>(entryId);
        var pauseAgainResult = await TestApp.SendAsync(new PauseRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry!.RowVersion
        });
        pauseAgainResult.Succeeded.ShouldBeFalse();

        // Resume on Paused (should succeed)
        entry = await TestApp.FindAsync<RecurringEntry>(entryId);
        var resumeResult = await TestApp.SendAsync(new ResumeRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry!.RowVersion
        });
        resumeResult.Succeeded.ShouldBeTrue();

        // Resume on Active (should fail)
        entry = await TestApp.FindAsync<RecurringEntry>(entryId);
        var resumeAgainResult = await TestApp.SendAsync(new ResumeRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry!.RowVersion
        });
        resumeAgainResult.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task T036_CancelledEntry_ShouldNotAllowResume()
    {
        // Create
        var createResult = await TestApp.SendAsync(new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Cancelled No Resume",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            Amount = 1000m
        });
        createResult.Succeeded.ShouldBeTrue();

        var entry = await TestApp.FindAsync<RecurringEntry>(1);
        entry.ShouldNotBeNull();
        var entryId = entry.Id;

        // Cancel
        var cancelResult = await TestApp.SendAsync(new CancelRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry.RowVersion
        });
        cancelResult.Succeeded.ShouldBeTrue();

        // Resume on Cancelled (should fail)
        entry = await TestApp.FindAsync<RecurringEntry>(entryId);
        var resumeResult = await TestApp.SendAsync(new ResumeRecurringEntryCommand
        {
            Id = entryId,
            RowVersion = entry!.RowVersion
        });
        resumeResult.Succeeded.ShouldBeFalse();
    }
}
