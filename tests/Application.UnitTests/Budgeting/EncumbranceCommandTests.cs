using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Domain.Budgeting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class EncumbranceCommandTests
{
    private BudgetAvailabilityService _availabilityService = null!;

    [SetUp]
    public void Setup()
    {
        _availabilityService = new BudgetAvailabilityService(null!);
    }

    // ─── T023: FSM Guard Matrix ─────────────────────────────────────

    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.PendingApproval, true)]
    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.Cancelled, true)]
    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.Closed, false)]
    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.Reversed, false)]
    [TestCase(EncumbranceStatus.Draft, EncumbranceStatus.Suspended, false)]
    [TestCase(EncumbranceStatus.PendingApproval, EncumbranceStatus.Approved, true)]
    [TestCase(EncumbranceStatus.PendingApproval, EncumbranceStatus.Cancelled, true)]
    [TestCase(EncumbranceStatus.PendingApproval, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.PendingApproval, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Approved, EncumbranceStatus.Active, true)]
    [TestCase(EncumbranceStatus.Approved, EncumbranceStatus.Cancelled, true)]
    [TestCase(EncumbranceStatus.Approved, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.Suspended, true)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.Closed, true)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.Cancelled, true)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.Reversed, true)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Active, EncumbranceStatus.PendingApproval, false)]
    [TestCase(EncumbranceStatus.Suspended, EncumbranceStatus.Closed, true)]
    [TestCase(EncumbranceStatus.Suspended, EncumbranceStatus.Cancelled, true)]
    [TestCase(EncumbranceStatus.Suspended, EncumbranceStatus.Reversed, true)]
    [TestCase(EncumbranceStatus.Suspended, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.Suspended, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Closed, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.Closed, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Cancelled, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.Cancelled, EncumbranceStatus.Draft, false)]
    [TestCase(EncumbranceStatus.Reversed, EncumbranceStatus.Active, false)]
    [TestCase(EncumbranceStatus.Reversed, EncumbranceStatus.Draft, false)]
    public void FsmGuard_ShouldAllowOrRejectTransition(
        EncumbranceStatus from, EncumbranceStatus to, bool shouldAllow)
    {
        var result = EncumbranceFsm.CanTransition(from, to);
        result.ShouldBe(shouldAllow,
            $"Transition {from} → {(shouldAllow ? "should" : "should not")} be allowed");
    }

    [Test]
    public void UpdateDelete_ShouldOnlyBeAllowedFromDraft()
    {
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Draft).ShouldBeTrue();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.PendingApproval).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Approved).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Active).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Suspended).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Closed).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Cancelled).ShouldBeFalse();
        EncumbranceFsm.CanUpdateDelete(EncumbranceStatus.Reversed).ShouldBeFalse();
    }

    [Test]
    public void Reverse_ShouldOnlyBeAllowedFromActiveOrSuspended()
    {
        EncumbranceFsm.CanReverse(EncumbranceStatus.Active).ShouldBeTrue();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Suspended).ShouldBeTrue();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Draft).ShouldBeFalse();
        EncumbranceFsm.CanReverse(EncumbranceStatus.PendingApproval).ShouldBeFalse();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Approved).ShouldBeFalse();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Closed).ShouldBeFalse();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Cancelled).ShouldBeFalse();
        EncumbranceFsm.CanReverse(EncumbranceStatus.Reversed).ShouldBeFalse();
    }

    // ─── T024: Gate Decisions ────────────────────────────────────────

    [Test]
    public void EvaluateControlMethod_None_ShouldAlwaysAllow()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.None, 5000m, 3000m);

        allowed.ShouldBeTrue();
        warning.ShouldBeNull();
    }

    [Test]
    public void EvaluateControlMethod_Warning_WithinBudget_ShouldAllowWithoutWarning()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Warning, 3000m, 5000m);

        allowed.ShouldBeTrue();
        warning.ShouldBeNull();
    }

    [Test]
    public void EvaluateControlMethod_Warning_ExceedsBudget_ShouldAllowWithWarning()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Warning, 5000m, 3000m);

        allowed.ShouldBeTrue();
        warning.ShouldNotBeNull();
        warning.ShouldContain("5");
        warning.ShouldContain("3");
    }

    [Test]
    public void EvaluateControlMethod_Blocking_WithinBudget_ShouldAllow()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Blocking, 3000m, 5000m);

        allowed.ShouldBeTrue();
        warning.ShouldBeNull();
    }

    [Test]
    public void EvaluateControlMethod_Blocking_ExceedsBudget_ShouldBlock()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Blocking, 5000m, 3000m);

        allowed.ShouldBeFalse();
        warning.ShouldNotBeNull();
        warning.ShouldContain("5");
        warning.ShouldContain("3");
    }

    [Test]
    public void EvaluateControlMethod_Warning_ExactAmount_ShouldAllow()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Warning, 5000m, 5000m);

        allowed.ShouldBeTrue();
        warning.ShouldBeNull();
    }

    [Test]
    public void EvaluateControlMethod_Blocking_ExactAmount_ShouldAllow()
    {
        var (allowed, warning) = _availabilityService.EvaluateControlMethod(
            BudgetControlMethod.Blocking, 5000m, 5000m);

        allowed.ShouldBeTrue();
        warning.ShouldBeNull();
    }
}

/// <summary>
/// FSM guard logic for encumbrance status transitions.
/// This is a domain service that encumbrance commands will depend on.
/// </summary>
public static class EncumbranceFsm
{
    private static readonly Dictionary<EncumbranceStatus, HashSet<EncumbranceStatus>> AllowedTransitions = new()
    {
        [EncumbranceStatus.Draft] = [EncumbranceStatus.PendingApproval, EncumbranceStatus.Cancelled],
        [EncumbranceStatus.PendingApproval] = [EncumbranceStatus.Approved, EncumbranceStatus.Cancelled],
        [EncumbranceStatus.Approved] = [EncumbranceStatus.Active, EncumbranceStatus.Cancelled],
        [EncumbranceStatus.Active] = [EncumbranceStatus.Suspended, EncumbranceStatus.Closed, EncumbranceStatus.Cancelled, EncumbranceStatus.Reversed],
        [EncumbranceStatus.Suspended] = [EncumbranceStatus.Closed, EncumbranceStatus.Cancelled, EncumbranceStatus.Reversed],
    };

    public static bool CanTransition(EncumbranceStatus from, EncumbranceStatus to)
    {
        if (!AllowedTransitions.TryGetValue(from, out var allowed))
            return false;

        return allowed.Contains(to);
    }

    public static bool CanUpdateDelete(EncumbranceStatus status)
        => status == EncumbranceStatus.Draft;

    public static bool CanReverse(EncumbranceStatus status)
        => status is EncumbranceStatus.Active or EncumbranceStatus.Suspended;
}
