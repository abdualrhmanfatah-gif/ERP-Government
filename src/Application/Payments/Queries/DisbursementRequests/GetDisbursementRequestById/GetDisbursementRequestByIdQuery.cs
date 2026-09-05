using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequestById;

public record GetDisbursementRequestByIdQuery(int Id) : IRequest<DisbursementRequestDetailDto?>;

public class GetDisbursementRequestByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementRequestByIdQuery, DisbursementRequestDetailDto?>
{
    public async Task<DisbursementRequestDetailDto?> Handle(
        GetDisbursementRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DisbursementRequests
            .Include(d => d.PaymentOrder)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        var approvals = await context.ApprovalHistory
            .Where(a => a.DocumentType == "DisbursementRequest" && a.DocumentId == entity.Id)
            .OrderBy(a => a.ApprovalStep)
            .Select(a => new ApprovalStepDto(
                a.ApprovalStep,
                a.ApproverUserId,
                "",
                a.RequiredRole,
                a.Decision,
                a.DecisionAt))
            .ToListAsync(cancellationToken);

        return new DisbursementRequestDetailDto(
            entity.Id,
            entity.RequestNumber,
            entity.PaymentOrderId,
            entity.PaymentOrder.PaymentOrderNumber,
            entity.RequestedById,
            "",
            entity.RequestDate,
            entity.Status,
            entity.HasWarning,
            entity.Notes,
            entity.PaymentOrder.AmountGross - entity.PaymentOrder.DeductionAmount,
            approvals);
    }
}
