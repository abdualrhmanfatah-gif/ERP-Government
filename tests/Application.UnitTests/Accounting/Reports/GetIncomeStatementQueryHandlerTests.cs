using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Accounting.Reports.IncomeStatement;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.Reports;

[TestFixture]
public class GetIncomeStatementQueryHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<ReportAuditService> _auditMock = null!;
    private GetIncomeStatementQueryHandler _handler = null!;

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
        _handler = new GetIncomeStatementQueryHandler(_contextMock.Object, _auditMock.Object);
    }

    private void SetupJournalEntryLines(List<JournalEntryLine> lines)
    {
        _contextMock.Setup(x => x.JournalEntryLines)
            .Returns(lines.AsQueryable().BuildMockForAsync().Object);
    }

    [Test]
    public async Task Handle_EmptyJournalEntryLines_ReturnsZeroNetIncome()
    {
        SetupJournalEntryLines([]);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-12-31" },
            CancellationToken.None);

        result.Revenue.Total.ShouldBe(0);
        result.Expenses.Total.ShouldBe(0);
        result.NetIncome.ShouldBe(0);
    }

    [Test]
    public async Task Handle_RevenueOnly_ReturnsPositiveNetIncome()
    {
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var lines = new List<JournalEntryLine>
        {
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Sales Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 100000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 3, 15) } },
            new() { AccountId = 401, Account = new Account { Code = "4002", Name = "Service Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 50000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 6, 20) } },
        };
        SetupJournalEntryLines(lines);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-12-31" },
            CancellationToken.None);

        result.Revenue.Total.ShouldBe(150000);
        result.Expenses.Total.ShouldBe(0);
        result.NetIncome.ShouldBe(150000);
    }

    [Test]
    public async Task Handle_ExpensesOnly_ReturnsNegativeNetIncome()
    {
        var expenseGroup = new AccountGroup { Id = 5, Code = "5000", Name = "Expenses", Type = AccountGroupType.Expense };
        var lines = new List<JournalEntryLine>
        {
            new() { AccountId = 500, Account = new Account { Code = "5001", Name = "Rent Expense", AccountGroupId = 5, AccountGroup = expenseGroup }, Debit = 30000, Credit = 0, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 1, 5) } },
            new() { AccountId = 501, Account = new Account { Code = "5002", Name = "Salary Expense", AccountGroupId = 5, AccountGroup = expenseGroup }, Debit = 80000, Credit = 0, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 6, 30) } },
        };
        SetupJournalEntryLines(lines);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-12-31" },
            CancellationToken.None);

        result.Revenue.Total.ShouldBe(0);
        result.Expenses.Total.ShouldBe(110000);
        result.NetIncome.ShouldBe(-110000);
    }

    [Test]
    public async Task Handle_RevenueAndExpenses_ComputesCorrectNetIncome()
    {
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var expenseGroup = new AccountGroup { Id = 5, Code = "5000", Name = "Expenses", Type = AccountGroupType.Expense };
        var lines = new List<JournalEntryLine>
        {
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Sales Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 200000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 3, 15) } },
            new() { AccountId = 500, Account = new Account { Code = "5001", Name = "COGS", AccountGroupId = 5, AccountGroup = expenseGroup }, Debit = 80000, Credit = 0, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 3, 16) } },
            new() { AccountId = 501, Account = new Account { Code = "5002", Name = "Operating Expenses", AccountGroupId = 5, AccountGroup = expenseGroup }, Debit = 60000, Credit = 0, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 6, 30) } },
        };
        SetupJournalEntryLines(lines);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-12-31" },
            CancellationToken.None);

        result.Revenue.Total.ShouldBe(200000);
        result.Expenses.Total.ShouldBe(140000);
        result.NetIncome.ShouldBe(60000);
    }

    [Test]
    public async Task Handle_DateRangeFilter_OnlyIncludesMatchingEntries()
    {
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var lines = new List<JournalEntryLine>
        {
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 100000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 3, 15) } },
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 50000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 6, 20) } },
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 30000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 9, 10) } },
        };
        SetupJournalEntryLines(lines);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-06-30" },
            CancellationToken.None);

        // Only Q1 and Q2 revenue included
        result.Revenue.Total.ShouldBe(150000);
    }

    [Test]
    public async Task Handle_UnpostedEntries_AreExcluded()
    {
        var revenueGroup = new AccountGroup { Id = 4, Code = "4000", Name = "Revenue", Type = AccountGroupType.Revenue };
        var lines = new List<JournalEntryLine>
        {
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 100000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Posted, DocumentDate = new DateOnly(2026, 3, 15) } },
            new() { AccountId = 400, Account = new Account { Code = "4001", Name = "Revenue", AccountGroupId = 4, AccountGroup = revenueGroup }, Debit = 0, Credit = 50000, JournalEntry = new JournalEntry { EntryStatus = EntryStatus.Draft, DocumentDate = new DateOnly(2026, 6, 20) } },
        };
        SetupJournalEntryLines(lines);

        var result = await _handler.Handle(
            new GetIncomeStatementQuery { StartDate = "2026-01-01", EndDate = "2026-12-31" },
            CancellationToken.None);

        result.Revenue.Total.ShouldBe(100000);
    }
}
