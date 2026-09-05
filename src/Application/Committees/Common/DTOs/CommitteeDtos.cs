using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Common.DTOs;

public class CommitteeDto
{
    public int Id { get; init; }
    public string CommitteeNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CommitteeType CommitteeType { get; init; }
    public string FormationDecisionNumber { get; init; } = string.Empty;
    public DateTime FormationDecisionDate { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public CommitteeStatus Status { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
}

public class CommitteeMemberDto
{
    public int Id { get; init; }
    public int CommitteeId { get; init; }
    public int? EmployeeId { get; init; }
    public string MemberName { get; init; } = string.Empty;
    public CommitteeMemberRole MemberRole { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}

public class CommitteeAssignmentDto
{
    public int Id { get; init; }
    public int CommitteeId { get; init; }
    public CommitteeAssignmentType AssignmentType { get; init; }
    public int? PurchaseOrderId { get; init; }
    public DateTime AssignmentDate { get; init; }
    public string? DecisionNumber { get; init; }
    public DateTime? DecisionDate { get; init; }
    public CommitteeAssignmentStatus Status { get; init; }
    public int RequiredSignaturesCount { get; init; }
    public int ActualSignaturesCount { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}
