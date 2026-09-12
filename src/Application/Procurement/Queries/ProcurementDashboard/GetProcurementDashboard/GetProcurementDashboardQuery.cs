using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.ProcurementDashboard.GetProcurementDashboard;

public record GetProcurementDashboardQuery() : IRequest<Result<ProcurementDashboardResponse>>;

public record ProcurementDashboardResponse(
    int PendingApprovals,
    int OpenPurchaseOrders,
    int PendingReceipts,
    int PendingInvoices,
    EncumbranceSummaryDto EncumbranceSummary);

public record EncumbranceSummaryDto(
    decimal TotalEncumbered,
    decimal TotalLiquidated,
    decimal TotalOutstanding);

public class GetProcurementDashboardQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetProcurementDashboardQuery, Result<ProcurementDashboardResponse>>
{
    public async Task<Result<ProcurementDashboardResponse>> Handle(
        GetProcurementDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var pendingApprovals = await context.PurchaseRequests
            .CountAsync(p => p.Status == PurchaseRequestStatus.Submitted, cancellationToken)
            + await context.PurchaseOrders
            .CountAsync(p => p.Status == PurchaseOrderStatus.Submitted, cancellationToken);

        var openPurchaseOrders = await context.PurchaseOrders
            .CountAsync(p => p.Status == PurchaseOrderStatus.Approved
                          || p.Status == PurchaseOrderStatus.Issued
                          || p.Status == PurchaseOrderStatus.PartiallyReceived, cancellationToken);

        var pendingReceipts = await context.GoodsReceiptNotes
            .CountAsync(g => g.Status == GRNStatus.Draft, cancellationToken);

        var pendingInvoices = await context.SupplierInvoices
            .CountAsync(i => i.Status == SupplierInvoiceStatus.Draft
                          || i.Status == SupplierInvoiceStatus.Submitted, cancellationToken);

        var encumbranceLines = await context.EncumbranceLines
            .Include(el => el.Encumbrance)
            .Where(el => el.Encumbrance.PurchaseOrderId != null
                      && el.Encumbrance.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled
                      && el.Encumbrance.Status != Domain.Budgeting.Enums.EncumbranceStatus.Reversed)
            .Select(el => new { el.Amount, el.LiquidatedAmount })
            .ToListAsync(cancellationToken);

        var totalEncumbered = encumbranceLines.Sum(e => e.Amount);
        var totalLiquidated = encumbranceLines.Sum(e => e.LiquidatedAmount);
        var totalOutstanding = totalEncumbered - totalLiquidated;

        var response = new ProcurementDashboardResponse(
            pendingApprovals,
            openPurchaseOrders,
            pendingReceipts,
            pendingInvoices,
            new EncumbranceSummaryDto(totalEncumbered, totalLiquidated, totalOutstanding));

        return Result<ProcurementDashboardResponse>.Success(response);
    }
}
