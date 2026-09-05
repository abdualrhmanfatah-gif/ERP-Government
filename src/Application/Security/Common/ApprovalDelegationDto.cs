using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Security.Common;

public class ApprovalDelegationDto
{
    public int Id { get; init; }
    public int DelegatorUserId { get; init; }
    public int DelegateUserId { get; init; }
    public string? EntityType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DelegationStatus Status { get; init; }
    public bool CanReDelegate { get; init; }
    public string? Reason { get; init; }
}
