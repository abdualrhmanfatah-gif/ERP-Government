using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Common;

public class ApprovalService(
    IApplicationDbContext context) : IApprovalService
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

        // Use first matched rule's role for audit trail (no role gate — permission is the gate via [Authorize])
        var matchedRole = evaluationResults
            .FirstOrDefault(r => !string.IsNullOrEmpty(r.RequiredRole))?.RequiredRole ?? "Unknown";

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
}
