using ERP_Government.Domain.Common;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Domain.Procurement.Entities;

public class PurchaseRequest : BaseAuditableEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateOnly? RequiredDate { get; set; }
    public int? DepartmentId { get; set; }
    public int? CostCenterId { get; set; }
    public int? RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public PurchaseRequestPriority Priority { get; set; }
    public PurchaseRequestStatus Status { get; set; }
    public decimal? TotalEstimatedCost { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<PurchaseRequestDetail> Details { get; set; } = [];
}
