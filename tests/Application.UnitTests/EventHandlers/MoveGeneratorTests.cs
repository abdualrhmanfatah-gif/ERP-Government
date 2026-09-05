using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Procurement;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

[TestFixture]
public class MoveGeneratorTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private async Task<(AccountingEvent accountingEvent, PostingRule postingRule)> SeedForTestAsync()
    {
        using var context = new ApplicationDbContext(_options);

        var accountingEvent = new AccountingEvent
        {
            EventType = EventType.PurchaseOrderApproved,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        };
        context.AccountingEvents.Add(accountingEvent);

        var postingRule = new PostingRule
        {
            Name = "Purchase Order Accrual",
            EventType = "PurchaseOrderApproved",
            JournalId = 5,
            Priority = 10,
            IsActive = true
        };
        context.PostingRules.Add(postingRule);

        await context.SaveChangesAsync();
        return (accountingEvent, postingRule);
    }

    [Test]
    public async Task GenerateMoveAsync_CreatesJournalEntryWithCorrectFields()
    {
        // Arrange
        var (accountingEvent, postingRule) = await SeedForTestAsync();
        using var context = new ApplicationDbContext(_options);
        var generator = new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        var journalEntry = await generator.GenerateJournalEntryAsync(accountingEvent, postingRule, sourceEvent, new DateOnly(2026, 8, 25));

        // Assert — entity is added to context (change tracker), not persisted
        journalEntry.ShouldNotBeNull();
        journalEntry.EntryNumber.ShouldStartWith("AUTO-2026-");
        journalEntry.EntryStatus.ShouldBe(EntryStatus.Draft);
        journalEntry.JournalId.ShouldBe(5);
        journalEntry.SourceEventId.ShouldBe(accountingEvent.Id);
        journalEntry.IsSystemGenerated.ShouldBeTrue();
    }

    [Test]
    public async Task GenerateMoveAsync_SetsCorrectNarration()
    {
        // Arrange
        var (accountingEvent, postingRule) = await SeedForTestAsync();
        using var context = new ApplicationDbContext(_options);
        var generator = new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        var journalEntry = await generator.GenerateJournalEntryAsync(accountingEvent, postingRule, sourceEvent, new DateOnly(2026, 8, 25));

        // Assert
        journalEntry.Narration.ShouldBe("Auto-generated from PurchaseOrderApproved event");
    }

    [Test]
    public async Task GenerateMoveAsync_GeneratesSequentialEntryNumbers()
    {
        // Arrange — use InMemory DB to test real sequential numbering
        var (accountingEvent, postingRule) = await SeedForTestAsync();
        using var context = new ApplicationDbContext(_options);
        var generator = new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act — generate first journalEntry (no SaveChanges, but InMemory tracks it)
        var journalEntry1 = await generator.GenerateJournalEntryAsync(accountingEvent, postingRule, sourceEvent, new DateOnly(2026, 8, 25));
        // Save to make the entry number query work for the second call
        context.JournalEntries.Add(journalEntry1);
        context.SaveChanges();

        // Act — generate second journalEntry
        var journalEntry2 = await generator.GenerateJournalEntryAsync(accountingEvent, postingRule, sourceEvent, new DateOnly(2026, 8, 25));

        // Assert
        journalEntry1.EntryNumber.ShouldNotBe(journalEntry2.EntryNumber);
    }

    [Test]
    public void GenerateMoveAsync_DoesNotCallSaveChangesAsync()
    {
        // Arrange — T019: regression test for nested SaveChanges prevention
        // This test verifies the contract at the code level:
        // JournalEntryGenerator must NOT contain SaveChangesAsync calls.
        // Since JournalEntryGenerator queries JournalEntries for entry number, we test via InMemory DB.
        var (accountingEvent, postingRule) = SeedForTestAsync().GetAwaiter().GetResult();
        using var context = new ApplicationDbContext(_options);
        var generator = new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        var journalEntry = generator.GenerateJournalEntryAsync(accountingEvent, postingRule, sourceEvent, new DateOnly(2026, 8, 25)).GetAwaiter().GetResult();

        // Assert — journalEntry created but NOT persisted (context not saved)
        journalEntry.ShouldNotBeNull();
        journalEntry.EntryNumber.ShouldStartWith("AUTO-2026-");

        // Verify the journalEntry is tracked but not saved
        var entry = context.Entry(journalEntry);
        entry.State.ShouldBe(EntityState.Added);
    }
}
