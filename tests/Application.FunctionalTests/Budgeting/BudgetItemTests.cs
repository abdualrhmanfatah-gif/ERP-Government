using ERP_Government.Application.Budgeting.Commands.BudgetItems;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Queries.BudgetItems;
using ERP_Government.Application.Budgeting.Queries.Budgets;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class BudgetItemTests : TestBase
{
    [Test]
    public async Task CreateBudgetItem_ShouldReturnCreated()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Item Test Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        var budgetId = budgetResult.Value;

        var itemResult = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "PS-001", "Personnel Services", null, null));
        itemResult.Succeeded.ShouldBeTrue();
        itemResult.Value.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task CreateBudgetItem_WithParent_ShouldLinkCorrectly()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Parent Link Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        var rootResult = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "ROOT-001", "Root Item", null, null));
        rootResult.Succeeded.ShouldBeTrue();

        var childResult = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "CHILD-001", "Child Item", rootResult.Value, null));
        childResult.Succeeded.ShouldBeTrue();

        var tree = await TestApp.SendAsync(new GetBudgetItemsTreeQuery(budgetId));
        tree.Count.ShouldBe(1);
        tree[0].Children.Count.ShouldBe(1);
        tree[0].Children[0].ItemCode.ShouldBe("CHILD-001");
    }

    [Test]
    public async Task GetBudgetItemsTree_ShouldComputeLevels()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Tree Level Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        // Level 0: root
        var root = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "L0-001", "Level 0", null, null));
        // Level 1: child of root
        var l1 = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "L1-001", "Level 1", root.Value, null));
        // Level 2: child of level 1
        var l2 = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "L2-001", "Level 2", l1.Value, null));

        var tree = await TestApp.SendAsync(new GetBudgetItemsTreeQuery(budgetId));
        tree.Count.ShouldBe(1);
        tree[0].Level.ShouldBe(0);
        tree[0].Children[0].Level.ShouldBe(1);
        tree[0].Children[0].Children[0].Level.ShouldBe(2);
    }

    [Test]
    public async Task AllowOverrun_Inheritance_ShouldResolve()
    {
        await TestApp.RunAsAdministratorAsync();

        // BudgetType with AllowOverrun = true (need to create via AddAsync since no toggle command for test setup)
        var budgetType = new ERP_Government.Domain.Budgeting.Entities.BudgetType
        {
            Code = "AO-TEST",
            Name = "AllowOverrun Test Type",
            ControlMethod = ERP_Government.Domain.Budgeting.Enums.BudgetControlMethod.None,
            AllowOverrun = true,
            IsActive = true
        };
        await TestApp.AddAsync(budgetType);

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "AllowOverrun Budget", 1, 1, budgetType.Id));
        var budgetId = budgetResult.Value;

        // Item with AllowOverrun = null -> should inherit from BudgetType
        var itemResult = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "AO-001", "Inherit Item", null, null));

        var tree = await TestApp.SendAsync(new GetBudgetItemsTreeQuery(budgetId));
        tree.Count.ShouldBe(1);
        tree[0].AllowOverrunEffective.ShouldBeTrue();
    }

    [Test]
    public async Task AllowOverrun_ItemLevelOverride_ShouldUseItemValue()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetType = new ERP_Government.Domain.Budgeting.Entities.BudgetType
        {
            Code = "AO-OVR",
            Name = "Override Test Type",
            ControlMethod = ERP_Government.Domain.Budgeting.Enums.BudgetControlMethod.None,
            AllowOverrun = true,
            IsActive = true
        };
        await TestApp.AddAsync(budgetType);

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Override Budget", 1, 1, budgetType.Id));
        var budgetId = budgetResult.Value;

        // Item with AllowOverrun = false -> should use item value, not inherit
        var item = new ERP_Government.Domain.Budgeting.Entities.BudgetItem
        {
            BudgetId = budgetId,
            ItemCode = "OVR-001",
            ItemName = "Override Item",
            AllowOverrun = false,
            IsActive = true
        };
        await TestApp.AddAsync(item);

        var tree = await TestApp.SendAsync(new GetBudgetItemsTreeQuery(budgetId));
        tree.Count.ShouldBe(1);
        tree[0].AllowOverrunEffective.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteBudgetItem_WithChildren_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Delete Test Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        var root = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "DEL-001", "Root", null, null));
        await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "DEL-002", "Child", root.Value, null));

        var deleteResult = await TestApp.SendAsync(new DeleteBudgetItemCommand(root.Value));
        deleteResult.Succeeded.ShouldBeFalse();
        deleteResult.Errors.ShouldContain(e => e.Contains("has children"));
    }

    [Test]
    public async Task MoveBudgetItem_ShouldUpdateParent()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Move Test Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        var parentA = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "MOV-A", "Parent A", null, null));
        var parentB = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "MOV-B", "Parent B", null, null));
        var child = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "MOV-C", "Child", parentA.Value, null));

        // Get the child's RowVersion
        var items = await TestApp.SendAsync(new GetBudgetItemsListQuery(budgetId));
        var childItem = items.First(x => x.Id == child.Value);

        var moveResult = await TestApp.SendAsync(new MoveBudgetItemCommand(
            child.Value, parentB.Value, childItem.RowVersion));
        moveResult.Succeeded.ShouldBeTrue();

        var tree = await TestApp.SendAsync(new GetBudgetItemsTreeQuery(budgetId));
        tree.Count.ShouldBe(2);
        tree.First(x => x.ItemCode == "MOV-B").Children.Count.ShouldBe(1);
        tree.First(x => x.ItemCode == "MOV-A").Children.Count.ShouldBe(0);
    }

    [Test]
    public async Task MoveBudgetItem_CircularReference_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Circular Test Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        var parent = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "CIRC-P", "Parent", null, null));
        var child = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "CIRC-C", "Child", parent.Value, null));

        // Get the child's RowVersion
        var items = await TestApp.SendAsync(new GetBudgetItemsListQuery(budgetId));
        var childItem = items.First(x => x.Id == child.Value);

        // Try to move parent under child -> circular
        var moveResult = await TestApp.SendAsync(new MoveBudgetItemCommand(
            parent.Value, child.Value, childItem.RowVersion));
        moveResult.Succeeded.ShouldBeFalse();
        moveResult.Errors.ShouldContain(e => e.Contains("Cannot move an item to be its own parent"));
    }

    [Test]
    public async Task DeleteBudgetItem_NonDraftBudget_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Non-Draft Delete Budget", 1, 1, 1));
        var budgetId = budgetResult.Value;

        var item = await TestApp.SendAsync(new CreateBudgetItemCommand(
            budgetId, "DEL-ND", "Non-Draft Item", null, null));

        // Submit budget to move out of Draft
        var budget = await TestApp.SendAsync(new GetBudgetByIdQuery(budgetId));
        await TestApp.SendAsync(new SubmitBudgetCommand(budgetId, budget.RowVersion));

        var deleteResult = await TestApp.SendAsync(new DeleteBudgetItemCommand(item.Value));
        deleteResult.Succeeded.ShouldBeFalse();
        deleteResult.Errors.ShouldContain(e => e.Contains("Only items in Draft budgets can be deleted"));
    }
}
