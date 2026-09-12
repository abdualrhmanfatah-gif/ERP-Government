using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class PurchaseRequestDetail : BaseAuditableEntity
{
    public int PurchaseRequestId { get; set; }
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal? ApprovedQuantity { get; set; }
    public decimal? UnitCostEstimate { get; set; }
    public decimal? TotalCostEstimate { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PurchaseRequest PurchaseRequest { get; set; } = null!;
}
