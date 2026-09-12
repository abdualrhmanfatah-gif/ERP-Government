using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.CompleteEvaluation;

[Authorize(Policy = PermissionCodes.QuotationsEvaluate)]
public record CompleteEvaluationCommand(
    int Id,
    decimal? TechnicalScore,
    decimal? FinancialScore,
    string? RejectionReason) : IRequest<Result>;
