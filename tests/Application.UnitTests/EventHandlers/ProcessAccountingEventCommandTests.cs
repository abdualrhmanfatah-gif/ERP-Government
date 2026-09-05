using ERP_Government.Application.Accounting.Commands.AccountingEvents.ProcessEvent;
using ERP_Government.Application.Accounting.Commands.AccountingEvents.RetryEvent;
using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

/// <summary>
/// T033: Manual retry tests.
/// Single context per test — matches real transaction scope.
/// </summary>
[TestFixture]
public class ProcessAccountingEventCommandTests
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
    public async Task Handle_FailedEvent_ResetsToPending()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 1,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 3,
            ErrorMessage = "Test error"
        });
        await context.SaveChangesAsync();

        var command = new ProcessAccountingEventCommand { EventId = 1 };

        // Act
        var handler = new ProcessAccountingEventCommandHandler(
            context,
            new AccountingEventAuditor(context));
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        var accountingEvent = await context.AccountingEvents.FindAsync(1);
        accountingEvent.ShouldNotBeNull();
        accountingEvent.Status.ShouldBe(EventStatus.Pending);
        accountingEvent.RetryCount.ShouldBe(0);
        accountingEvent.ErrorMessage.ShouldBeNull();
    }

    [Test]
    public async Task Handle_NonFailedEvent_ReturnsFailure()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 2,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        });
        await context.SaveChangesAsync();

        var command = new ProcessAccountingEventCommand { EventId = 2 };

        // Act & Assert
        var handler = new ProcessAccountingEventCommandHandler(
            context,
            new AccountingEventAuditor(context));
        var result = await handler.Handle(command, CancellationToken.None);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("failed"));
    }

    [Test]
    public async Task Handle_EventNotFound_ReturnsFailure()
    {
        using var context = new ApplicationDbContext(_options);
        var command = new ProcessAccountingEventCommand { EventId = 999 };

        // Act & Assert
        var handler = new ProcessAccountingEventCommandHandler(
            context,
            new AccountingEventAuditor(context));
        var result = await handler.Handle(command, CancellationToken.None);
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task RetryCommand_FailedEventWithRetryCountLessThan3_ReturnsFailure()
    {
        // Arrange — T019: RetryAccountingEventCommand rejects RetryCount < 3
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 3,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 2,
            ErrorMessage = "Test error"
        });
        await context.SaveChangesAsync();

        var command = new RetryAccountingEventCommand { EventId = 3 };

        // Act
        var handler = new RetryAccountingEventCommandHandler(
            context,
            new AccountingEventAuditor(context));
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Automatic retry is still available"));
    }

    [Test]
    public async Task RetryCommand_FailedEventWithRetryCountAtLeast3_ResetsToPending()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        context.AccountingEvents.Add(new AccountingEvent
        {
            Id = 4,
            EventType = EventType.Other,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Posted,
            RetryCount = 3,
            ErrorMessage = "Test error"
        });
        await context.SaveChangesAsync();

        var command = new RetryAccountingEventCommand { EventId = 4 };

        // Act
        var handler = new RetryAccountingEventCommandHandler(
            context,
            new AccountingEventAuditor(context));
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        var accountingEvent = await context.AccountingEvents.FindAsync(4);
        accountingEvent.ShouldNotBeNull();
        accountingEvent.Status.ShouldBe(EventStatus.Pending);
        accountingEvent.RetryCount.ShouldBe(0);
    }
}
