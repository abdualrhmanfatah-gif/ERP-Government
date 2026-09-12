using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.CompleteEvaluation;

public class CompleteEvaluationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CompleteEvaluationCommand, Result>
{
    public async Task<Result> Handle(
        CompleteEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.UnderEvaluation)
            return Result.Failure(["Only quotations under evaluation can be completed."]);

        entity.TechnicalScore = request.TechnicalScore;
        entity.FinancialScore = request.FinancialScore;
        entity.RejectionReason = request.RejectionReason;
        entity.Status = QuotationStatus.Evaluated;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
