using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// ApprovalRule seed data — 6 approval rules.
/// </summary>
public static class ApprovalRuleSeedData
{
    public static List<ApprovalRule> GetApprovalRules() =>
    [
        new() { DocumentType="PurchaseOrder", AmountThreshold=100000, ApproverRole="PROC_MGR", Sequence=1 },
        new() { DocumentType="PurchaseOrder", AmountThreshold=500000, ApproverRole="FIN_MGR", Sequence=2 },
        new() { DocumentType="PaymentOrder", AmountThreshold=50000, ApproverRole="PAY_MGR", Sequence=1 },
        new() { DocumentType="PaymentOrder", AmountThreshold=200000, ApproverRole="FIN_MGR", Sequence=2 },
        new() { DocumentType="PurchaseRequest", AmountThreshold=100000, ApproverRole="PROC_MGR", Sequence=1 },
        new() { DocumentType="PurchaseRequest", AmountThreshold=500000, ApproverRole="FIN_MGR", Sequence=2 },
    ];
}
