using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Accounting.Reports.TrialBalance;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.Reports;

[TestFixture]
public class GetTrialBalanceQueryHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<ReportAuditService> _auditMock = null!;
    private GetTrialBalanceQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);

        var auditLogs = new List<Domain.Security.Entities.SecurityAuditLog>();
        _contextMock.Setup(x => x.SecurityAuditLogs).Returns(auditLogs.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _auditMock = new Mock<ReportAuditService>(_contextMock.Object, _userMock.Object);
        _handler = new GetTrialBalanceQueryHandler(_contextMock.Object, _auditMock.Object);
    }

    private void SetupBalances(List<AccountBalance> balances)
    {
        _contextMock.Setup(x => x.AccountBalances)
            .Returns(balances.AsQueryable().BuildMockForAsync().Object);
    }

    [Test]
    public async Task Handle_BalancesExist_ReturnsGroupedSections()
    {
        // Arrange
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var liabilityGroup = new AccountGroup { Id = 2, Code = "2000", Name = "Liabilities", Type = AccountGroupType.Liability };
        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "YER" }, ClosingDebit = 50000, ClosingCredit = 0 },
            new() { AccountId = 101, Account = new Account { Code = "1002", Name = "Bank", AccountGroupId = 1, AccountGroup = assetGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "YER" }, ClosingDebit = 30000, ClosingCredit = 0 },
            new() { AccountId = 200, Account = new Account { Code = "2001", Name = "AP", AccountGroupId = 2, AccountGroup = liabilityGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "YER" }, ClosingDebit = 0, ClosingCredit = 80000 },
        };
        SetupBalances(balances);

        // Act
        var result = await _handler.Handle(
            new GetTrialBalanceQuery { FiscalYearId = 1, FiscalPeriodId = 1 },
            CancellationToken.None);

        // Assert
        result.Sections.Count.ShouldBe(2);
        result.TotalDebit.ShouldBe(80000);
        result.TotalCredit.ShouldBe(80000);
        result.IsBalanced.ShouldBeTrue();
        result.FiscalYearName.ShouldBe("2026");
        result.PeriodName.ShouldBe("Q1");
    }

    [Test]
    public async Task Handle_EmptyPeriod_ReturnsEmptySections()
    {
        // Arrange
        SetupBalances([]);

        // Act
        var result = await _handler.Handle(
            new GetTrialBalanceQuery { FiscalYearId = 1, FiscalPeriodId = 1 },
            CancellationToken.None);

        // Assert
        result.Sections.ShouldBeEmpty();
        result.TotalDebit.ShouldBe(0);
        result.TotalCredit.ShouldBe(0);
        result.IsBalanced.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_UnbalancedTotals_ReturnsIsBalancedFalse()
    {
        // Arrange
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "YER" }, ClosingDebit = 1000, ClosingCredit = 0 },
            new() { AccountId = 101, Account = new Account { Code = "1002", Name = "Bank", AccountGroupId = 1, AccountGroup = assetGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "YER" }, ClosingDebit = 0, ClosingCredit = 200 },
        };
        SetupBalances(balances);

        // Act
        var result = await _handler.Handle(
            new GetTrialBalanceQuery { FiscalYearId = 1, FiscalPeriodId = 1 },
            CancellationToken.None);

        // Assert
        result.TotalDebit.ShouldBe(1000);
        result.TotalCredit.ShouldBe(200);
        result.IsBalanced.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_IncludesCorrectCurrency()
    {
        // Arrange
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, FiscalYearId = 1, FiscalYear = new FiscalYear { Id = 1, Name = "2026" }, FiscalPeriodId = 1, FiscalPeriod = new FiscalPeriod { Id = 1, Name = "Q1" }, CurrencyId = 1, Currency = new Currency { Code = "USD" }, ClosingDebit = 50000, ClosingCredit = 0 },
        };
        SetupBalances(balances);

        // Act
        var result = await _handler.Handle(
            new GetTrialBalanceQuery { FiscalYearId = 1, FiscalPeriodId = 1 },
            CancellationToken.None);

        // Assert
        result.Currency.ShouldBe("USD");
    }

    [Test]
    public async Task Validate_FiscalYearIdZero_ReturnsValidationError()
    {
        // Arrange
        var validator = new GetTrialBalanceQueryValidator();
        var query = new GetTrialBalanceQuery { FiscalYearId = 0, FiscalPeriodId = 1 };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FiscalYearId");
    }

    [Test]
    public async Task Validate_FiscalPeriodIdZero_ReturnsValidationError()
    {
        // Arrange
        var validator = new GetTrialBalanceQueryValidator();
        var query = new GetTrialBalanceQuery { FiscalYearId = 1, FiscalPeriodId = 0 };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FiscalPeriodId");
    }
}
