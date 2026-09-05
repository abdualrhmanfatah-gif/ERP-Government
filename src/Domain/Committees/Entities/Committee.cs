using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Committees.Entities;

public class Committee : BaseAuditableEntity
{
    public string CommitteeNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Enums.CommitteeType CommitteeType { get; set; }
    public string FormationDecisionNumber { get; set; } = string.Empty;
    public DateOnly FormationDecisionDate { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public Enums.CommitteeStatus Status { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
