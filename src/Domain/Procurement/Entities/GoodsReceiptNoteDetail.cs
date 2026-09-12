using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class GoodsReceiptNoteDetail : BaseAuditableEntity
{
    public int GRNId { get; set; }
    public int PurchaseOrderDetailId { get; set; }
    public int ItemId { get; set; }
    public int UnitId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public GoodsReceiptNote GoodsReceiptNote { get; set; } = null!;
    public PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;
}
