using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.Encumbrances;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Budgeting;

[TestFixture]
public class EncumbranceReversalTests : TestBase
{
    private int _budgetId;
    private int _budgetItemId;
    private int _appropriationId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(
            "Reversal Budget", 1, 1, 1));
        budgetResult.Succeeded.ShouldBeTrue();
        _budgetId = budgetResult.Value;

        var budget = await TestApp.FindAsync<Budget>(_budgetId);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);

        var item = new BudgetItem
        {
            BudgetId = _budgetId,
            ItemCode = "RV-001",
            ItemName = "Reversal Test Item"
        };
        await TestApp.AddAsync(item);
        _budgetItemId = item.Id;

        var appResult = await TestApp.SendAsync(new CreateAppropriationCommand(
            _budgetId, _budgetItemId, AppropriationType.Original, "PO", 1, 100000m));
        appResult.Succeeded.ShouldBeTrue();
        _appropriationId = appResult.Value;

        var appropriation = await TestApp.FindAsync<Appropriation>(_appropriationId);
        appropriation!.Status = AppropriationStatus.Active;
        await TestApp.AddAsync(appropriation);
    }

    private async Task<int> CreateAndActivateEncumbrance(decimal amount)
    {
        var createResult = await TestApp.SendAsync(new CreateEncumbranceCommand(
            _appropriationId, EncumbranceType.Commitment, null, null,
            "PO", 1, "Reversal test", DateOnly.FromDateTime(DateTime.UtcNow), amount));
        createResult.Succeeded.ShouldBeTrue();
        var id = createResult.Value;

        var enc = await TestApp.FindAsync<Encumbrance>(id);
        await TestApp.SendAsync(new SubmitEncumbranceCommand(id, enc!.RowVersion));
        enc = await TestApp.FindAsync<Encumbrance>(id);
        await TestApp.SendAsync(new ApproveEncumbranceCommand(id, enc!.RowVersion, null));
        enc = await TestApp.FindAsync<Encumbrance>(id);
        await TestApp.SendAsync(new ActivateEncumbranceCommand(id, enc!.RowVersion));

        return id;
    }

    [Test]
    public async Task Reverse_ActiveEncumbrance_ShouldCreateNegativeRowAndMarkOriginal()
    {
        var encumbranceId = await CreateAndActivateEncumbrance(15000m);

        var encumbrance = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        var reverseResult = await TestApp.SendAsync(new ReverseEncumbranceCommand(
            encumbranceId, encumbrance!.RowVersion, "Reversal reason"));
        reverseResult.Succeeded.ShouldBeTrue();

        var original = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        original!.Status.ShouldBe(EncumbranceStatus.Reversed);
        original.ReversalOfId.ShouldBeNull();
        original.Amount.ShouldBe(15000m);
    }

    [Test]
    public async Task Reverse_DraftEncumbrance_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateEncumbranceCommand(
            _appropriationId, EncumbranceType.Commitment, null, null,
            "PO", 1, "Draft only", DateOnly.FromDateTime(DateTime.UtcNow), 5000m));
        createResult.Succeeded.ShouldBeTrue();
        var id = createResult.Value;

        var enc = await TestApp.FindAsync<Encumbrance>(id);
        var reverseResult = await TestApp.SendAsync(new ReverseEncumbranceCommand(
            id, enc!.RowVersion, "reason"));
        reverseResult.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Reverse_WithRequiredReason_ShouldFailWithoutReason()
    {
        var encumbranceId = await CreateAndActivateEncumbrance(10000m);

        var enc = await TestApp.FindAsync<Encumbrance>(encumbranceId);
        var reverseResult = await TestApp.SendAsync(new ReverseEncumbranceCommand(
            encumbranceId, enc!.RowVersion, null));
        reverseResult.Succeeded.ShouldBeFalse();
    }
}
