using ERP_Government.Application.Accounting.Reports.BalanceSheet;
using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Security.Entities;
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

        _contextMock.Setup(x => x.SecurityAuditLogs)
            .Returns(new List<SecurityAuditLog>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _contextMock.Setup(x => x.Currencies)
            .Returns(new List<Currency>
            {
                new() { Id = 1, Code = "YER", Name = "Yemeni Rial", IsBase = true, IsActive = true }
            }.AsQueryable().BuildMockForAsync().Object);
        SetupFiscalPeriods([]);

        _auditMock = new Mock<ReportAuditService>(_contextMock.Object, _userMock.Object);
        _handler = new GetBalanceSheetQueryHandler(_contextMock.Object, _auditMock.Object);
    }

    [Test]
    public async Task Handle_ClassifiesBalanceSheetIntoDetailedSections()
    {
        var cashGroup = Group(18, "18", "Cash", AccountGroupType.Asset);
        var fixedAssetGroup = Group(11, "11", "Fixed Assets", AccountGroupType.Asset);
        var investmentGroup = Group(13, "13", "Investments", AccountGroupType.Asset);
        var payableGroup = Group(25, "25", "Payables", AccountGroupType.Liability);
        var accrualGroup = Group(27, "27", "Accruals", AccountGroupType.Liability);
        var equityGroup = Group(22, "22", "Equity", AccountGroupType.Equity);
        var revenueGroup = Group(41, "41", "Revenue", AccountGroupType.Revenue);
        var expenseGroup = Group(31, "31", "Expenses", AccountGroupType.Expense);

        SetupJournalEntryLines(
        [
            Line(18101, "18101", "Cash on hand", cashGroup, debit: 100m),
            Line(11001, "11001", "Building", fixedAssetGroup, debit: 250m),
            Line(13001, "13001", "Long investment", investmentGroup, debit: 300m),
            Line(25101, "25101", "Accounts payable", payableGroup, credit: 180m),
            Line(27201, "27201", "Accrued expenses", accrualGroup, credit: 20m),
            Line(22001, "22001", "Paid capital", equityGroup, credit: 400m),
            Line(41001, "41001", "Revenue", revenueGroup, credit: 90m),
            Line(31001, "31001", "Expense", expenseGroup, debit: 40m),
        ]);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = "2026-12-31" },
            CancellationToken.None);

        result.CurrentAssets.Sections.Single(x => x.Title == "النقد وما في حكمه").Total.ShouldBe(100m);
        result.NonCurrentAssets.Sections.Single(x => x.Title == "الممتلكات والآلات والمعدات").Total.ShouldBe(250m);
        result.NonCurrentAssets.Sections.Single(x => x.Title == "الاستثمارات طويلة الأجل").Total.ShouldBe(300m);
        result.CurrentLiabilities.Sections.Single(x => x.Title == "الحسابات الدائنة / الذمم الدائنة").Total.ShouldBe(180m);
        result.CurrentLiabilities.Sections.Single(x => x.Title == "المصروفات المستحقة").Total.ShouldBe(20m);
        result.Equity.Sections.Single(x => x.Title == "صافي الدخل / الخسارة").Total.ShouldBe(50m);
        result.Assets.Total.ShouldBe(650m);
        result.LiabilitiesAndEquity.ShouldBe(650m);
        result.Balanced.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_IncludesInactiveAccountsForHistoricalReports()
    {
        var cashGroup = Group(18, "18", "Cash", AccountGroupType.Asset);

        SetupJournalEntryLines(
        [
            Line(18101, "18101", "Inactive cash", cashGroup, debit: 75m, accountIsActive: false),
        ]);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = "2026-12-31" },
            CancellationToken.None);

        result.Assets.Total.ShouldBe(75m);
        result.CurrentAssets.Sections.Single(x => x.Title == "النقد وما في حكمه").Total.ShouldBe(75m);
    }

    [Test]
    public async Task Handle_FiscalPeriodCapsAsOfDateToPeriodEnd()
    {
        var cashGroup = Group(18, "18", "Cash", AccountGroupType.Asset);
        SetupFiscalPeriods(
        [
            new()
            {
                Id = 10,
                FiscalYearId = 1,
                PeriodNumber = 1,
                Name = "January",
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 1, 31),
                IsActive = true
            }
        ]);

        SetupJournalEntryLines(
        [
            Line(18101, "18101", "January cash", cashGroup, debit: 75m, date: new DateOnly(2026, 1, 31)),
            Line(18101, "18101", "February cash", cashGroup, debit: 25m, date: new DateOnly(2026, 2, 1)),
        ]);

        var result = await _handler.Handle(
            new GetBalanceSheetQuery { AsOfDate = "2026-02-28", FiscalPeriodId = 10 },
            CancellationToken.None);

        result.AsOfDate.ShouldBe(new DateOnly(2026, 1, 31));
        result.Assets.Total.ShouldBe(75m);
    }

    [Test]
    public void Validator_RejectsFutureAsOfDate()
    {
        var validator = new GetBalanceSheetQueryValidator();

        var result = validator.Validate(new GetBalanceSheetQuery
        {
            AsOfDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd")
        });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorMessage == "As of date cannot be in the future.");
    }

    private void SetupJournalEntryLines(List<JournalEntryLine> lines)
    {
        _contextMock.Setup(x => x.JournalEntryLines)
            .Returns(lines.AsQueryable().BuildMockForAsync().Object);
    }

    private void SetupFiscalPeriods(List<FiscalPeriod> periods)
    {
        _contextMock.Setup(x => x.FiscalPeriods)
            .Returns(periods.AsQueryable().BuildMockForAsync().Object);
    }

    private static AccountGroup Group(int id, string code, string name, AccountGroupType type) =>
        new()
        {
            Id = id,
            Code = code,
            Name = name,
            Type = type,
            IsActive = true
        };

    private static JournalEntryLine Line(
        int accountId,
        string accountCode,
        string accountName,
        AccountGroup group,
        decimal debit = 0,
        decimal credit = 0,
        DateOnly? date = null,
        bool accountIsActive = true) =>
        new()
        {
            AccountId = accountId,
            Account = new Account
            {
                Id = accountId,
                Code = accountCode,
                Name = accountName,
                AccountGroupId = group.Id,
                AccountGroup = group,
                IsActive = accountIsActive
            },
            CurrencyId = 1,
            ExchangeRate = 1,
            Debit = debit,
            Credit = credit,
            JournalEntry = new JournalEntry
            {
                EntryStatus = EntryStatus.Posted,
                DocumentDate = date ?? new DateOnly(2026, 6, 30),
                PeriodId = 1,
                FiscalYearId = 1,
            }
        };
}
