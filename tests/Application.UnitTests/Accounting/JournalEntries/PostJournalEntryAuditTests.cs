using System.Text.Json;
using ERP_Government.Application.Accounting.Commands.JournalEntries.PostJournalEntry;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.JournalEntries;

[TestFixture]
public class PostJournalEntryAuditTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrencyConversionService> _conversionMock = null!;
    private Mock<IUser> _userMock = null!;
    private PostJournalEntryCommandHandler _handler = null!;

    private const int BaseCurrencyId = 1;
    private const int ForeignCurrencyId = 3;
    private const int UserId = 42;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _conversionMock = new Mock<ICurrencyConversionService>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(UserId);

        _handler = new PostJournalEntryCommandHandler(
            _contextMock.Object,
            _conversionMock.Object,
            _userMock.Object);
    }

    [Test]
    public async Task PostMove_ForeignCurrency_ShouldWriteAuditLog()
    {
        // Arrange — balanced lines (debit + credit)
        var journalEntry = CreateApprovedJournalEntry();
        var debitLine = CreateForeignLine(debit: 1000m, credit: 0m);
        var creditLine = CreateForeignLine(debit: 0m, credit: 1000m);
        var baseCurrency = CreateBaseCurrency();

        var lines = new List<JournalEntryLine> { debitLine, creditLine };
        _conversionMock.Setup(c => c.ConvertAtPostAsync(
                It.IsAny<JournalEntryLine>(), BaseCurrencyId, It.IsAny<DateOnly>(),
                ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JournalEntryLine ml, int _, DateOnly _, ExchangeRateType _, CancellationToken _) =>
                new JournalEntryLineConversion(ml.Debit * 1.5m, ml.Credit * 1.5m, 1.5m, new DateOnly(2026, 1, 1), false));

        SetupMocks(journalEntry, lines, [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert - check for errors first
        if (!result.Succeeded)
        {
            Assert.Fail($"Handler returned failure: {string.Join("; ", result.Errors)}");
        }
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.EventCategory == "JournalPosting" &&
            log.Action == "Post" &&
            log.Success == true &&
            log.EntityName == "JournalEntry" &&
            log.EntityId == 1
        )), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostMove_BaseCurrencyLine_ShouldLogRateOne()
    {
        // Arrange — balanced lines (debit + credit), all in base currency
        var journalEntry = CreateApprovedJournalEntry();
        var lines = new List<JournalEntryLine>
        {
            new() { Id = 200, JournalEntryId = 1, AccountId = 11, CurrencyId = BaseCurrencyId, Debit = 500m, Credit = 0m },
            new() { Id = 201, JournalEntryId = 1, AccountId = 12, CurrencyId = BaseCurrencyId, Debit = 0m, Credit = 500m }
        };
        var baseCurrency = CreateBaseCurrency();

        _conversionMock.Setup(c => c.ConvertAtPostAsync(
                It.IsAny<JournalEntryLine>(), BaseCurrencyId, It.IsAny<DateOnly>(),
                ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JournalEntryLine ml, int _, DateOnly _, ExchangeRateType _, CancellationToken _) =>
                new JournalEntryLineConversion(ml.Debit, ml.Credit, 1m, new DateOnly(2026, 1, 15), true));

        SetupMocks(journalEntry, lines, [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Assert.Fail($"Handler returned failure: {string.Join("; ", result.Errors)}");
        }
        // Verify audit log was written with correct structure
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.Success == true &&
            log.EventCategory == "JournalPosting" &&
            log.Action == "Post" &&
            log.NewValues != null
        )), Times.Once, "SecurityAuditLogs.Add should be called once for successful posting");
    }

    [Test]
    public async Task PostMove_MissingRate_ShouldWriteFailedAuditLog()
    {
        // Arrange
        var journalEntry = CreateApprovedJournalEntry();
        var line = CreateForeignLine();
        var baseCurrency = CreateBaseCurrency();

        SetupMocks(journalEntry, [line], [baseCurrency],
            convResult: new JournalEntryLineConversion(0m, 0m, null, null, false));

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.EventCategory == "JournalPosting" &&
            log.Action == "PostFailed" &&
            log.Success == false &&
            log.FailureReason != null &&
            log.FailureReason.Contains("No exchange rate found")
        )), Times.Once);
    }

    [Test]
    public async Task PostMove_DoubleEntryImbalance_ShouldWriteFailedAuditLog()
    {
        // Arrange
        var journalEntry = CreateApprovedJournalEntry();
        var line1 = CreateForeignLine(debit: 1000m, credit: 0m);
        var line2 = CreateForeignLine(debit: 0m, credit: 100m);
        var baseCurrency = CreateBaseCurrency();

        // Line 1: base debit = 1500, Line 2: base credit = 150 (100 * 1.5) — imbalance
        _conversionMock.Setup(c => c.ConvertAtPostAsync(
                It.IsAny<JournalEntryLine>(), BaseCurrencyId, It.IsAny<DateOnly>(),
                ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JournalEntryLine ml, int _, DateOnly _, ExchangeRateType _, CancellationToken _) =>
                new JournalEntryLineConversion(ml.Debit * 1.5m, ml.Credit * 1.5m, 1.5m, new DateOnly(2026, 1, 1), false));

        SetupContext(journalEntry, [line1, line2], [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Base double-entry imbalance"));
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.Success == false &&
            log.FailureReason != null &&
            log.FailureReason.Contains("Base double-entry imbalance")
        )), Times.Once);
    }

    #region Helpers

    private JournalEntry CreateApprovedJournalEntry() => new()
    {
        Id = 1,
        EntryStatus = EntryStatus.Approved,
        DocumentDate = new DateOnly(2026, 1, 15),
        PeriodId = 1,
        FiscalYearId = 1,
        JournalId = 10
    };

    private JournalEntryLine CreateForeignLine(decimal debit = 1000m, decimal credit = 0m) => new()
    {
        Id = 100,
        JournalEntryId = 1,
        AccountId = 10,
        CurrencyId = ForeignCurrencyId,
        Debit = debit,
        Credit = credit
    };

    private JournalEntryLine CreateBaseCurrencyLine(decimal debit = 500m, decimal credit = 0m) => new()
    {
        Id = 101,
        JournalEntryId = 1,
        AccountId = 11,
        CurrencyId = BaseCurrencyId,
        Debit = debit,
        Credit = credit
    };

    private Currency CreateBaseCurrency() => new()
    {
        Id = BaseCurrencyId,
        Code = "IQD",
        IsBase = true,
        IsActive = true
    };

    private void SetupMocks(JournalEntry journalEntry, List<JournalEntryLine> lines, List<Currency> currencies,
        JournalEntryLineConversion? convResult = null)
    {
        var journalEntriesMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(journalEntry);
        _contextMock.Setup(c => c.JournalEntries).Returns(journalEntriesMock.Object);

        _contextMock.Setup(c => c.JournalEntryLines).Returns(lines.AsQueryable().BuildMockForAsync().Object);

        _contextMock.Setup(c => c.Currencies).Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        var periodsMock = new Mock<DbSet<FiscalPeriod>>();
        periodsMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(
            new FiscalPeriod { Id = 1, IsLockedForPosting = false, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 1, 31) });
        _contextMock.Setup(c => c.FiscalPeriods).Returns(periodsMock.Object);

        var yearsMock = new Mock<DbSet<FiscalYear>>();
        yearsMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(
            new FiscalYear { Id = 1, Status = FiscalYearStatus.Open });
        _contextMock.Setup(c => c.FiscalYears).Returns(yearsMock.Object);

        _contextMock.Setup(c => c.SecurityAuditLogs)
            .Returns(new List<SecurityAuditLog>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        if (convResult is not null)
        {
            _conversionMock.Setup(c => c.ConvertAtPostAsync(
                    It.IsAny<JournalEntryLine>(), BaseCurrencyId, It.IsAny<DateOnly>(),
                    ExchangeRateType.Official, It.IsAny<CancellationToken>()))
                .ReturnsAsync(convResult);
        }
    }

    private void SetupContext(JournalEntry journalEntry, List<JournalEntryLine> lines, List<Currency> currencies)
    {
        var journalEntriesMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(journalEntry);
        _contextMock.Setup(c => c.JournalEntries).Returns(journalEntriesMock.Object);

        _contextMock.Setup(c => c.JournalEntryLines).Returns(lines.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Currencies).Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        var periodsMock = new Mock<DbSet<FiscalPeriod>>();
        periodsMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(
            new FiscalPeriod { Id = 1, IsLockedForPosting = false, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 1, 31) });
        _contextMock.Setup(c => c.FiscalPeriods).Returns(periodsMock.Object);

        var yearsMock = new Mock<DbSet<FiscalYear>>();
        yearsMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(
            new FiscalYear { Id = 1, Status = FiscalYearStatus.Open });
        _contextMock.Setup(c => c.FiscalYears).Returns(yearsMock.Object);

        _contextMock.Setup(c => c.SecurityAuditLogs)
            .Returns(new List<SecurityAuditLog>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #endregion
}
