using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Common;

public interface IBudgetAvailabilityService
{
    Task<decimal> GetAvailableForAppropriationAsync(int budgetItemAllocationId);
    Task<BudgetAvailabilitySummary> GetAvailabilitySummaryAsync(int budgetItemAllocationId);
    Task<List<AvailabilityBreakdownDto>> GetAvailabilityBreakdownAsync(int budgetItemAllocationId, int fiscalYearId);
    bool EvaluateAllowOverrun(bool? budgetItemAllowOverrun, bool? budgetAllowOverrun, bool budgetTypeAllowOverrun);
    (bool Allowed, string? Warning) EvaluateControlMethod(BudgetControlMethod controlMethod, decimal requested, decimal available);
}

public record BudgetAvailabilitySummary(
    int BudgetItemAllocationId,
    decimal ApprovedAmount,
    decimal ActualExpenditure,
    decimal RemainingAmount,
    decimal OutstandingEncumbrance,
    decimal AvailableAmount,
    bool EffectiveAllowOverrun,
    BudgetControlMethod BudgetControlMethod);

public class BudgetAvailabilityService : IBudgetAvailabilityService
{
    private readonly IApplicationDbContext _context;

    public BudgetAvailabilityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetAvailableForAppropriationAsync(int budgetItemAllocationId)
    {
        var allocation = await _context.BudgetItemAllocations
            .Include(a => a.BudgetItem)
            .Include(a => a.Budget)
            .FirstOrDefaultAsync(a => a.Id == budgetItemAllocationId);

        if (allocation is null)
            return 0;

        var approvedAmount = allocation.ApprovedAmount ?? allocation.ProposedAmount;

        var actualExpenditure = await ComputeActualExpenditureAsync(allocation.BudgetItem.AccountId, allocation.Budget.FiscalYearId);
        var outstandingEncumbrance = await ComputeOutstandingEncumbranceAsync(allocation.BudgetItemId);

        return approvedAmount - actualExpenditure - outstandingEncumbrance;
    }

    public async Task<BudgetAvailabilitySummary> GetAvailabilitySummaryAsync(int budgetItemAllocationId)
    {
        var allocation = await _context.BudgetItemAllocations
            .Include(a => a.BudgetItem)
            .Include(a => a.Budget)
                .ThenInclude(b => b.BudgetType)
            .FirstOrDefaultAsync(a => a.Id == budgetItemAllocationId);

        if (allocation is null)
            return new BudgetAvailabilitySummary(budgetItemAllocationId, 0, 0, 0, 0, 0, false, BudgetControlMethod.None);

        var approvedAmount = allocation.ApprovedAmount ?? 0;
        var actualExpenditure = await ComputeActualExpenditureAsync(allocation.BudgetItem.AccountId, allocation.Budget.FiscalYearId);
        var outstandingEncumbrance = await ComputeOutstandingEncumbranceAsync(allocation.BudgetItemId);
        var remainingAmount = approvedAmount - actualExpenditure;
        var availableAmount = remainingAmount - outstandingEncumbrance;

        var effectiveAllowOverrun = EvaluateAllowOverrun(
            allocation.BudgetItem.AllowOverrun,
            allocation.Budget.AllowOverrun,
            allocation.Budget.BudgetType?.AllowOverrun ?? false);

        var controlMethod = allocation.Budget.BudgetType?.ControlMethod ?? BudgetControlMethod.None;

        return new BudgetAvailabilitySummary(
            budgetItemAllocationId,
            approvedAmount,
            actualExpenditure,
            remainingAmount,
            outstandingEncumbrance,
            availableAmount,
            effectiveAllowOverrun,
            controlMethod);
    }

    public async Task<List<AvailabilityBreakdownDto>> GetAvailabilityBreakdownAsync(int budgetItemAllocationId, int fiscalYearId)
    {
        var allocation = await _context.BudgetItemAllocations
            .Include(a => a.BudgetItem)
            .Include(a => a.Budget)
            .FirstOrDefaultAsync(a => a.Id == budgetItemAllocationId);

        if (allocation is null)
            return [];

        var approvedAmount = allocation.ApprovedAmount ?? 0;
        var actualExpenditure = await ComputeActualExpenditureAsync(allocation.BudgetItem.AccountId, allocation.Budget.FiscalYearId);
        var outstandingEncumbrance = await ComputeOutstandingEncumbranceAsync(allocation.BudgetItemId);
        var available = approvedAmount - actualExpenditure - outstandingEncumbrance;

        return
        [
            new AvailabilityBreakdownDto(
                FundId: allocation.Budget.FundId,
                FundCode: allocation.Budget.Fund?.FundNumber ?? "DEFAULT",
                FundName: allocation.Budget.Fund?.FundName ?? "Default Fund",
                ProgramId: null,
                ProgramCode: null,
                ProgramName: null,
                ProjectId: null,
                ProjectCode: null,
                ProjectName: null,
                BudgetItemId: allocation.BudgetItemId,
                ItemCode: allocation.BudgetItem.ItemCode,
                AppropriationAmount: approvedAmount,
                EncumberedAmount: outstandingEncumbrance,
                PaidAmount: actualExpenditure,
                AvailableAmount: available)
        ];
    }

    public bool EvaluateAllowOverrun(bool? budgetItemAllowOverrun, bool? budgetAllowOverrun, bool budgetTypeAllowOverrun)
    {
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

    private async Task<decimal> ComputeActualExpenditureAsync(int? accountId, int fiscalYearId)
    {
        if (!accountId.HasValue)
            return 0;

        return await _context.JournalEntryLines
            .Where(l => l.AccountId == accountId.Value
                && l.JournalEntry.FiscalYearId == fiscalYearId
                && l.JournalEntry.EntryStatus == Domain.Accounting.Enums.EntryStatus.Posted
                && l.JournalEntry.ReversalOfId == null)
            .SumAsync(l => l.Debit - l.Credit);
    }

    private async Task<decimal> ComputeOutstandingEncumbranceAsync(int budgetItemId)
    {
        return await _context.EncumbranceLines
            .Where(l => l.BudgetItemId == budgetItemId
                && (l.Encumbrance.Status == EncumbranceStatus.Active
                    || l.Encumbrance.Status == EncumbranceStatus.PartiallyReleased
                    || l.Encumbrance.Status == EncumbranceStatus.PartiallyLiquidated)
                && l.Encumbrance.ReversalOfId == null)
            .SumAsync(l => l.Amount - l.LiquidatedAmount - l.CancelledAmount);
    }
}
