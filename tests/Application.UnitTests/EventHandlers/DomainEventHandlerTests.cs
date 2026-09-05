using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.FinancialSettings;
using ERP_Government.Domain.Events.Procurement;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

[TestFixture]
public class DomainEventHandlerTests
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
    public async Task Handle_WithValidEvent_CreatesAccountingEvent()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var handler = new DomainEventHandler(context, new LoggerFactory().CreateLogger<DomainEventHandler>());

        var @event = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert — entity is added to context (change tracker), not persisted
        var accountingEvent = context.AccountingEvents.Local.FirstOrDefault();
        accountingEvent.ShouldNotBeNull();
        accountingEvent.EventType.ShouldBe(EventType.PurchaseOrderApproved);
        accountingEvent.SourceDocumentType.ShouldBe("PurchaseOrder");
        accountingEvent.SourceDocumentId.ShouldBe(42);
        accountingEvent.Status.ShouldBe(EventStatus.Pending);
        accountingEvent.RetryCount.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithFinancialSettingsEvent_CreatesCorrectSourceType()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var handler = new DomainEventHandler(context, new LoggerFactory().CreateLogger<DomainEventHandler>());

        var @event = new ExchangeRateActivated
        {
            SourceEntityId = 10,
            OccurredAt = DateTimeOffset.UtcNow,
            ExchangeRateId = 10,
            CurrencyPair = "USD/EUR",
            Rate = 0.85m
        };

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var accountingEvent = context.AccountingEvents.Local.FirstOrDefault();
        accountingEvent.ShouldNotBeNull();
        accountingEvent.EventType.ShouldBe(EventType.Other);
        accountingEvent.SourceDocumentType.ShouldBe("ExchangeRate");
        accountingEvent.SourceDocumentId.ShouldBe(10);
    }

    [Test]
    public async Task Handle_MultipleEvents_CreatesMultipleAccountingEvents()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var handler = new DomainEventHandler(context, new LoggerFactory().CreateLogger<DomainEventHandler>());

        var event1 = new PurchaseOrderApproved
        {
            SourceEntityId = 1,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 500m
        };

        var event2 = new PurchaseOrderApproved
        {
            SourceEntityId = 2,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 2,
            GrandTotal = 750m
        };

        // Act
        await handler.Handle(event1, CancellationToken.None);
        await handler.Handle(event2, CancellationToken.None);

        // Assert
        var accountingEvents = context.AccountingEvents.Local.ToList();
        accountingEvents.Count.ShouldBe(2);
        accountingEvents.ShouldContain(e => e.SourceDocumentId == 1);
        accountingEvents.ShouldContain(e => e.SourceDocumentId == 2);
    }

    [Test]
    public async Task Handle_DoesNotCallSaveChangesAsync()
    {
        // Arrange — T018: regression test for nested SaveChanges prevention
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDbSet = new Mock<DbSet<AccountingEvent>>();
        mockContext.Setup(x => x.AccountingEvents).Returns(mockDbSet.Object);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new DomainEventHandler(mockContext.Object, new LoggerFactory().CreateLogger<DomainEventHandler>());

        var @event = new PurchaseOrderApproved
        {
            SourceEntityId = 42,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = 1,
            GrandTotal = 1000m
        };

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert — SaveChangesAsync must NOT be called (persistence handled by OutboxProcessorService)
        mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockDbSet.Verify(x => x.Add(It.IsAny<AccountingEvent>()), Times.Once);
    }
}
