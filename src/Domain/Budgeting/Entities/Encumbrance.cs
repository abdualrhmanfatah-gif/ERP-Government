using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class Encumbrance : BaseAuditableEntity
{
    public string EncumbranceNumber { get; set; } = string.Empty;
    public Enums.EncumbranceType EncumbranceType { get; set; }
    public int? VendorPartyId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? DocumentType { get; set; }
    public int? DocumentId { get; set; }
    public DateOnly EncumbranceDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public Enums.EncumbranceStatus Status { get; set; } = Enums.EncumbranceStatus.Draft;
    public int? ReversalOfId { get; set; }
    public string? ReversalReason { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Encumbrance? ReversalOf { get; set; }
}
