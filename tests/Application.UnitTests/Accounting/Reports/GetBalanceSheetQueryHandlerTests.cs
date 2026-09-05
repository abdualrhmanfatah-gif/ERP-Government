using ERP_Government.Application.Accounting.Reports.BalanceSheet;
using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.Reports;

[TestFixture]
public class GetBalanceSheetQueryHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<ReportAuditService> _auditMock = null!;
    private GetBalanceSheetQueryHandler _handler = null!;

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
        _handler = new GetBalanceSheetQueryHandler(_contextMock.Object, _auditMock.Object);
    }

    private void SetupBalances(List<AccountBalance> balances)
    {
        _contextMock.Setup(x => x.AccountBalances)
            .Returns(balances.AsQueryable().BuildMockForAsync().Object);
    }

    [Test]
    public async Task Handle_EmptyBalances_ReturnsZeroAssetsAndEquity()
    {
        SetupBalances([]);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = DateTime.Today.ToString("yyyy-MM-dd") },
            CancellationToken.None);

        result.Assets.Total.ShouldBe(0);
        result.Liabilities.Total.ShouldBe(0);
        result.Equity.Total.ShouldBe(0);
        result.Balanced.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_AssetsOnly_ReturnsBalancedWithZeroLiabilitiesAndEquity()
    {
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, ClosingDebit = 50000, ClosingCredit = 0 },
            new() { AccountId = 101, Account = new Account { Code = "1002", Name = "Bank", AccountGroupId = 1, AccountGroup = assetGroup }, ClosingDebit = 30000, ClosingCredit = 0 },
        };
        SetupBalances(balances);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = DateTime.Today.ToString("yyyy-MM-dd") },
            CancellationToken.None);

        result.Assets.Total.ShouldBe(80000);
        result.Liabilities.Total.ShouldBe(0);
        result.Equity.Total.ShouldBe(0);
        result.Balanced.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_ComplexScenario_AssetsEqualsLiabilitiesPlusEquity()
    {
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var liabilityGroup = new AccountGroup { Id = 2, Code = "2000", Name = "Liabilities", Type = AccountGroupType.Liability };
        var equityGroup = new AccountGroup { Id = 3, Code = "3000", Name = "Equity", Type = AccountGroupType.Equity };
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var expenseGroup = new AccountGroup { Id = 5, Code = "5000", Name = "Expenses", Type = AccountGroupType.Expense };

        var balances = new List<AccountBalance>
        {
            // Assets: 100,000
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, ClosingDebit = 100000, ClosingCredit = 0 },
            // Liabilities: 40,000
            new() { AccountId = 200, Account = new Account { Code = "2001", Name = "Accounts Payable", AccountGroupId = 2, AccountGroup = liabilityGroup }, ClosingDebit = 0, ClosingCredit = 40000 },
            // Equity: 30,000
            new() { AccountId = 300, Account = new Account { Code = "3001", Name = "Capital", AccountGroupId = 3, AccountGroup = equityGroup }, ClosingDebit = 0, ClosingCredit = 30000 },
            // Revenue: 50,000
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Sales Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, ClosingDebit = 0, ClosingCredit = 50000 },
            // Expenses: 20,000
            new() { AccountId = 500, Account = new Account { Code = "5001", Name = "Rent Expense", AccountGroupId = 5, AccountGroup = expenseGroup }, ClosingDebit = 20000, ClosingCredit = 0 },
        };
        SetupBalances(balances);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = DateTime.Today.ToString("yyyy-MM-dd") },
            CancellationToken.None);

        // Net Income = Revenue (50,000) - Expenses (20,000) = 30,000
        // Total Equity = Equity (30,000) + Net Income (30,000) = 60,000
        // Liabilities + Equity = 40,000 + 60,000 = 100,000
        result.Assets.Total.ShouldBe(100000);
        result.Liabilities.Total.ShouldBe(40000);
        result.Equity.Total.ShouldBe(60000); // 30,000 + 30,000 net income
        result.LiabilitiesAndEquity.ShouldBe(100000);
        result.Balanced.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_InactiveAccounts_AreExcluded()
    {
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup, IsActive = true }, ClosingDebit = 50000, ClosingCredit = 0 },
            new() { AccountId = 101, Account = new Account { Code = "1002", Name = "Old Account", AccountGroupId = 1, AccountGroup = assetGroup, IsActive = false }, ClosingDebit = 10000, ClosingCredit = 0 },
        };
        SetupBalances(balances);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = DateTime.Today.ToString("yyyy-MM-dd") },
            CancellationToken.None);

        result.Assets.Total.ShouldBe(50000);
    }

    [Test]
    public async Task Handle_NegativeNetIncome_SubtractsFromEquity()
    {
        var assetGroup = new AccountGroup { Id = 1, Code = "1000", Name = "Assets", Type = AccountGroupType.Asset };
        var liabilityGroup = new AccountGroup { Id = 2, Code = "2000", Name = "Liabilities", Type = AccountGroupType.Liability };
        var equityGroup = new AccountGroup { Id = 3, Code = "3000", Name = "Equity", Type = AccountGroupType.Equity };
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var expenseGroup = new AccountGroup { Id = 5, Code = "5000", Name = "Expenses", Type = AccountGroupType.Expense };

        var balances = new List<AccountBalance>
        {
            new() { AccountId = 100, Account = new Account { Code = "1001", Name = "Cash", AccountGroupId = 1, AccountGroup = assetGroup }, ClosingDebit = 80000, ClosingCredit = 0 },
            new() { AccountId = 200, Account = new Account { Code = "2001", Name = "AP", AccountGroupId = 2, AccountGroup = liabilityGroup }, ClosingDebit = 0, ClosingCredit = 50000 },
            new() { AccountId = 300, Account = new Account { Code = "3001", Name = "Capital", AccountGroupId = 3, AccountGroup = equityGroup }, ClosingDebit = 0, ClosingCredit = 50000 },
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, ClosingDebit = 0, ClosingCredit = 20000 },
            new() { AccountId = 500, Account = new Account { Code = "5001", Name = "Expenses", AccountGroupId = 5, AccountGroup = expenseGroup }, ClosingDebit = 40000, ClosingCredit = 0 },
        };
        SetupBalances(balances);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = DateTime.Today.ToString("yyyy-MM-dd") },
            CancellationToken.None);

        // Net Income = Revenue (20,000) - Expenses (40,000) = -20,000
        // Total Equity = 50,000 + (-20,000) = 30,000
        // Assets (80,000) = Liabilities (50,000) + Equity (30,000) = 80,000
        result.Equity.Total.ShouldBe(30000);
        result.Balanced.ShouldBeTrue();
    }
}
