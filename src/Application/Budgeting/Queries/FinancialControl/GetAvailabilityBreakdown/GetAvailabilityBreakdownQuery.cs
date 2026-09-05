using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Budgeting.Queries.FinancialControl.GetAvailabilityBreakdown;

[Authorize(Policy = PermissionCodes.BudgetItemsView)]
public record GetAvailabilityBreakdownQuery(int BudgetItemId, int FiscalYearId)
    : IRequest<AvailabilityBreakdownResult>;

public record AvailabilityBreakdownResult(
    int BudgetItemId,
    string BudgetItemCode,
    int FiscalYearId,
    string FiscalYearName,
    List<AvailabilityBreakdownDto> Breakdown,
    AvailabilityBreakdownTotalDto Totals);

public class GetAvailabilityBreakdownQueryHandler(
    IBudgetAvailabilityService availabilityService,
    IApplicationDbContext context) : IRequestHandler<GetAvailabilityBreakdownQuery, AvailabilityBreakdownResult>
{
    public async Task<AvailabilityBreakdownResult> Handle(
        GetAvailabilityBreakdownQuery request,
        CancellationToken cancellationToken)
    {
        var item = await context.BudgetItems.FindAsync(request.BudgetItemId, cancellationToken);
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);

        var breakdown = await availabilityService.GetAvailabilityBreakdownAsync(
            request.BudgetItemId, request.FiscalYearId);

        var totals = new AvailabilityBreakdownTotalDto(
            breakdown.Sum(b => b.AppropriationAmount),
            breakdown.Sum(b => b.EncumberedAmount),
            breakdown.Sum(b => b.PaidAmount),
            breakdown.Sum(b => b.AvailableAmount));

        return new AvailabilityBreakdownResult(
            request.BudgetItemId,
            item?.ItemCode ?? string.Empty,
            request.FiscalYearId,
            fiscalYear?.Name ?? string.Empty,
            breakdown,
            totals);
    }
}
