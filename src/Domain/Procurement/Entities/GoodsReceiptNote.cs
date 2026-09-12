using ERP_Government.Domain.Common;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Domain.Procurement.Entities;

public class GoodsReceiptNote : BaseAuditableEntity
{
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime GRNDate { get; set; }
    public int? SupplierPartyId { get; set; }
    public int PurchaseOrderId { get; set; }
    public int WarehouseId { get; set; }
    public int LocationId { get; set; }
    public int? ReceivedBy { get; set; }
    public GRNStatus Status { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public ICollection<GoodsReceiptNoteDetail> Details { get; set; } = [];
}
