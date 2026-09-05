using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Committee seed data — 2 committees.
/// </summary>
public static class CommitteeSeedData
{
    public static List<Committee> GetCommittees() =>
    [
        new() { CommitteeNumber="COM-001", Name="لجنة المشتريات", CommitteeType=CommitteeType.Tender, FormationDecisionNumber="قرار 2026/001", FormationDecisionDate=new DateOnly(2026,1,1), ValidFrom=new DateOnly(2026,1,1), Status=CommitteeStatus.Active },
        new() { CommitteeNumber="COM-002", Name="لجنة الاستلام", CommitteeType=CommitteeType.Receiving, FormationDecisionNumber="قرار 2026/002", FormationDecisionDate=new DateOnly(2026,1,1), ValidFrom=new DateOnly(2026,1,1), Status=CommitteeStatus.Active },
    ];
}
