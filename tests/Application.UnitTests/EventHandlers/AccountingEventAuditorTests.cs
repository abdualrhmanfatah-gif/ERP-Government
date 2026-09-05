using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

/// <summary>
/// T030: AccountingEventAuditor unit tests.
/// Tests MarkProcessing, MarkCompleted, MarkFailed (with retry count), ResetForRetry.
/// Note: Auditor methods are now in-memory only (no SaveChangesAsync).
/// </summary>
[TestFixture]
public class AccountingEventAuditorTests
{
    private AccountingEventAuditor _auditor = null!;

    [SetUp]
    public void SetUp()
    {
        // Auditor no longer needs DbContext — all methods are pure in-memory mutations.
        // We pass null since the implementation no longer calls SaveChangesAsync.
        _auditor = new AccountingEventAuditor(null!);
    }

    [Test]
    public void MarkProcessingAsync_SetsStatusToProcessing()
    {
        // Arrange
        var accountingEvent = new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        };

        // Act
        var task = _auditor.MarkProcessingAsync(accountingEvent);
        task.Wait();

        // Assert
        accountingEvent.Status.ShouldBe(EventStatus.Posted);
    }

    [Test]
    public void MarkCompletedAsync_SetsStatusToCompletedAndProcessedAt()
    {
        // Arrange
        var accountingEvent = new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 0
        };

        // Act
        var task = _auditor.MarkCompletedAsync(accountingEvent);
        task.Wait();

        // Assert
        accountingEvent.Status.ShouldBe(EventStatus.Reversed);
        accountingEvent.ProcessedAt.ShouldNotBeNull();
    }

    [Test]
    public void MarkFailedAsync_SetsStatusToFailedAndIncrementsRetryCount()
    {
        // Arrange
        var accountingEvent = new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 1
        };

        // Act
        var task = _auditor.MarkFailedAsync(accountingEvent, "Test error");
        task.Wait();
        var canRetry = task.Result;

        // Assert
        accountingEvent.Status.ShouldBe(EventStatus.Posted);
        accountingEvent.ErrorMessage.ShouldBe("Test error");
        accountingEvent.RetryCount.ShouldBe(2);
        accountingEvent.ProcessedAt.ShouldNotBeNull();
        canRetry.ShouldBeTrue(); // RetryCount 2 < 3
    }

    [Test]
    public void MarkFailedAsync_ReturnsFalseWhenRetryCountExceeds3()
    {
        // Arrange
        var accountingEvent = new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 3
        };

        // Act
        var task = _auditor.MarkFailedAsync(accountingEvent, "Test error");
        task.Wait();
        var canRetry = task.Result;

        // Assert
        accountingEvent.RetryCount.ShouldBe(4);
        canRetry.ShouldBeFalse(); // RetryCount 4 >= 3
    }

    [Test]
    public void ResetForRetryAsync_ResetsStatusToPendingAndClearsFields()
    {
        // Arrange
        var accountingEvent = new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 3,
            ErrorMessage = "Test error",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        // Act
        var task = _auditor.ResetForRetryAsync(accountingEvent);
        task.Wait();

        // Assert
        accountingEvent.Status.ShouldBe(EventStatus.Pending);
        accountingEvent.RetryCount.ShouldBe(0);
        accountingEvent.ErrorMessage.ShouldBeNull();
        accountingEvent.ProcessedAt.ShouldBeNull();
    }

    [Test]
    public async Task ResetStaleProcessingEventsAsync_ResetsEventsOlderThan5Minutes()
    {
        // Arrange — T020: orphan recovery test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var auditor = new AccountingEventAuditor(context);

        // Add a stale Processing event (last modified > 5 minutes ago)
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 0,
            LastModified = DateTimeOffset.UtcNow.AddMinutes(-10)
        });

        // Add a recent Processing event (should NOT be reset)
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 2,
            EventType = EventType.PaymentExecution,
            SourceDocumentType = "PaymentOrder",
            SourceDocumentId = 10,
            Status = EventStatus.Posted,
            RetryCount = 0,
            LastModified = DateTimeOffset.UtcNow.AddMinutes(-2)
        });

        await context.SaveChangesAsync();

        // Act
        var resetCount = await auditor.ResetStaleProcessingEventsAsync();

        // Assert
        resetCount.ShouldBe(1);

        var staleEvent = await context.AccountingEvents.FindAsync(1);
        staleEvent.ShouldNotBeNull();
        staleEvent.Status.ShouldBe(EventStatus.Pending);

        var recentEvent = await context.AccountingEvents.FindAsync(2);
        recentEvent.ShouldNotBeNull();
        recentEvent.Status.ShouldBe(EventStatus.Posted);
    }
}
