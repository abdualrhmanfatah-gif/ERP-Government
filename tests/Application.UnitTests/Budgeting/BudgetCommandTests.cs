using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Security.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class BudgetCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _currentUserMock = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private Mock<IAttachmentGateService> _attachmentGateMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserMock = new Mock<IUser>();
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _statusLoggerMock = new Mock<IDocumentStatusLogger>();
        _attachmentGateMock = new Mock<IAttachmentGateService>();
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("BUD-000001");
        _currentUserMock.Setup(x => x.Id).Returns(1);
    }

    private void WireContext(
        Mock<DbSet<Budget>> budgets = null!,
        Mock<DbSet<FiscalYear>> fiscalYears = null!,
        Mock<DbSet<Fund>> funds = null!,
        Mock<DbSet<BudgetType>> budgetTypes = null!,
        Mock<DbSet<ApprovalHistory>> approvalHistory = null!)
    {
        if (budgets != null) _contextMock.Setup(x => x.Budgets).Returns(budgets.Object);
        if (fiscalYears != null) _contextMock.Setup(x => x.FiscalYears).Returns(fiscalYears.Object);
        if (funds != null) _contextMock.Setup(x => x.Funds).Returns(funds.Object);
        if (budgetTypes != null) _contextMock.Setup(x => x.BudgetTypes).Returns(budgetTypes.Object);
        if (approvalHistory != null) _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);

        var emptyBudgetTransactions = new List<BudgetTransaction>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetTransactions).Returns(emptyBudgetTransactions.Object);

        var emptyEncumbranceLines = new List<EncumbranceLine>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.EncumbranceLines).Returns(emptyEncumbranceLines.Object);

        var emptyAllocations = new List<BudgetItemAllocation>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetItemAllocations).Returns(emptyAllocations.Object);
    }

    private static void SetupFindAsync(Mock<DbSet<Budget>> mockSet, List<Budget> data)
    {
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var id = (int)keyValues[0];
                var entity = data.FirstOrDefault(e => e.Id == id);
                return ValueTask.FromResult(entity);
            });
    }

    // ─── CreateBudget ──────────────────────────────────────────────

    [Test]
    public async Task CreateBudget_ValidCommand_ShouldCreateAndReturnId()
    {
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        var fiscalYears = new List<FiscalYear> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var funds = new List<Fund> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var budgetTypes = new List<BudgetType> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgets, fiscalYears, funds, budgetTypes);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CreateBudgetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object)
            .Handle(new CreateBudgetCommand("General Budget", 1, 1, 1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budgets.Verify(x => x.Add(It.IsAny<Budget>()), Times.Once);
    }

    [Test]
    public async Task CreateBudget_InvalidFiscalYear_ShouldReturnFailure()
    {
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        var fiscalYears = new List<FiscalYear>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, fiscalYears: fiscalYears);

        var result = await new CreateBudgetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object)
            .Handle(new CreateBudgetCommand("Budget", 999, 1, 1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Fiscal year not found"));
    }

    [Test]
    public async Task CreateBudget_InvalidFund_ShouldReturnFailure()
    {
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        var fiscalYears = new List<FiscalYear> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var funds = new List<Fund>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, fiscalYears: fiscalYears, funds: funds);

        var result = await new CreateBudgetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object)
            .Handle(new CreateBudgetCommand("Budget", 1, 999, 1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Fund not found"));
    }

    [Test]
    public async Task CreateBudget_InvalidBudgetType_ShouldReturnFailure()
    {
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        var fiscalYears = new List<FiscalYear> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var funds = new List<Fund> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var budgetTypes = new List<BudgetType>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, fiscalYears: fiscalYears, funds: funds, budgetTypes: budgetTypes);

        var result = await new CreateBudgetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object)
            .Handle(new CreateBudgetCommand("Budget", 1, 1, 999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Budget type not found"));
    }

    // ─── UpdateBudget ──────────────────────────────────────────────

    [Test]
    public async Task UpdateBudget_DraftBudget_ShouldSucceed()
    {
        var budget = new Budget { Id = 1, BudgetNumber = "OLD-001", BudgetName = "Old Name", FiscalYearId = 1, FundId = 1, BudgetTypeId = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var fiscalYears = new List<FiscalYear> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var funds = new List<Fund> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var budgetTypes = new List<BudgetType> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgets, fiscalYears, funds, budgetTypes);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new UpdateBudgetCommandHandler(_contextMock.Object)
            .Handle(new UpdateBudgetCommand(1, "NEW-001", "New Name", 1, 1, 1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task UpdateBudget_NonDraftBudget_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new UpdateBudgetCommandHandler(_contextMock.Object)
            .Handle(new UpdateBudgetCommand(1, "NEW-001", "New Name", 1, 1, 1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Draft budgets can be updated"));
    }

    // ─── FSM Transitions ───────────────────────────────────────────

    [Test]
    public async Task SubmitDraft_ShouldTransitionToSubmitted()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new SubmitBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new SubmitBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Submitted);
    }

    [Test]
    public async Task SubmitNonDraft_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Submitted, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new SubmitBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new SubmitBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Draft budgets can be submitted"));
    }

    [Test]
    public async Task ApproveSubmitted_ShouldTransitionToApproved()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Submitted, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new ApproveBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object, _attachmentGateMock.Object)
            .Handle(new ApproveBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Approved);
    }

    [Test]
    public async Task ApproveDraft_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new ApproveBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object, _attachmentGateMock.Object)
            .Handle(new ApproveBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Submitted budgets can be approved"));
    }

    [Test]
    public async Task ActivateApproved_ShouldTransitionToActive()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Approved, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new ActivateBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new ActivateBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Active);
    }

    [Test]
    public async Task ActivateDraft_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new ActivateBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new ActivateBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Approved budgets can be activated"));
    }

    [Test]
    public async Task SuspendActive_ShouldTransitionToSuspended()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new SuspendBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new SuspendBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Suspended);
    }

    [Test]
    public async Task SuspendDraft_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new SuspendBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new SuspendBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Active budgets can be suspended"));
    }

    [Test]
    public async Task CloseActive_ShouldTransitionToClosed()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CloseBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CloseBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Closed);
    }

    [Test]
    public async Task CloseSuspended_ShouldTransitionToClosed()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Suspended, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CloseBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CloseBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Closed);
    }

    [Test]
    public async Task CloseDraft_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new CloseBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CloseBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Active or Suspended budgets can be closed"));
    }

    [Test]
    public async Task CancelDraft_ShouldTransitionToCancelled()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CancelBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CancelBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task CancelSubmitted_ShouldTransitionToCancelled()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Submitted, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CancelBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CancelBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task CancelSuspended_ShouldTransitionToCancelled()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Suspended, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        WireContext(budgets: budgets, approvalHistory: approvalHistory);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CancelBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CancelBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budget.Status.ShouldBe(BudgetStatus.Cancelled);
    }

    [Test]
    public async Task CancelActive_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active, RowVersion = [1, 2, 3] };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgets: budgets);

        var result = await new CancelBudgetCommandHandler(_contextMock.Object, _currentUserMock.Object, _statusLoggerMock.Object)
            .Handle(new CancelBudgetCommand(1, [1, 2, 3]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only Draft, Submitted, or Suspended budgets can be cancelled"));
    }

    // ─── US1: Number Assignment ──────────────────────────────────────

    [Test]
    public async Task CreateBudget_ShouldCallSequenceServiceForBudgetNumber()
    {
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        var fiscalYears = new List<FiscalYear> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var funds = new List<Fund> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        var budgetTypes = new List<BudgetType> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgets, fiscalYears, funds, budgetTypes);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Budget", It.IsAny<CancellationToken>()))
            .ReturnsAsync("BGT-000042");

        var result = await new CreateBudgetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object)
            .Handle(new CreateBudgetCommand("Test Budget", 1, 1, 1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _sequenceServiceMock.Verify(s => s.GenerateNextNumberAsync("Budget", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateBudgetCommand_ShouldNotExposeBudgetNumber()
    {
        var command = new CreateBudgetCommand("Budget", 1, 1, 1);

        var property = typeof(CreateBudgetCommand).GetProperty("BudgetNumber");
        property.ShouldBeNull();
    }

    [Test]
    public void CreateBudgetCommandValidator_ShouldNotHaveBudgetNumberRule()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("Budget", 1, 1, 1));

        result.IsValid.ShouldBeTrue();

        result.Errors.ShouldNotContain(e => e.PropertyName == "BudgetNumber");
    }

    // ─── Validators ────────────────────────────────────────────────

    [Test]
    public async Task CreateValidator_EmptyBudgetName_ShouldHaveError()
    {
        var result = await new CreateBudgetCommandValidator().ValidateAsync(new CreateBudgetCommand("", 1, 1, 1));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "BudgetName");
    }

    [Test]
    public async Task CreateValidator_InvalidFiscalYear_ShouldHaveError()
    {
        var result = await new CreateBudgetCommandValidator().ValidateAsync(new CreateBudgetCommand("Budget", 0, 1, 1));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FiscalYearId");
    }

    [Test]
    public async Task CreateValidator_InvalidFund_ShouldHaveError()
    {
        var result = await new CreateBudgetCommandValidator().ValidateAsync(new CreateBudgetCommand("Budget", 1, 0, 1));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FundId");
    }

    [Test]
    public async Task CreateValidator_InvalidBudgetType_ShouldHaveError()
    {
        var result = await new CreateBudgetCommandValidator().ValidateAsync(new CreateBudgetCommand("Budget", 1, 1, 0));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "BudgetTypeId");
    }
}
