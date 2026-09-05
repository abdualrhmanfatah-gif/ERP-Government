using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class UserPermission : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string? Reason { get; set; }
    public int? ApprovedById { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User User { get; set; } = null!;
    public SecurityPermission Permission { get; set; } = null!;
    public User? ApprovedBy { get; set; }
}
