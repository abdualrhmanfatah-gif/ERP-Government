using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Common;

public interface IBudgetAvailabilityService
{
    Task<decimal> GetAvailableForAppropriationAsync(int budgetItemId);
    Task<decimal> GetAvailableForEncumbranceAsync(int appropriationId);
    Task<BudgetAvailabilitySummary> GetAvailabilitySummaryAsync(int budgetItemId);
    Task<List<AvailabilityBreakdownDto>> GetAvailabilityBreakdownAsync(int budgetItemId, int fiscalYearId);
    bool EvaluateAllowOverrun(bool? budgetItemAllowOverrun, bool? budgetAllowOverrun, bool budgetTypeAllowOverrun);
    (bool Allowed, string? Warning) EvaluateControlMethod(BudgetControlMethod controlMethod, decimal requested, decimal available);
}

public record BudgetAvailabilitySummary(
    int BudgetItemId,
    decimal NetAppropriated,
    decimal Encumbered,
    decimal Available,
    bool EffectiveAllowOverrun,
    BudgetControlMethod BudgetControlMethod);

public class BudgetAvailabilityService : IBudgetAvailabilityService
{
    private readonly IApplicationDbContext _context;

    public BudgetAvailabilityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetAvailableForAppropriationAsync(int budgetItemId)
    {
        var netAppropriated = await _context.Appropriations
            .Where(a => a.BudgetItemId == budgetItemId && a.Status == AppropriationStatus.Active)
            .SumAsync(a =>
                a.AppropriationType == AppropriationType.Original ? a.Amount :
                a.AppropriationType == AppropriationType.Supplement ? a.Amount :
                a.AppropriationType == AppropriationType.Reduction ? -a.Amount :
                a.AppropriationType == AppropriationType.Adjustment ? a.Amount :
                0m);

        return netAppropriated;
    }

    public async Task<decimal> GetAvailableForEncumbranceAsync(int appropriationId)
    {
        // Get the budget item from the appropriation
        var appropriation = await _context.Appropriations
            .FirstOrDefaultAsync(a => a.Id == appropriationId);

        if (appropriation == null)
            return 0m;

        // Get net available from the budget item
        var netAppropriated = await GetAvailableForAppropriationAsync(appropriation.BudgetItemId);

        // Subtract encumbrances for this appropriation
        var encumbered = await _context.Encumbrances
            .Where(e => e.AppropriationId == appropriationId
                && (e.Status == EncumbranceStatus.Active
                    || e.Status == EncumbranceStatus.PartiallyReleased
                    || e.Status == EncumbranceStatus.PartiallyLiquidated)
                && e.ReversalOfId == null)
            .SumAsync(e => e.Amount);

        return netAppropriated - encumbered;
    }

    public async Task<BudgetAvailabilitySummary> GetAvailabilitySummaryAsync(int budgetItemId)
    {
        var item = await _context.BudgetItems
            .FirstOrDefaultAsync(bi => bi.Id == budgetItemId);

        if (item == null)
            return new BudgetAvailabilitySummary(budgetItemId, 0, 0, 0, false, BudgetControlMethod.None);

        var budget = await _context.Budgets
            .Include(b => b.BudgetType)
            .FirstOrDefaultAsync(b => b.Id == item.BudgetId);

        var netAppropriated = await GetAvailableForAppropriationAsync(budgetItemId);

        var encumbered = await _context.Encumbrances
            .Where(e => _context.Appropriations.Any(a => a.BudgetItemId == budgetItemId && a.Id == e.AppropriationId)
                && (e.Status == EncumbranceStatus.Active
                    || e.Status == EncumbranceStatus.PartiallyReleased
                    || e.Status == EncumbranceStatus.PartiallyLiquidated)
                && e.ReversalOfId == null)
            .SumAsync(e => e.Amount);

        var effectiveAllowOverrun = EvaluateAllowOverrun(
            item.AllowOverrun,
            budget?.AllowOverrun,
            budget?.BudgetType?.AllowOverrun ?? false);

        var controlMethod = budget?.BudgetType?.ControlMethod ?? BudgetControlMethod.None;

        return new BudgetAvailabilitySummary(
            budgetItemId,
            netAppropriated,
            encumbered,
            netAppropriated - encumbered,
            effectiveAllowOverrun,
            controlMethod);
    }

