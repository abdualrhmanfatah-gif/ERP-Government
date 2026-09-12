using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetAvailabilityBreakdown;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class AvailabilityBreakdownTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IBudgetAvailabilityService> _availabilityServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _availabilityServiceMock = new Mock<IBudgetAvailabilityService>();
    }

    private void SetupBudgetItemFindAsync(int id, BudgetItem? item)
    {
        var mockSet = new Mock<DbSet<BudgetItem>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(item));
        _contextMock.Setup(x => x.BudgetItems).Returns(mockSet.Object);
    }

    private void SetupFiscalYearFindAsync(int id, FiscalYear? fy)
    {
        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(fy));
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
    }

    // ─── T020: Breakdown per fund/program/project/item ──────────────

    [Test]
    public async Task GetAvailabilityBreakdown_ReturnsBreakdownPerDimension_ShouldReturnCorrectBreakdown()
    {
        var breakdown = new List<AvailabilityBreakdownDto>
        {
            new(1, "F1", "Fund One", 10, "P1", "Program One", null, null, null, 100, "ITEM-001", 50000m, 10000m, 5000m, 35000m),
            new(2, "F2", "Fund Two", 20, "P2", "Program Two", null, null, null, 100, "ITEM-001", 30000m, 8000m, 3000m, 19000m)
        };

        _availabilityServiceMock
            .Setup(s => s.GetAvailabilityBreakdownAsync(100, 1))
            .ReturnsAsync(breakdown);

        var budgetItem = new BudgetItem { Id = 100, ItemCode = "ITEM-001", ItemName = "Test Item", BudgetId = 1 };
        SetupBudgetItemFindAsync(100, budgetItem);

        var fy = new FiscalYear { Id = 1, Name = "FY2026" };
        SetupFiscalYearFindAsync(1, fy);

        var handler = new GetAvailabilityBreakdownQueryHandler(_availabilityServiceMock.Object, _contextMock.Object);
        var result = await handler.Handle(new GetAvailabilityBreakdownQuery(100, 1), CancellationToken.None);

        result.Breakdown.Count.ShouldBe(2);
        result.Breakdown[0].FundCode.ShouldBe("F1");
        result.Breakdown[0].FundName.ShouldBe("Fund One");
        result.Breakdown[1].FundCode.ShouldBe("F2");
        result.Breakdown[1].FundName.ShouldBe("Fund Two");
    }

    // ─── T021: Available = appropriation - encumbrances - payments ───

    [Test]
    public async Task GetAvailabilityBreakdown_ComputesAvailable_ShouldSubtractEncumbrancesAndPayments()
    {
        var breakdown = new List<AvailabilityBreakdownDto>
        {
            new(1, "F1", "Fund One", null, null, null, null, null, null, 100, "ITEM-001", 100000m, 30000m, 20000m, 50000m)
        };

        _availabilityServiceMock
            .Setup(s => s.GetAvailabilityBreakdownAsync(100, 1))
            .ReturnsAsync(breakdown);

        var budgetItem = new BudgetItem { Id = 100, ItemCode = "ITEM-001", ItemName = "Test Item", BudgetId = 1 };
        SetupBudgetItemFindAsync(100, budgetItem);

        var fy = new FiscalYear { Id = 1, Name = "FY2026" };
        SetupFiscalYearFindAsync(1, fy);

        var handler = new GetAvailabilityBreakdownQueryHandler(_availabilityServiceMock.Object, _contextMock.Object);
        var result = await handler.Handle(new GetAvailabilityBreakdownQuery(100, 1), CancellationToken.None);

        var line = result.Breakdown[0];
        line.AvailableAmount.ShouldBe(50000m); // 100000 - 30000 - 20000
        result.Totals.AppropriationAmount.ShouldBe(100000m);
        result.Totals.EncumberedAmount.ShouldBe(30000m);
        result.Totals.PaidAmount.ShouldBe(20000m);
        result.Totals.AvailableAmount.ShouldBe(50000m);
    }

    // ─── T022: Empty for non-existent budget item ───────────────────

    [Test]
    public async Task GetAvailabilityBreakdown_NonExistentBudgetItem_ShouldReturnEmpty()
    {
        _availabilityServiceMock
            .Setup(s => s.GetAvailabilityBreakdownAsync(999, 1))
            .ReturnsAsync(new List<AvailabilityBreakdownDto>());

        SetupBudgetItemFindAsync(999, null);

        var fy = new FiscalYear { Id = 1, Name = "FY2026" };
        SetupFiscalYearFindAsync(1, fy);

        var handler = new GetAvailabilityBreakdownQueryHandler(_availabilityServiceMock.Object, _contextMock.Object);
        var result = await handler.Handle(new GetAvailabilityBreakdownQuery(999, 1), CancellationToken.None);

        result.Breakdown.ShouldBeEmpty();
        result.Totals.AppropriationAmount.ShouldBe(0m);
        result.Totals.EncumberedAmount.ShouldBe(0m);
        result.Totals.PaidAmount.ShouldBe(0m);
        result.Totals.AvailableAmount.ShouldBe(0m);
    }

    // ─── T023: Negative availability when encumbrances exceed ──────

    [Test]
    public async Task GetAvailabilityBreakdown_EncumbrancesExceedAppropriations_ShouldShowNegative()
    {
        var breakdown = new List<AvailabilityBreakdownDto>
        {
            new(1, "F1", "Fund One", null, null, null, null, null, null, 100, "ITEM-001", 50000m, 60000m, 0m, -10000m)
        };

        _availabilityServiceMock
            .Setup(s => s.GetAvailabilityBreakdownAsync(100, 1))
            .ReturnsAsync(breakdown);

        var budgetItem = new BudgetItem { Id = 100, ItemCode = "ITEM-001", ItemName = "Test Item", BudgetId = 1 };
        SetupBudgetItemFindAsync(100, budgetItem);

        var fy = new FiscalYear { Id = 1, Name = "FY2026" };
        SetupFiscalYearFindAsync(1, fy);

        var handler = new GetAvailabilityBreakdownQueryHandler(_availabilityServiceMock.Object, _contextMock.Object);
        var result = await handler.Handle(new GetAvailabilityBreakdownQuery(100, 1), CancellationToken.None);

        result.Breakdown[0].AvailableAmount.ShouldBe(-10000m); // 50000 - 60000 - 0
        result.Totals.AvailableAmount.ShouldBe(-10000m);
    }

    // ─── T024: Scoped to single fiscal year ────────────────────────

    [Test]
    public async Task GetAvailabilityBreakdown_ScopedToFiscalYear_ShouldQueryCorrectYear()
    {
        var breakdown = new List<AvailabilityBreakdownDto>
        {
            new(1, "F1", "Fund One", null, null, null, null, null, null, 100, "ITEM-001", 80000m, 15000m, 10000m, 55000m)
        };

        _availabilityServiceMock
            .Setup(s => s.GetAvailabilityBreakdownAsync(100, 2))
            .ReturnsAsync(breakdown);

        var budgetItem = new BudgetItem { Id = 100, ItemCode = "ITEM-001", ItemName = "Test Item", BudgetId = 1 };
        SetupBudgetItemFindAsync(100, budgetItem);

        var fy = new FiscalYear { Id = 2, Name = "FY2026" };
        SetupFiscalYearFindAsync(2, fy);

        var handler = new GetAvailabilityBreakdownQueryHandler(_availabilityServiceMock.Object, _contextMock.Object);
        var result = await handler.Handle(new GetAvailabilityBreakdownQuery(100, 2), CancellationToken.None);

        result.FiscalYearId.ShouldBe(2);
        result.FiscalYearName.ShouldBe("FY2026");
        _availabilityServiceMock.Verify(s => s.GetAvailabilityBreakdownAsync(100, 2), Times.Once);
    }
}
