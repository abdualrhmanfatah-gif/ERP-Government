namespace ERP_Government.Application.Security.Common;

public interface IApprovalRuleEvaluationService
{
    Task<IReadOnlyList<ApprovalRuleResult>> EvaluateAsync(
        string documentType,
        decimal amount,
        int? fundId,
        int? currencyId,
        CancellationToken ct);
}
