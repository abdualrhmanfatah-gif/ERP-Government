using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Procurement;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

/// <summary>
/// T029: PostingPipelineHandler unit tests.
/// Single context per test — Auditor mutations are in-memory (no SaveChanges).
/// Assertions check the change-tracked state (same as what OutboxProcessorService would persist).
/// </summary>
[TestFixture]
public class PostingPipelineHandlerTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task Handle_WithMatchingRule_GeneratesJournalEntryAndMarksCompleted()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 1,
            EventType = EventType.PurchaseOrderApproved,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        });
        context.PostingRules.Add(new PostingRule
        {
            Name = "Purchase Order Accrual",
            EventType = "PurchaseOrderApproved",
            JournalId = 5,
            Priority = 10,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new PostingPipelineHandler(
            context,
            new PostingRuleMatcher(context),
            new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>()),
            new AccountingEventAuditor(context),
            new RetryPolicy(),
            new LoggerFactory().CreateLogger<PostingPipelineHandler>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        await handler.Handle(sourceEvent, CancellationToken.None);

        // Assert — check change tracker for in-memory state
        var accountingEvent = context.AccountingEvents.Local.First(e => e.Id == 1);
        accountingEvent.Status.ShouldBe(EventStatus.Reversed);

        var journalEntries = context.JournalEntries.Local.Where(m => m.SourceEventId == 1).ToList();
        journalEntries.Count.ShouldBe(1);
        journalEntries[0].JournalId.ShouldBe(5);
    }

    [Test]
    public async Task Handle_WithNoMatchingRule_MarksFailed()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 2,
            EventType = EventType.PurchaseOrderApproved,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        });
        await context.SaveChangesAsync();

        var handler = new PostingPipelineHandler(
            context,
            new PostingRuleMatcher(context),
            new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>()),
            new AccountingEventAuditor(context),
            new RetryPolicy(),
            new LoggerFactory().CreateLogger<PostingPipelineHandler>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        await handler.Handle(sourceEvent, CancellationToken.None);

        // Assert
        var accountingEvent = context.AccountingEvents.Local.First(e => e.Id == 2);
        accountingEvent.Status.ShouldBe(EventStatus.Posted);
        accountingEvent.ErrorMessage.ShouldNotBeNull();
        accountingEvent.ErrorMessage.ShouldContain("No posting rule found");
    }

    [Test]
    public async Task Handle_MultipleRules_GeneratesMultipleJournalEntries()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 3,
            EventType = EventType.PurchaseOrderApproved,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        });
        context.PostingRules.Add(new PostingRule
        {
            Name = "Rule 1",
            EventType = "PurchaseOrderApproved",
            JournalId = 1,
            Priority = 10,
            IsActive = true
        });
        context.PostingRules.Add(new PostingRule
        {
            Name = "Rule 2",
            EventType = "PurchaseOrderApproved",
            JournalId = 2,
            Priority = 20,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new PostingPipelineHandler(
            context,
            new PostingRuleMatcher(context),
            new JournalEntryGenerator(context, new LoggerFactory().CreateLogger<JournalEntryGenerator>()),
            new AccountingEventAuditor(context),
            new RetryPolicy(),
            new LoggerFactory().CreateLogger<PostingPipelineHandler>());

        var sourceEvent = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        await handler.Handle(sourceEvent, CancellationToken.None);

        // Assert
        var accountingEvent = context.AccountingEvents.Local.First(e => e.Id == 3);
        accountingEvent.Status.ShouldBe(EventStatus.Reversed);

        var journalEntries = context.JournalEntries.Local.Where(m => m.SourceEventId == 3).ToList();
        journalEntries.Count.ShouldBe(2);
    }
}
