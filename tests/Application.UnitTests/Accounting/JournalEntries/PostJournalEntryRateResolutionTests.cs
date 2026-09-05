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
public class PostJournalEntryRateResolutionTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrencyConversionService> _conversionMock = null!;
    private Mock<IUser> _userMock = null!;
    private PostJournalEntryCommandHandler _handler = null!;

    private const int BaseCurrencyId = 1;
    private const int ForeignCurrencyId = 3;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _conversionMock = new Mock<ICurrencyConversionService>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);

        _handler = new PostJournalEntryCommandHandler(
            _contextMock.Object,
            _conversionMock.Object,
            _userMock.Object);
    }

    [Test]
    public async Task PostMove_NearestOnOrBefore_ShouldUseCorrectRate()
    {
        // Arrange — two rates: 1.4 on Jan 1, 1.5 on Feb 1; post with DocumentDate=Jan 15
        var journalEntry = new JournalEntry
        {
            Id = 1,
            EntryStatus = EntryStatus.Approved,
            DocumentDate = new DateOnly(2026, 1, 15),
            PeriodId = 1,
            FiscalYearId = 1
        };

        var lines = new List<JournalEntryLine>
        {
            new() { Id = 100, JournalEntryId = 1, AccountId = 10, CurrencyId = ForeignCurrencyId, Debit = 1000m, Credit = 0m },
            new() { Id = 101, JournalEntryId = 1, AccountId = 11, CurrencyId = ForeignCurrencyId, Debit = 0m, Credit = 1000m }
        };

        var baseCurrency = new Currency { Id = BaseCurrencyId, Code = "IQD", IsBase = true, IsActive = true };

        // Simulate ExchangeRateResolver: nearest-on-or-before for Jan 15 = rate from Jan 1 (1.4)
        _conversionMock.Setup(c => c.ConvertAtPostAsync(
                It.IsAny<JournalEntryLine>(), BaseCurrencyId, new DateOnly(2026, 1, 15),
                ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(1400m, 1400m, 1.4m, new DateOnly(2026, 1, 1), false));

        SetupContext(journalEntry, lines, [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue($"Expected success but got: {string.Join("; ", result.Errors)}");

        // Verify the audit log contains the resolved rate date
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.Success == true &&
            log.NewValues != null &&
            log.NewValues.Contains("2026-01-01") // RateDateResolved from rate on Jan 1
        )), Times.Once);
    }

    [Test]
    public async Task PostMove_RateExactlyAtBoundary_ShouldBeInclusive()
    {
        // Arrange — rate with RateDate=Jan 15, post with DocumentDate=Jan 15 (inclusive boundary)
        var journalEntry = new JournalEntry
        {
            Id = 1,
            EntryStatus = EntryStatus.Approved,
            DocumentDate = new DateOnly(2026, 1, 15),
            PeriodId = 1,
            FiscalYearId = 1
        };

        var lines = new List<JournalEntryLine>
        {
            new() { Id = 100, JournalEntryId = 1, AccountId = 10, CurrencyId = ForeignCurrencyId, Debit = 500m, Credit = 0m },
            new() { Id = 101, JournalEntryId = 1, AccountId = 11, CurrencyId = ForeignCurrencyId, Debit = 0m, Credit = 500m }
        };

        var baseCurrency = new Currency { Id = BaseCurrencyId, Code = "IQD", IsBase = true, IsActive = true };

        // Rate from Jan 15 is used (inclusive: RateDate <= date)
        _conversionMock.Setup(c => c.ConvertAtPostAsync(
                It.IsAny<JournalEntryLine>(), BaseCurrencyId, new DateOnly(2026, 1, 15),
                ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(750m, 750m, 1.5m, new DateOnly(2026, 1, 15), false));

        SetupContext(journalEntry, lines, [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue($"Expected success but got: {string.Join("; ", result.Errors)}");

        // Verify the audit log shows RateDateResolved = Jan 15 (same day, inclusive)
        _contextMock.Verify(c => c.SecurityAuditLogs.Add(It.Is<SecurityAuditLog>(log =>
            log.Success == true &&
            log.NewValues != null &&
            log.NewValues.Contains("2026-01-15") // RateDateResolved = same day
        )), Times.Once);
    }

    [Test]
    public async Task PostMove_Immutability_AlreadyPostedLinesPreserved()
    {
        // Arrange — journalEntry with EntryStatus = Posted (already posted)
        var journalEntry = new JournalEntry
        {
            Id = 1,
            EntryStatus = EntryStatus.Posted,
            DocumentDate = new DateOnly(2026, 1, 15),
            PeriodId = 1,
            FiscalYearId = 1
        };

        var lines = new List<JournalEntryLine>
        {
            new() { Id = 100, JournalEntryId = 1, AccountId = 10, CurrencyId = ForeignCurrencyId, Debit = 1000m, Credit = 0m }
        };

        var baseCurrency = new Currency { Id = BaseCurrencyId, Code = "IQD", IsBase = true, IsActive = true };

        SetupContext(journalEntry, lines, [baseCurrency]);

        // Act
        var result = await _handler.Handle(new PostJournalEntryCommand { Id = 1, RowVersion = [1, 2, 3] }, CancellationToken.None);

        // Assert — posting blocked (lifecycle guard: only Approved journal entries can be posted)
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only approved journal entries can be posted"));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Helpers

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

        _contextMock.Setup(c => c.AccountBalances)
            .Returns(new List<AccountBalance>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SecurityAuditLogs)
            .Returns(new List<SecurityAuditLog>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #endregion
}
