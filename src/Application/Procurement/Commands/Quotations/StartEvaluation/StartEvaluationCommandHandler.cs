using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.StartEvaluation;

public class StartEvaluationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<StartEvaluationCommand, Result>
{
    public async Task<Result> Handle(
        StartEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.Submitted)
            return Result.Failure(["Only submitted quotations can start evaluation."]);

        entity.Status = QuotationStatus.UnderEvaluation;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
