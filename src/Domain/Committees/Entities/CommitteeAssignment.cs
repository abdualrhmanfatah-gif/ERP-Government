using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Committees.Entities;

public class CommitteeAssignment : BaseAuditableEntity
{
    public int CommitteeId { get; set; }
    public Enums.CommitteeAssignmentType AssignmentType { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateOnly AssignmentDate { get; set; }
    public string? DecisionNumber { get; set; }
    public DateOnly? DecisionDate { get; set; }
    public Enums.CommitteeAssignmentStatus Status { get; set; }
    public int RequiredSignaturesCount { get; set; } = 1;
    public int ActualSignaturesCount { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Committee Committee { get; set; } = null!;
}
