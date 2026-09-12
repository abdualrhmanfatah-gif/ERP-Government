using ERP_Government.Application.Budgeting.Commands.FinancialControl.ReopenFiscalYear;
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
public class ReopenFiscalYearTests
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

    private async Task SeedLapsedFiscalYear(int fyId, string name)
    {
        _dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fyId, Name = name, IsClosed = true, YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = Domain.FinancialSettings.Enums.FiscalYearStatus.HardClosed
        });
        _dbContext.YearClosingRuns.Add(new YearClosingRun
        {
            FiscalYearId = fyId, StartedAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 50000m, LapsedEncumbranceTotal = 20000m
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedLapseTransaction(int budgetItemId, decimal amount, int fyId)
    {
        var budget = new Budget { Id = budgetItemId / 100, FiscalYearId = fyId, BudgetNumber = $"B-{budgetItemId}", BudgetName = "Test", BudgetTypeId = 1, FundId = 1, Status = BudgetStatus.Active, EffectiveFrom = new DateOnly(2026, 1, 1) };
        var budgetItem = new BudgetItem { Id = budgetItemId, BudgetId = budgetItemId / 100, ItemCode = $"ITEM-{budgetItemId}", ItemName = "Test" };
        _dbContext.Budgets.Add(budget);
        _dbContext.BudgetItems.Add(budgetItem);

        var transaction = new BudgetTransaction
        {
            TransactionNumber = $"BTR-{budgetItemId}",
            BudgetId = budgetItemId / 100,
            TransactionType = BudgetTransactionType.Lapse,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Lapse transaction",
            Status = BudgetTransactionStatus.Posted,
            PostedAt = DateTimeOffset.UtcNow,
            PostedBy = "1"
        };
        _dbContext.BudgetTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        _dbContext.BudgetTransactionLines.Add(new BudgetTransactionLine
        {
            BudgetTransactionId = transaction.Id,
            BudgetItemId = budgetItemId,
            Direction = TransactionDirection.Decrease,
            Amount = amount,
            Remarks = "Lapse"
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedCancelledEncumbrance(int id, int budgetItemId, decimal amount)
    {
        _dbContext.Encumbrances.Add(new Encumbrance
        {
            Id = id, TotalAmount = amount,
            Status = EncumbranceStatus.Cancelled, EncumbranceNumber = $"ENC-{id}",
            EncumbranceType = EncumbranceType.Commitment, EncumbranceDate = new DateOnly(2026, 6, 1),
            DocumentType = "BudgetTransaction", DocumentId = 1
        });
        await _dbContext.SaveChangesAsync();
    }

    // ─── T031: Restores amounts when no payments against lapsed items ─

    [Test]
    public async Task ReopenFiscalYear_NoPaymentsAgainstLapsed_ShouldRestoreAmounts()
    {
        await SeedLapsedFiscalYear(1, "FY2026");
        await SeedLapseTransaction(100, 50000m, 1);
        await SeedCancelledEncumbrance(10, 100, 20000m);

        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.RestoredBudgetTotal.ShouldBe(50000m);
        result.Value!.RestoredEncumbranceTotal.ShouldBe(20000m);
        (await _dbContext.FiscalYears.FindAsync(1))!.IsClosed.ShouldBeFalse();
    }

    // ─── T032: Blocked when payments exist against lapsed items ──────

    [Test]
    public async Task ReopenFiscalYear_FinalAccountIssued_ShouldReturnFailure()
    {
        await SeedLapsedFiscalYear(1, "FY2026");
        _dbContext.FinalAccounts.Add(new FinalAccount
        {
            FiscalYearId = 1, Status = FinalAccountStatus.Issued,
            GeneratedAt = DateTimeOffset.UtcNow, GeneratedById = 1,
            IssuedAt = DateTimeOffset.UtcNow, IssuedById = 1
        });
        await _dbContext.SaveChangesAsync();

        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("final account has been issued"));
    }

    // ─── T033: Blocked when final account issued ────────────────────

    [Test]
    public async Task ReopenFiscalYear_NotLapsed_ShouldReturnFailure()
    {
        _dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = 1, Name = "FY2026", IsClosed = false, YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        });
        await _dbContext.SaveChangesAsync();

        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("has not been lapsed"));
    }

    [Test]
    public async Task ReopenFiscalYear_FiscalYearNotFound_ShouldReturnFailure()
    {
        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task ReopenFiscalYear_ShouldMarkLapsedRunAsReversed()
    {
        await SeedLapsedFiscalYear(1, "FY2026");
        await SeedLapseTransaction(100, 30000m, 1);

        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var lapsedRun = await _dbContext.YearClosingRuns.FirstAsync(r => r.RunType == YearClosingRunType.Lapse);
        lapsedRun.Status.ShouldBe(YearClosingRunStatus.Reversed);
    }

    [Test]
    public async Task ReopenFiscalYear_ShouldCreateReopenRun()
    {
        await SeedLapsedFiscalYear(1, "FY2026");
        await SeedLapseTransaction(100, 40000m, 1);

        var handler = new ReopenFiscalYearCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new ReopenFiscalYearCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var runs = await _dbContext.YearClosingRuns.Where(r => r.RunType == YearClosingRunType.Reopen).ToListAsync();
        runs.Count.ShouldBe(1);
        runs[0].Status.ShouldBe(YearClosingRunStatus.Completed);
    }
}
