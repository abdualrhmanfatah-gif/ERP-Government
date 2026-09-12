using ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.IssueFinalAccount;
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
public class GenerateFinalAccountTests
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

    private async Task SeedClosedFiscalYear(int fyId, string name)
    {
        _dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fyId, Name = name, IsClosed = true, YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = Domain.FinancialSettings.Enums.FiscalYearStatus.HardClosed
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedBudgetTransactionLine(int budgetItemId, decimal amount, TransactionDirection direction, BudgetTransactionType type, int fyId, string itemCode, int budgetTransactionId)
    {
        if (!_dbContext.Budgets.Any(b => b.Id == fyId))
        {
            _dbContext.Budgets.Add(new Budget
            {
                Id = fyId, FiscalYearId = fyId, BudgetNumber = $"B-{fyId}",
                BudgetName = "Test", BudgetTypeId = 1, FundId = 1,
                Status = Domain.Budgeting.Enums.BudgetStatus.Active,
                EffectiveFrom = new DateOnly(2026, 1, 1)
            });
        }
        if (!_dbContext.BudgetItems.Any(bi => bi.Id == budgetItemId))
        {
            _dbContext.BudgetItems.Add(new BudgetItem
            {
                Id = budgetItemId, BudgetId = fyId, ItemCode = itemCode, ItemName = itemCode
            });
        }
        if (!_dbContext.BudgetTransactions.Any(bt => bt.Id == budgetTransactionId))
        {
            _dbContext.BudgetTransactions.Add(new BudgetTransaction
            {
                Id = budgetTransactionId,
                TransactionNumber = $"BTR-{budgetTransactionId}",
                BudgetId = fyId,
                TransactionType = type,
                TransactionDate = new DateOnly(2026, 6, 1),
                Status = BudgetTransactionStatus.Posted,
                PostedAt = DateTimeOffset.UtcNow,
                PostedBy = "1"
            });
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

    // ─── T040: Creates FinalAccount with Draft status ────────────────

    [Test]
    public async Task GenerateFinalAccount_ValidRequest_ShouldCreateDraftStatus()
    {
        await SeedClosedFiscalYear(1, "FY2026");
        await SeedBudgetTransactionLine(100, 50000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, 1, "ITEM-001", 1);
        _dbContext.YearClosingRuns.Add(new YearClosingRun
        {
            FiscalYearId = 1, StartedAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 0, LapsedEncumbranceTotal = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var account = await _dbContext.FinalAccounts.FirstAsync();
        account.Status.ShouldBe(FinalAccountStatus.Draft);
    }

    // ─── T041: Creates closing entries (balanced journal entries) ─────

    [Test]
    public async Task GenerateFinalAccount_ShouldCreateFinalAccountLines()
    {
        await SeedClosedFiscalYear(1, "FY2026");
        // Both transactions on same BudgetItemId=100 → 1 line in final account
        await SeedBudgetTransactionLine(100, 50000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, 1, "ITEM-001", 1);
        await SeedBudgetTransactionLine(100, 10000m, TransactionDirection.Increase, BudgetTransactionType.Supplement, 1, "ITEM-001", 2);
        _dbContext.YearClosingRuns.Add(new YearClosingRun
        {
            FiscalYearId = 1, StartedAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 0, LapsedEncumbranceTotal = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.LineCount.ShouldBe(1);
    }

    // ─── T042: Creates FinalAccountLines with correct budget-vs-actual ─

    [Test]
    public async Task GenerateFinalAccount_ShouldComputeCorrectBudgetedAmount()
    {
        await SeedClosedFiscalYear(1, "FY2026");
        await SeedBudgetTransactionLine(100, 80000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, 1, "ITEM-A", 1);
        await SeedBudgetTransactionLine(200, 45000m, TransactionDirection.Increase, BudgetTransactionType.InitialAppropriation, 1, "ITEM-B", 2);
        await SeedBudgetTransactionLine(200, 5000m, TransactionDirection.Decrease, BudgetTransactionType.Reduction, 1, "ITEM-B", 3);
        _dbContext.YearClosingRuns.Add(new YearClosingRun
        {
            FiscalYearId = 1, StartedAt = DateTimeOffset.UtcNow, RunById = 1,
            RunType = YearClosingRunType.Lapse, Status = YearClosingRunStatus.Completed,
            LapsedAppropriationTotal = 0, LapsedEncumbranceTotal = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.LineCount.ShouldBe(2);

        var lines = await _dbContext.FinalAccountLines.ToListAsync();
        lines.Count.ShouldBe(2);
    }

    // ─── T043: Rejected when year not lapsed ────────────────────────

    [Test]
    public async Task GenerateFinalAccount_YearNotClosed_ShouldReturnFailure()
    {
        _dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = 1, Name = "FY2026", IsClosed = false, YearNumber = 2026,
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31),
            Status = Domain.FinancialSettings.Enums.FiscalYearStatus.Open
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("must be closed"));
    }

    [Test]
    public async Task GenerateFinalAccount_FiscalYearNotFound_ShouldReturnFailure()
    {
        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task GenerateFinalAccount_AlreadyExists_ShouldReturnFailure()
    {
        await SeedClosedFiscalYear(1, "FY2026");
        _dbContext.FinalAccounts.Add(new FinalAccount
        {
            FiscalYearId = 1, Status = FinalAccountStatus.Draft,
            GeneratedAt = DateTimeOffset.UtcNow, GeneratedById = 1
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GenerateFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new GenerateFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    // ─── T044: IssueFinalAccount transitions to Issued ──────────────

    [Test]
    public async Task IssueFinalAccount_DraftAccount_ShouldTransitionToIssued()
    {
        _dbContext.FinalAccounts.Add(new FinalAccount
        {
            Id = 1, FiscalYearId = 1, Status = FinalAccountStatus.Draft,
            GeneratedAt = DateTimeOffset.UtcNow, GeneratedById = 1
        });
        await _dbContext.SaveChangesAsync();

        var handler = new IssueFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new IssueFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var account = await _dbContext.FinalAccounts.FindAsync(1);
        account!.Status.ShouldBe(FinalAccountStatus.Issued);
        account.IssuedAt.ShouldNotBeNull();
        account.IssuedById.ShouldBe(1);
    }

    [Test]
    public async Task IssueFinalAccount_AlreadyIssued_ShouldReturnFailure()
    {
        _dbContext.FinalAccounts.Add(new FinalAccount
        {
            Id = 1, FiscalYearId = 1, Status = FinalAccountStatus.Issued,
            GeneratedAt = DateTimeOffset.UtcNow, GeneratedById = 1
        });
        await _dbContext.SaveChangesAsync();

        var handler = new IssueFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new IssueFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already been issued"));
    }

    [Test]
    public async Task IssueFinalAccount_NotFound_ShouldReturnFailure()
    {
        var handler = new IssueFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new IssueFinalAccountCommand(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    // ─── T045: IssueFinalAccount prevents reopening of fiscal year ────

    [Test]
    public async Task IssueFinalAccount_ShouldPreventFiscalYearReopening()
    {
        _dbContext.FinalAccounts.Add(new FinalAccount
        {
            Id = 1, FiscalYearId = 1, Status = FinalAccountStatus.Draft,
            GeneratedAt = DateTimeOffset.UtcNow, GeneratedById = 1
        });
        await _dbContext.SaveChangesAsync();

        var handler = new IssueFinalAccountCommandHandler(_dbContext, _userMock.Object);
        var result = await handler.Handle(new IssueFinalAccountCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var account = await _dbContext.FinalAccounts.FindAsync(1);
        account!.Status.ShouldBe(FinalAccountStatus.Issued);
    }
}
