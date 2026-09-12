using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.StartEvaluation;

[Authorize(Policy = PermissionCodes.QuotationsEvaluate)]
public record StartEvaluationCommand(int Id) : IRequest<Result>;
