using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Committees.Entities;

public class CommitteeMember : BaseAuditableEntity
{
    public int CommitteeId { get; set; }
    public int? EmployeeId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public Enums.CommitteeMemberRole MemberRole { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Committee Committee { get; set; } = null!;
}
