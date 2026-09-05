using ERP_Government.Application.Budgeting.Commands.BudgetItems;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Application.UnitTests.Accounting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class BudgetItemCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    private void WireContext(
        Mock<DbSet<BudgetItem>> budgetItems = null!,
        Mock<DbSet<Budget>> budgets = null!,
        Mock<DbSet<Appropriation>> appropriations = null!)
    {
        if (budgetItems != null) _contextMock.Setup(x => x.BudgetItems).Returns(budgetItems.Object);
        if (budgets != null) _contextMock.Setup(x => x.Budgets).Returns(budgets.Object);
        if (appropriations != null) _contextMock.Setup(x => x.Appropriations).Returns(appropriations.Object);
    }

    private static void SetupBudgetItemFindAsync(Mock<DbSet<BudgetItem>> mockSet, List<BudgetItem> data)
    {
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var id = (int)keyValues[0];
                var entity = data.FirstOrDefault(e => e.Id == id);
                return ValueTask.FromResult(entity);
            });
    }

    private static void SetupBudgetFindAsync(Mock<DbSet<Budget>> mockSet, List<Budget> data)
    {
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var id = (int)keyValues[0];
                var entity = data.FirstOrDefault(e => e.Id == id);
                return ValueTask.FromResult(entity);
            });
    }

    // ─── CreateBudgetItem ──────────────────────────────────────────

    [Test]
    public async Task CreateBudgetItem_ValidCommand_ShouldCreateAndReturnId()
    {
        var budgetItems = new List<BudgetItem>().AsQueryable().BuildMockForAsync();
        var budgets = new List<Budget> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new CreateBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new CreateBudgetItemCommand(1, "PS-001", "Personnel Services", null, null), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budgetItems.Verify(x => x.Add(It.IsAny<BudgetItem>()), Times.Once);
    }

    [Test]
    public async Task CreateBudgetItem_DuplicateCode_ShouldReturnFailure()
    {
        var budgetItems = new List<BudgetItem> { new() { Id = 1, BudgetId = 1, ItemCode = "PS-001" } }.AsQueryable().BuildMockForAsync();
        var budgets = new List<Budget> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new CreateBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new CreateBudgetItemCommand(1, "PS-001", "Duplicate", null, null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task CreateBudgetItem_InvalidBudget_ShouldReturnFailure()
    {
        var budgetItems = new List<BudgetItem>().AsQueryable().BuildMockForAsync();
        var budgets = new List<Budget>().AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new CreateBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new CreateBudgetItemCommand(999, "PS-001", "Item", null, null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Budget not found"));
    }

    [Test]
    public async Task CreateBudgetItem_InvalidParent_ShouldReturnFailure()
    {
        var budgetItems = new List<BudgetItem>().AsQueryable().BuildMockForAsync();
        var budgets = new List<Budget> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new CreateBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new CreateBudgetItemCommand(1, "PS-001", "Item", 999, null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Parent budget item not found"));
    }

    [Test]
    public async Task CreateBudgetItem_ParentFromDifferentBudget_ShouldReturnFailure()
    {
        var budgetItems = new List<BudgetItem> { new() { Id = 10, BudgetId = 2, ItemCode = "X-001" } }.AsQueryable().BuildMockForAsync();
        var budgets = new List<Budget> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new CreateBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new CreateBudgetItemCommand(1, "PS-001", "Item", 10, null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Parent budget item not found in the same budget"));
    }

    // ─── DeleteBudgetItem ──────────────────────────────────────────

    [Test]
    public async Task DeleteBudgetItem_DraftBudget_ShouldSucceed()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1 };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        var appropriations = new List<Appropriation>().AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets, appropriations: appropriations);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new DeleteBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new DeleteBudgetItemCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        budgetItems.Verify(x => x.Remove(item), Times.Once);
    }

    [Test]
    public async Task DeleteBudgetItem_NonDraftBudget_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1 };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new DeleteBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new DeleteBudgetItemCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only items in Draft budgets can be deleted"));
    }

    [Test]
    public async Task DeleteBudgetItem_WithChildren_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1 };
        var child = new BudgetItem { Id = 2, BudgetId = 1, ParentId = 1 };
        var budgetItems = new List<BudgetItem> { item, child }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item, child });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new DeleteBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new DeleteBudgetItemCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("has children"));
    }

    [Test]
    public async Task DeleteBudgetItem_WithAppropriations_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1 };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        var appropriations = new List<Appropriation> { new() { Id = 1, BudgetItemId = 1 } }.AsQueryable().BuildMockForAsync();
        WireContext(budgetItems: budgetItems, budgets: budgets, appropriations: appropriations);

        var result = await new DeleteBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new DeleteBudgetItemCommand(1), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("has appropriations"));
    }

    // ─── MoveBudgetItem ────────────────────────────────────────────

    [Test]
    public async Task MoveBudgetItem_ValidMove_ShouldSucceed()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1, ParentId = null, RowVersion = [1, 2] };
        var newParent = new BudgetItem { Id = 2, BudgetId = 1 };
        var allItems = new List<BudgetItem> { item, newParent };
        var budgetItems = allItems.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, allItems);

        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new MoveBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new MoveBudgetItemCommand(1, 2, [1, 2]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        item.ParentId.ShouldBe(2);
    }

    [Test]
    public async Task MoveBudgetItem_NonDraftBudget_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1, RowVersion = [1, 2] };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Active };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new MoveBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new MoveBudgetItemCommand(1, null, [1, 2]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only items in Draft budgets can be moved"));
    }

    [Test]
    public async Task MoveBudgetItem_ToSelf_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1, RowVersion = [1, 2] };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new MoveBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new MoveBudgetItemCommand(1, 1, [1, 2]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Cannot move an item to be its own parent"));
    }

    [Test]
    public async Task MoveBudgetItem_NewParentNotFound_ShouldReturnFailure()
    {
        var item = new BudgetItem { Id = 1, BudgetId = 1, RowVersion = [1, 2] };
        var budgetItems = new List<BudgetItem> { item }.AsQueryable().BuildMockForAsync();
        SetupBudgetItemFindAsync(budgetItems, new List<BudgetItem> { item });
        var budget = new Budget { Id = 1, Status = BudgetStatus.Draft };
        var budgets = new List<Budget> { budget }.AsQueryable().BuildMockForAsync();
        SetupBudgetFindAsync(budgets, new List<Budget> { budget });
        WireContext(budgetItems: budgetItems, budgets: budgets);

        var result = await new MoveBudgetItemCommandHandler(_contextMock.Object)
            .Handle(new MoveBudgetItemCommand(1, 999, [1, 2]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("New parent budget item not found"));
    }

    // ─── Validators ────────────────────────────────────────────────

    [Test]
    public async Task CreateItemValidator_EmptyItemCode_ShouldHaveError()
    {
        var result = await new CreateBudgetItemCommandValidator().ValidateAsync(new CreateBudgetItemCommand(1, "", "Name", null, null));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ItemCode");
    }

    [Test]
    public async Task CreateItemValidator_EmptyItemName_ShouldHaveError()
    {
        var result = await new CreateBudgetItemCommandValidator().ValidateAsync(new CreateBudgetItemCommand(1, "CODE", "", null, null));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ItemName");
    }
}
