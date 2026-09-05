using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class ApprovalDelegation : BaseAuditableEntity
{
    public int DelegatorUserId { get; set; }
    public int DelegateUserId { get; set; }
    public string? EntityType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DelegationStatus Status { get; set; }
    public bool CanReDelegate { get; set; }
    public string? Reason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User DelegatorUser { get; set; } = null!;
    public User DelegateUser { get; set; } = null!;
}
