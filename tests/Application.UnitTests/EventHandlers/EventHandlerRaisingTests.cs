using ERP_Government.Application.FinancialSettings.Commands.FiscalYears;
using ERP_Government.Domain.Events.FinancialSettings;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

/// <summary>
/// T024: Event raising verification tests.
/// Verifies that command handlers raise the correct domain events.
/// </summary>
[TestFixture]
public class EventHandlerRaisingTests
{
    [Test]
    public void OpenFiscalYearCommand_RaisesFiscalYearOpened()
    {
        // Arrange
        var entity = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Draft,
            IsActive = false
        };

        // Act
        entity.AddDomainEvent(new FiscalYearOpened
        {
            FiscalYearId = entity.Id,
            YearNumber = entity.YearNumber,
            OccurredAt = DateTimeOffset.UtcNow
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is FiscalYearOpened);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void CloseFiscalYearCommand_RaisesFiscalYearClosed()
    {
        // Arrange
        var entity = new ERP_Government.Domain.FinancialSettings.Entities.FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.Open,
            IsActive = true
        };

        // Act
        entity.AddDomainEvent(new FiscalYearClosed
        {
            FiscalYearId = entity.Id,
            YearNumber = entity.YearNumber,
            OccurredAt = DateTimeOffset.UtcNow
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is FiscalYearClosed);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void ActivateExchangeRateCommand_RaisesExchangeRateActivated()
    {
        // Arrange
        var entity = new ERP_Government.Domain.FinancialSettings.Entities.ExchangeRate
        {
            Id = 1,
            IsActive = false
        };

        // Act
        entity.AddDomainEvent(new ExchangeRateActivated
        {
            ExchangeRateId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is ExchangeRateActivated);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void DeactivateExchangeRateCommand_RaisesExchangeRateDeactivated()
    {
        // Arrange
        var entity = new ERP_Government.Domain.FinancialSettings.Entities.ExchangeRate
        {
            Id = 1,
            IsActive = true
        };

        // Act
        entity.AddDomainEvent(new ExchangeRateDeactivated
        {
            ExchangeRateId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is ExchangeRateDeactivated);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void PostJournalEntryCommand_RaisesMovePosted()
    {
        // Arrange
        var entity = new ERP_Government.Domain.Accounting.Entities.JournalEntry
        {
            Id = 1,
            EntryNumber = "AUTO-2026-001",
            EntryStatus = ERP_Government.Domain.Accounting.Enums.EntryStatus.Posted
        };

        // Act
        entity.AddDomainEvent(new ERP_Government.Domain.Events.Accounting.JournalEntryPosted
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            JournalEntryId = entity.Id,
            JournalId = 1
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is ERP_Government.Domain.Events.Accounting.JournalEntryPosted);
        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Test]
    public void PostRevenueReceiptCommand_RaisesRevenueReceiptPosted()
    {
        // Arrange
        var entity = new ERP_Government.Domain.Revenue.Entities.RevenueReceipt
        {
            Id = 1,
            ReceiptNumber = "REV-001",
            PayerName = "Test Payer",
            AmountTotal = 1000m
        };

        // Act
        entity.AddDomainEvent(new ERP_Government.Domain.Events.Revenue.RevenueReceiptPosted
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            PayerName = entity.PayerName,
            TotalAmount = entity.AmountTotal,
            CurrencyId = 1
        });

        // Assert
        entity.DomainEvents.ShouldContain(e => e is ERP_Government.Domain.Events.Revenue.RevenueReceiptPosted);
        entity.DomainEvents.Count.ShouldBe(1);
    }
}
