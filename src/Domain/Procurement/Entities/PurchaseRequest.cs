using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Procurement.Entities;

public class PurchaseRequest : BaseAuditableEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateOnly? RequiredDate { get; set; }
    public int? DepartmentId { get; set; }
    public int? CostCenterId { get; set; }
    public int? RequesterId { get; set; }
    public string? Priority { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? TotalItems { get; set; }
    public decimal? TotalQuantity { get; set; }
    public decimal? EstimatedTotalCost { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public int? CancelledById { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? FinalApprovedById { get; set; }
    public DateTime? FinalApprovedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
