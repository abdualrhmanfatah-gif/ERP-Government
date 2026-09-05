using ERP_Government.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Common;

public class ApprovalRuleEvaluationService(
    IApplicationDbContext context) : IApprovalRuleEvaluationService
{
    public async Task<IReadOnlyList<ApprovalRuleResult>> EvaluateAsync(
        string documentType,
        decimal amount,
        int? fundId,
        int? currencyId,
        CancellationToken ct)
    {
        var rules = await context.ApprovalRules
            .Where(r => r.DocumentType == documentType && r.IsActive)
            .Where(r => r.AmountThreshold == null || amount >= r.AmountThreshold)
            .Where(r => r.FundId == null || fundId != null && r.FundId == fundId)
            .OrderBy(r => r.Sequence)
            .Select(r => new ApprovalRuleResult
            {
                Sequence = r.Sequence,
                RequiredRole = r.ApproverRole ?? string.Empty,
                ApproverRoleId = r.ApproverRoleId
            })
            .ToListAsync(ct);

        return rules;
    }
}
