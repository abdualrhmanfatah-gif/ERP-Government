using ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
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
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("BTR-000001");
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

    private async Task SeedBudgetTransactionLine(int budgetItemId, decimal amount, TransactionDirection direction, BudgetTransactionType type, BudgetTransactionStatus status, Budget budget, int budgetTransactionId)
    {
        if (!_dbContext.Budgets.Any(b => b.Id == budget.Id))
        {
            _dbContext.Budgets.Add(budget);
            await _dbContext.SaveChangesAsync();
        }
        if (!_dbContext.BudgetItems.Any(bi => bi.Id == budgetItemId))
        {
            _dbContext.BudgetItems.Add(new BudgetItem { Id = budgetItemId, BudgetId = budget.Id, ItemCode = $"ITEM-{budgetItemId}", ItemName = $"Item {budgetItemId}" });
            await _dbContext.SaveChangesAsync();
        }
        if (!_dbContext.BudgetTransactions.Any(bt => bt.Id == budgetTransactionId))
        {
            _dbContext.BudgetTransactions.Add(new BudgetTransaction
            {
                Id = budgetTransactionId,
                TransactionNumber = $"BTR-{budgetTransactionId}",
                BudgetId = budget.Id,
                TransactionType = type,
                TransactionDate = new DateOnly(2026, 6, 1),
                Status = status,
                PostedAt = DateTimeOffset.UtcNow,
                PostedBy = "1"
            });
            await _dbContext.SaveChangesAsync();
        }
        _dbContext.BudgetTransactionLines.Add(new BudgetTransactionLine
        {
            BudgetTransactionId = budgetTransactionId,
            BudgetItemId = budgetItemId,
            Direction = direction,
            Amount = amount
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedEncumbrance(int id, int budgetItemId, decimal amount, EncumbranceStatus status, int budgetTransactionId, int fiscalYearId, int? reversalOfId = null)
    {
        if (!_dbContext.BudgetTransactions.Any(bt => bt.Id == budgetTransactionId))
        {
            var budget = _dbContext.Budgets.FirstOrDefault(b => b.FiscalYearId == fiscalYearId);
            if (budget == null)
            {
                budget = new Budget { Id = budgetTransactionId / 10, FiscalYearId = fiscalYearId, BudgetNumber = $"B-{budgetTransactionId}", BudgetName = "Test", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
                _dbContext.Budgets.Add(budget);
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.BudgetTransactions.Add(new BudgetTransaction
            {
                Id = budgetTransactionId,
                TransactionNumber = $"BTR-{budgetTransactionId}",
                BudgetId = budget.Id,
                TransactionType = BudgetTransactionType.InitialAppropriation,
                TransactionDate = new DateOnly(2026, 6, 1),
                Status = BudgetTransactionStatus.Posted,
                PostedAt = DateTimeOffset.UtcNow,
                PostedBy = "1"
            });
            await _dbContext.SaveChangesAsync();
        }
        _dbContext.Encumbrances.Add(new Encumbrance
        {
            Id = id, TotalAmount = amount,
            Status = status, ReversalOfId = reversalOfId,
            EncumbranceNumber = $"ENC-{id}", EncumbranceType = EncumbranceType.Commitment,
            EncumbranceDate = new DateOnly(2026, 6, 1),
            DocumentType = "BudgetTransaction", DocumentId = budgetTransactionId
        });
        await _dbContext.SaveChangesAsync();

        if (!_dbContext.BudgetItems.Any(bi => bi.Id == budgetItemId))
        {
            var budget = _dbContext.Budgets.First(b => b.FiscalYearId == fiscalYearId);
            _dbContext.BudgetItems.Add(new BudgetItem { Id = budgetItemId, BudgetId = budget.Id, ItemCode = $"ITEM-{budgetItemId}", ItemName = $"Item {budgetItemId}" });
            await _dbContext.SaveChangesAsync();
        }
        _dbContext.EncumbranceLines.Add(new EncumbranceLine
        {
            EncumbranceId = id, BudgetItemId = budgetItemId,
            Amount = amount
        });
        await _dbContext.SaveChangesAsync();
    }

    // ─── T027: Lapses available budget and marks encumbrances lapsed ─

    [Test]
    public async Task LapseFiscalYear_ValidRun_ShouldCancelEncumbrancesAndComputeTotals()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        await SeedBudgetTransactionLine(100, 50000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, BudgetTransactionStatus.Posted, budget, 1);
        await SeedEncumbrance(10, 100, 20000m, EncumbranceStatus.Active, budgetTransactionId: 2, fiscalYearId: 1);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        // 50000 revised - 0 expenditure - 20000 encumbrance = 30000 available to lapse
        result.Value!.LapsedAppropriationTotal.ShouldBe(30000m);
        result.Value!.LapsedEncumbranceTotal.ShouldBe(20000m);
        (await _dbContext.FiscalYears.FindAsync(1))!.IsClosed.ShouldBeTrue();
    }

    // ─── T028: Creates YearClosingRun append-only record ──────────────

    [Test]
    public async Task LapseFiscalYear_ShouldCreateYearClosingRun()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        await SeedBudgetTransactionLine(100, 30000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, BudgetTransactionStatus.Posted, budget, 1);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
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
            FiscalYearId = 1, StartedAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 0, LapsedEncumbranceTotal = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already been lapsed"));
    }

    // ─── T030: Fiscal year not found ────────────────────────────────

    [Test]
    public async Task LapseFiscalYear_FiscalYearNotFound_ShouldReturnFailure()
    {
        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    // ─── T034: Year-boundary document splitting ─────────────────────

    [Test]
    public async Task LapseFiscalYear_WithMixedTransactionTypes_ShouldComputeCorrectTotals()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        // Increase 100000 (InitialAppropriation) + 20000 (Supplement) - 10000 (Reduction) = 110000 available
        await SeedBudgetTransactionLine(100, 100000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, BudgetTransactionStatus.Posted, budget, 1);
        await SeedBudgetTransactionLine(100, 20000m, TransactionDirection.Increase, BudgetTransactionType.Supplement, BudgetTransactionStatus.Posted, budget, 2);
        await SeedBudgetTransactionLine(100, 10000m, TransactionDirection.Decrease, BudgetTransactionType.Reduction, BudgetTransactionStatus.Posted, budget, 3);
        await SeedEncumbrance(10, 100, 30000m, EncumbranceStatus.Active, budgetTransactionId: 4, fiscalYearId: 1);
        await SeedEncumbrance(11, 100, 5000m, EncumbranceStatus.PartiallyLiquidated, budgetTransactionId: 5, fiscalYearId: 1);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        // (100000 + 20000 - 10000) - 0 expenditure - (30000 + 5000) encumbrance = 75000 available to lapse
        result.Value!.LapsedAppropriationTotal.ShouldBe(75000m);
        result.Value!.LapsedEncumbranceTotal.ShouldBe(35000m);
    }

    [Test]
    public async Task LapseFiscalYear_WithReversedEncumbrance_ShouldNotCountReversed()
    {
        await SeedFiscalYear(1, "FY2026");
        var budget = new Budget { Id = 1, FiscalYearId = 1, BudgetNumber = "B-001", BudgetName = "Test Budget", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        await SeedBudgetTransactionLine(100, 50000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, BudgetTransactionStatus.Posted, budget, 1);
        await SeedEncumbrance(10, 100, 20000m, EncumbranceStatus.Active, budgetTransactionId: 2, fiscalYearId: 1);
        await SeedEncumbrance(11, 100, 10000m, EncumbranceStatus.Cancelled, budgetTransactionId: 3, fiscalYearId: 1, reversalOfId: 10);

        var handler = new LapseFiscalYearCommandHandler(_dbContext, _sequenceServiceMock.Object, _userMock.Object);
        var result = await handler.Handle(new LapseFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.LapsedEncumbranceTotal.ShouldBe(20000m);
    }
}
