using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Common;

public class ApprovalService(
    IApplicationDbContext context,
    IIdentityService identityService) : IApprovalService
{
    public async Task<ApprovalResult> ValidateAndRecordAsync(
        string documentType,
        int documentId,
        int executingUserId,
        string decision,
        string? reason,
        IReadOnlyList<ApprovalRuleResult> evaluationResults,
        CancellationToken ct)
    {
        var errors = new List<string>();

        var user = await context.Users.FindAsync(executingUserId, ct);
        if (user is null)
            return new ApprovalResult { Success = false, Errors = ["User not found."] };

        if (evaluationResults.Count == 0)
            return new ApprovalResult { Success = false, Errors = ["No approval rules defined for this document type."] };

        // Check user has at least one required role
        var hasRequiredRole = false;
        string? matchedRole = null;

        foreach (var rule in evaluationResults)
        {
            if (string.IsNullOrEmpty(rule.RequiredRole)) continue;

            if (await identityService.IsInRoleAsync(executingUserId, rule.RequiredRole))
            {
                hasRequiredRole = true;
                matchedRole = rule.RequiredRole;
                break;
            }
        }

        if (!hasRequiredRole)
        {
            // Check delegation
            var delegation = await ResolveActiveDelegationAsync(executingUserId, documentType, ct);
            if (delegation is not null)
            {
                hasRequiredRole = true;
                matchedRole = $"Delegated:{delegation.DelegatorUserId}";
            }
        }

        if (!hasRequiredRole)
        {
            var requiredRoles = evaluationResults.Select(r => r.RequiredRole).Where(r => !string.IsNullOrEmpty(r));
            errors.Add($"User does not have any required approval role ({string.Join(", ", requiredRoles)}).");
            return new ApprovalResult { Success = false, Errors = errors };
        }

        // Create ApprovalHistory record
        var evaluationSnapshot = JsonSerializer.Serialize(evaluationResults.Select(r => new
        {
            r.Sequence,
            r.RequiredRole,
            r.ApproverRoleId
        }));

        var history = new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = documentType,
            DocumentId = documentId,
            ApproverUserId = executingUserId,
            RequiredRole = matchedRole ?? string.Empty,
            Decision = decision,
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = reason,
            EvaluationSnapshot = evaluationSnapshot,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = executingUserId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = executingUserId.ToString()
        };

        context.ApprovalHistory.Add(history);
        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Approval",
            Action = decision,
            UserId = executingUserId,
            EntityName = documentType,
            EntityId = documentId,
            Success = true,
            Timestamp = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(ct);

        return new ApprovalResult
        {
            Success = true,
            ApprovalHistoryId = history.Id
        };
    }

    public async Task<IReadOnlyList<ApprovalDelegation>> ResolveDelegationAsync(
        int userId,
        string documentType,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await context.ApprovalDelegations
            .Where(d => d.DelegateUserId == userId
                && d.Status == Domain.Security.Enums.DelegationStatus.Active
                && d.StartDate <= today
                && d.EndDate >= today
                && (d.EntityType == null || d.EntityType == documentType))
            .OrderByDescending(d => d.Created)
            .ToListAsync(ct);
    }

    private async Task<ApprovalDelegation?> ResolveActiveDelegationAsync(
        int userId, string documentType, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await context.ApprovalDelegations
            .FirstOrDefaultAsync(d =>
                d.DelegateUserId == userId
                && d.Status == Domain.Security.Enums.DelegationStatus.Active
                && d.StartDate <= today
                && d.EndDate >= today
                && (d.EntityType == null || d.EntityType == documentType),
                ct);
    }
}
