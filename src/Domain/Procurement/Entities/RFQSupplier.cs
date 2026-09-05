using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class RFQSupplier : BaseAuditableEntity
{
    public int RFQId { get; set; }
    public int SupplierId { get; set; }
    public int? PartyId { get; set; }
    public DateTime? InvitationDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public RequestForQuotation? RequestForQuotation { get; set; }
}
