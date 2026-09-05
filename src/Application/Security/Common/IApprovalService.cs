using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Security.Common;

public interface IApprovalService
{
    /// <summary>
    /// Validates that the user is authorized to approve the document (role or delegation),
    /// records the decision in ApprovalHistory, and returns the result.
    /// </summary>
    /// <param name="documentType">Entity type discriminator (e.g. "PurchaseOrder")</param>
    /// <param name="documentId">ID of the document being approved</param>
    /// <param name="executingUserId">Domain User.Id of the executing user</param>
    /// <param name="decision">"Approved" or "Rejected"</param>
    /// <param name="reason">Optional reason (required for rejection)</param>
    /// <param name="evaluationResults">Rule evaluation results from IApprovalRuleEvaluationService</param>
    /// <param name="ct">Cancellation token</param>
    Task<ApprovalResult> ValidateAndRecordAsync(
        string documentType,
        int documentId,
        int executingUserId,
        string decision,
        string? reason,
        IReadOnlyList<ApprovalRuleResult> evaluationResults,
        CancellationToken ct);

    /// <summary>
    /// Resolves active delegations for a user for a given document type.
    /// </summary>
    Task<IReadOnlyList<ApprovalDelegation>> ResolveDelegationAsync(
        int userId,
        string documentType,
        CancellationToken ct);
}
