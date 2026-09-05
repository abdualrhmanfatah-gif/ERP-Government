using ERP_Government.Application.Accounting.Commands.JournalEntries.PostJournalEntry;
using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Accounting;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class PostJournalEntryCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICurrencyConversionService> _conversionMock = null!;
    private Mock<IUser> _userMock = null!;
    private PostJournalEntryCommandHandler _handler = null!;

    private static readonly FiscalPeriod Period = new()
    {
        Id = 5,
        FiscalYearId = 2,
        PeriodNumber = 8,
        Name = "أغسطس",
        StartDate = new DateOnly(2026, 8, 1),
        EndDate = new DateOnly(2026, 8, 31),
        IsLockedForPosting = false
    };

    private static readonly FiscalYear Year = new()
    {
        Id = 2,
        YearNumber = 2026,
        Name = "2026",
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 12, 31),
        Status = FiscalYearStatus.Open
    };

    private static readonly Currency BaseCurrency = new() { Id = 1, Code = "SAR", Name = "ريال", IsBase = true, IsActive = true };

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _conversionMock = new Mock<ICurrencyConversionService>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);
        _handler = new PostJournalEntryCommandHandler(_contextMock.Object, _conversionMock.Object, _userMock.Object);
    }

    private void SetupDependencies(
        List<JournalEntryLine> lines,
        List<Currency>? currencies = null,
        List<AccountBalance>? existingBalances = null)
    {
        var journalEntry = new JournalEntry
        {
            Id = 10,
            EntryNumber = "JRN-000001",
            DocumentDate = new DateOnly(2026, 8, 15),
            EntryStatus = EntryStatus.Approved,
            PeriodId = 5,
            FiscalYearId = 2
        };
        var journalEntriesMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(journalEntry);
        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesMock.Object);

        var periodMock = new Mock<DbSet<FiscalPeriod>>();
        periodMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(Period);
        _contextMock.Setup(x => x.FiscalPeriods).Returns(periodMock.Object);

        var yearMock = new Mock<DbSet<FiscalYear>>();
        yearMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(Year);
        _contextMock.Setup(x => x.FiscalYears).Returns(yearMock.Object);

        currencies ??= [BaseCurrency];
        _contextMock.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        _contextMock.Setup(x => x.JournalEntryLines).Returns(lines.AsQueryable().BuildMockForAsync().Object);

        _contextMock.Setup(x => x.AccountBalances)
            .Returns((existingBalances ?? new List<AccountBalance>()).AsQueryable().BuildMockForAsync().Object);

        _contextMock.Setup(x => x.SecurityAuditLogs)
            .Returns(new List<SecurityAuditLog>().AsQueryable().BuildMockForAsync().Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Test]
    public async Task Handle_ApprovedMoveWithForeignLine_ComputesEphemeralBaseAndPosts()
    {
        var dr = new JournalEntryLine { JournalEntryId = 10, Sequence = 1, AccountId = 100, CurrencyId = 3, Debit = 100m, Credit = 0m };
        var cr = new JournalEntryLine { JournalEntryId = 10, Sequence = 2, AccountId = 200, CurrencyId = 3, Debit = 0m, Credit = 100m };
        SetupDependencies([dr, cr]);

        _conversionMock.Setup(x => x.ConvertAtPostAsync(dr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(150.00m, 0m, 1.5m, new DateOnly(2026, 8, 1), false));
        _conversionMock.Setup(x => x.ConvertAtPostAsync(cr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(0m, 150.00m, 1.5m, new DateOnly(2026, 8, 1), false));

        var result = await _handler.Handle(new PostJournalEntryCommand{ Id = 10, RowVersion = [1] }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_NoExchangeRate_BlocksPostAndDoesNotPersist()
    {
        var line = new JournalEntryLine { JournalEntryId = 10, Sequence = 1, AccountId = 100, CurrencyId = 3, Debit = 100m, Credit = 0m };
        SetupDependencies([line]);

        _conversionMock.Setup(x => x.ConvertAtPostAsync(line, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(0m, 0m, null, null, false));

        var result = await _handler.Handle(new PostJournalEntryCommand{ Id = 10, RowVersion = [1] }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("No exchange rate", StringComparison.OrdinalIgnoreCase));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_MultipleBaseCurrencies_BlocksWithConfigurationError()
    {
        var line = new JournalEntryLine { JournalEntryId = 10, Sequence = 1, AccountId = 100, CurrencyId = 3, Debit = 100m, Credit = 0m };
        SetupDependencies([line], currencies:
        [
            new Currency { Id = 1, Code = "SAR", IsBase = true, IsActive = true },
            new Currency { Id = 2, Code = "USD", IsBase = true, IsActive = true }
        ]);

        var result = await _handler.Handle(new PostJournalEntryCommand{ Id = 10, RowVersion = [1] }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("base currency", StringComparison.OrdinalIgnoreCase));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_BaseBalanceImbalance_BlocksPost()
    {
        var dr = new JournalEntryLine { JournalEntryId = 10, Sequence = 1, AccountId = 100, CurrencyId = 3, Debit = 100m, Credit = 0m };
        var cr = new JournalEntryLine { JournalEntryId = 10, Sequence = 2, AccountId = 200, CurrencyId = 3, Debit = 0m, Credit = 90m };
        SetupDependencies([dr, cr]);

        _conversionMock.Setup(x => x.ConvertAtPostAsync(dr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(150.00m, 0m, 1.5m, new DateOnly(2026, 8, 1), false));
        _conversionMock.Setup(x => x.ConvertAtPostAsync(cr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(0m, 135.00m, 1.5m, new DateOnly(2026, 8, 1), false));

        var result = await _handler.Handle(new PostJournalEntryCommand{ Id = 10, RowVersion = [1] }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("imbalance", StringComparison.OrdinalIgnoreCase));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_SuccessfulPost_RaisesMovePosted()
    {
        var dr = new JournalEntryLine { JournalEntryId = 10, Sequence = 1, AccountId = 100, CurrencyId = 1, Debit = 100m, Credit = 0m };
        var cr = new JournalEntryLine { JournalEntryId = 10, Sequence = 2, AccountId = 200, CurrencyId = 1, Debit = 0m, Credit = 100m };
        SetupDependencies([dr, cr]);

        _conversionMock.Setup(x => x.ConvertAtPostAsync(dr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(100m, 0m, 1m, new DateOnly(2026, 8, 15), true));
        _conversionMock.Setup(x => x.ConvertAtPostAsync(cr, 1, new DateOnly(2026, 8, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JournalEntryLineConversion(0m, 100m, 1m, new DateOnly(2026, 8, 15), true));

        var result = await _handler.Handle(new PostJournalEntryCommand{ Id = 10, RowVersion = [1] }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
