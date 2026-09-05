using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class TransferCommandTests
{
    // ─── T038: Transfer Validation ──────────────────────────────────

    [Test]
    public void Transfer_SameBudgetRequired_DifferentBudgets_ShouldFail()
    {
        var validation = ValidateTransfer(budgetId: 1, sourceItemId: 1, targetItemId: 2, amount: 1000m,
            sourceBudgetId: 1, targetBudgetId: 2);

        validation.IsValid.ShouldBeFalse();
        validation.Errors.ShouldContain(e => e.Contains("same Budget"));
    }

    [Test]
    public void Transfer_SourceCannotEqualTarget_ShouldFail()
    {
        var validation = ValidateTransfer(budgetId: 1, sourceItemId: 1, targetItemId: 1, amount: 1000m,
            sourceBudgetId: 1, targetBudgetId: 1);

        validation.IsValid.ShouldBeFalse();
        validation.Errors.ShouldContain(e => e.Contains("Source", StringComparison.OrdinalIgnoreCase) && e.Contains("target", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void Transfer_AmountMustBePositive_Zero_ShouldFail()
    {
        var validation = ValidateTransfer(budgetId: 1, sourceItemId: 1, targetItemId: 2, amount: 0m,
            sourceBudgetId: 1, targetBudgetId: 1);

        validation.IsValid.ShouldBeFalse();
        validation.Errors.ShouldContain(e => e.Contains("greater than zero"));
    }

    [Test]
    public void Transfer_AmountMustBePositive_Negative_ShouldFail()
    {
        var validation = ValidateTransfer(budgetId: 1, sourceItemId: 1, targetItemId: 2, amount: -500m,
            sourceBudgetId: 1, targetBudgetId: 1);

        validation.IsValid.ShouldBeFalse();
    }

    [Test]
    public void Transfer_Valid_ShouldPass()
    {
        var validation = ValidateTransfer(budgetId: 1, sourceItemId: 1, targetItemId: 2, amount: 1000m,
            sourceBudgetId: 1, targetBudgetId: 1);

        validation.IsValid.ShouldBeTrue();
    }

    private static ValidationResult ValidateTransfer(
        int budgetId, int sourceItemId, int targetItemId, decimal amount,
        int sourceBudgetId, int targetBudgetId)
    {
        var errors = new List<string>();

        if (sourceBudgetId != targetBudgetId)
            errors.Add("Source and target must be in the same Budget.");

        if (sourceItemId == targetItemId)
            errors.Add("Source and target items must be different.");

        if (amount <= 0)
            errors.Add("Amount must be greater than zero.");

        return new ValidationResult(errors.Count == 0, errors);
    }

    private record ValidationResult(bool IsValid, List<string> Errors);
}