    public async Task<List<AvailabilityBreakdownDto>> GetAvailabilityBreakdownAsync(int budgetItemId, int fiscalYearId)
    {
        var item = await _context.BudgetItems
            .FirstOrDefaultAsync(bi => bi.Id == budgetItemId);

        if (item == null)
            return [];

        var budget = await _context.Budgets
            .Include(b => b.BudgetType)
            .FirstOrDefaultAsync(b => b.Id == item.BudgetId);

        var effectiveAllowOverrun = EvaluateAllowOverrun(
            item.AllowOverrun,
            budget?.AllowOverrun,
            budget?.BudgetType?.AllowOverrun ?? false);

        var controlMethod = budget?.BudgetType?.ControlMethod ?? BudgetControlMethod.None;

        var appropriations = await _context.Appropriations
            .Where(a => a.BudgetItemId == budgetItemId
                && a.Status == AppropriationStatus.Active)
            .ToListAsync();

        var appropriationIds = appropriations.Select(a => a.Id).ToList();

        var encumbrances = await _context.Encumbrances
            .Where(e => appropriationIds.Contains(e.AppropriationId)
                && (e.Status == EncumbranceStatus.Active
                    || e.Status == EncumbranceStatus.PartiallyReleased
                    || e.Status == EncumbranceStatus.PartiallyLiquidated)
                && e.ReversalOfId == null)
            .ToListAsync();

        var breakdown = appropriations
            .GroupBy(a => new
            {
                FundId = item.FundId ?? 0,
                FundCode = "DEFAULT",
                FundName = "Default Fund",
                ProgramId = (int?)null,
                ProgramCode = (string?)null,
                ProgramName = (string?)null,
                ProjectId = (int?)null,
                ProjectCode = (string?)null,
                ProjectName = (string?)null
            })
            .Select(g => new AvailabilityBreakdownDto(
                g.Key.FundId,
                g.Key.FundCode,
                g.Key.FundName,
                g.Key.ProgramId,
                g.Key.ProgramCode,
                g.Key.ProgramName,
                g.Key.ProjectId,
                g.Key.ProjectCode,
                g.Key.ProjectName,
                budgetItemId,
                item.ItemCode,
                g.Sum(a => a.AppropriationType == Domain.Budgeting.Enums.AppropriationType.Original ? a.Amount :
                    a.AppropriationType == Domain.Budgeting.Enums.AppropriationType.Supplement ? a.Amount :
                    a.AppropriationType == Domain.Budgeting.Enums.AppropriationType.Reduction ? -a.Amount :
                    a.AppropriationType == Domain.Budgeting.Enums.AppropriationType.Adjustment ? a.Amount : 0m),
                encumbrances
                    .Where(e => g.Any(a => a.Id == e.AppropriationId))
                    .Sum(e => e.Amount),
                0m,
                0m))
            .ToList();

        var result = new List<AvailabilityBreakdownDto>();

        foreach (var line in breakdown)
        {
            var lineEncumbered = encumbrances
                .Where(e => appropriations.Any(a => a.Id == e.AppropriationId))
                .Sum(e => e.Amount);

            var available = line.AppropriationAmount - lineEncumbered - line.PaidAmount;
            result.Add(line with
            {
                EncumberedAmount = lineEncumbered,
                AvailableAmount = available
            });
        }

        return result;
    }

    public bool EvaluateAllowOverrun(bool? budgetItemAllowOverrun, bool? budgetAllowOverrun, bool budgetTypeAllowOverrun)
    {
        // Priority: BudgetItem > Budget > BudgetType
        if (budgetItemAllowOverrun.HasValue)
            return budgetItemAllowOverrun.Value;

        if (budgetAllowOverrun.HasValue)
            return budgetAllowOverrun.Value;

        return budgetTypeAllowOverrun;
    }

    public (bool Allowed, string? Warning) EvaluateControlMethod(BudgetControlMethod controlMethod, decimal requested, decimal available)
    {
        return controlMethod switch
        {
            BudgetControlMethod.None => (true, null),
            BudgetControlMethod.Warning => requested > available
                ? (true, $"Requested amount {requested:C} exceeds available {available:C}. Warning only.")
                : (true, null),
            BudgetControlMethod.Blocking => requested > available
                ? (false, $"Requested amount {requested:C} exceeds available {available:C}. Blocked.")
                : (true, null),
            _ => (true, null)
        };
    }
}
