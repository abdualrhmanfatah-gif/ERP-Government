using ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class LapseFiscalYearTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private async Task SeedFiscalYear(int id, string name, bool isClosed = false)
    {
        _dbContext.FiscalYears.Add(new FiscalYear { Id = id, Name = name, IsClosed = isClosed, YearNumber = 2026, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), Status = Domain.FinancialSettings.Enums.FiscalYearStatus.Open });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedAppropriation(int id, int budgetItemId, decimal amount, AppropriationType type, AppropriationStatus status, Budget budget, BudgetItem budgetItem)
    {
        if (!_dbContext.Budgets.Any(b => b.Id == budget.Id))
        {
            _dbContext.Budgets.Add(budget);
            await _dbContext.SaveChangesAsync();
        }
        if (!_dbContext.BudgetItems.Any(bi => bi.Id == budgetItem.Id))
        {
            _dbContext.BudgetItems.Add(budgetItem);
            await _dbContext.SaveChangesAsync();
        }
        _dbContext.Appropriations.Add(new Appropriation
        {
            Id = id, BudgetItemId = budgetItemId, Amount = amount,
            AppropriationType = type, Status = status, BudgetId = budget.Id,
            AppropriationNumber = $"APP-{id}", DocumentType = "Test", DocumentId = 1
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedEncumbrance(int id, int appropriationId, decimal amount, EncumbranceStatus status, int? reversalOfId = null)
    {
        _dbContext.Encumbrances.Add(new Encumbrance
        {
            Id = id, AppropriationId = appropriationId, Amount = amount,
            Status = status, ReversalOfId = reversalOfId,
            EncumbranceNumber = $"ENC-{id}", EncumbranceType = Domain.Budgeting.Enums.EncumbranceType.Commitment,
            EncumbranceDate = new DateOnly(2026, 6, 1), DocumentType = "Test", DocumentId = 1
        });
        await _dbContext.SaveChangesAsync();
    }

    // ─── T027: Nullifies appropriations and marks encumbrances lapsed ─

    [Test]
    public async Task LapseFiscalYear_ValidRun_ShouldCancelAppropriationsAndEncumbrances()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, TotalAmount = 100000m, Status = Domain.Budgeting.Enums.BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        var budgetItem = new BudgetItem { Id = 100, BudgetId = 1, ItemCode = "ITEM-001", ItemName = "Test Item" };
        await SeedAppropriation(1, 100, 50000m, AppropriationType.Original, AppropriationStatus.Active, budget, budgetItem);
        await SeedEncumbrance(10, 1, 20000m, EncumbranceStatus.Active);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.LapsedAppropriationTotal.ShouldBe(50000m);
        result.Value!.LapsedEncumbranceTotal.ShouldBe(20000m);
        (await _dbContext.FiscalYears.FindAsync(1))!.IsClosed.ShouldBeTrue();
    }

    // ─── T028: Creates YearClosingRun append-only record ──────────────

    [Test]
    public async Task LapseFiscalYear_ShouldCreateYearClosingRun()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, TotalAmount = 100000m, Status = Domain.Budgeting.Enums.BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        var budgetItem = new BudgetItem { Id = 100, BudgetId = 1, ItemCode = "ITEM-001", ItemName = "Test Item" };
        await SeedAppropriation(1, 100, 30000m, AppropriationType.Original, AppropriationStatus.Active, budget, budgetItem);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var runs = await _dbContext.YearClosingRuns.ToListAsync();
        runs.Count.ShouldBe(1);
        runs[0].RunType.ShouldBe(YearClosingRunType.Lapse);
        runs[0].Status.ShouldBe(YearClosingRunStatus.Completed);
    }

    // ─── T029: Idempotent — duplicate run rejected ──────────────────

    [Test]
    public async Task LapseFiscalYear_DuplicateRun_ShouldReturnFailure()
    {
        await SeedFiscalYear(1, "FY2026");
        _dbContext.YearClosingRuns.Add(new YearClosingRun
        {
            FiscalYearId = 1, RunAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 0, LapsedEncumbranceTotal = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already been lapsed"));
    }

    // ─── T030: Fiscal year not found ────────────────────────────────

    [Test]
    public async Task LapseFiscalYear_FiscalYearNotFound_ShouldReturnFailure()
    {
        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    // ─── T034: Year-boundary document splitting ─────────────────────

    [Test]
    public async Task LapseFiscalYear_WithMixedAppropriationTypes_ShouldComputeCorrectTotals()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, TotalAmount = 200000m, Status = Domain.Budgeting.Enums.BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        var budgetItem = new BudgetItem { Id = 100, BudgetId = 1, ItemCode = "ITEM-001", ItemName = "Test Item" };
        await SeedAppropriation(1, 100, 100000m, AppropriationType.Original, AppropriationStatus.Active, budget, budgetItem);
        await SeedAppropriation(2, 100, 20000m, AppropriationType.Supplement, AppropriationStatus.Active, budget, budgetItem);
        await SeedAppropriation(3, 100, 10000m, AppropriationType.Reduction, AppropriationStatus.Active, budget, budgetItem);
        await SeedEncumbrance(10, 1, 30000m, EncumbranceStatus.Active);
        await SeedEncumbrance(11, 2, 5000m, EncumbranceStatus.PartiallyReleased);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        // Original 100000 + Supplement 20000 - Reduction 10000 = 110000
        result.Value!.LapsedAppropriationTotal.ShouldBe(110000m);
        result.Value!.LapsedEncumbranceTotal.ShouldBe(35000m);
    }

    [Test]
    public async Task LapseFiscalYear_WithReversedEncumbrance_ShouldNotCountReversed()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, TotalAmount = 100000m, Status = Domain.Budgeting.Enums.BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        var budgetItem = new BudgetItem { Id = 100, BudgetId = 1, ItemCode = "ITEM-001", ItemName = "Test Item" };
        await SeedAppropriation(1, 100, 50000m, AppropriationType.Original, AppropriationStatus.Active, budget, budgetItem);
        await SeedEncumbrance(10, 1, 20000m, EncumbranceStatus.Active);
        await SeedEncumbrance(11, 1, 10000m, EncumbranceStatus.Cancelled, reversalOfId: 10);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.LapsedEncumbranceTotal.ShouldBe(20000m);
    }
}
