using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.PendingApprovals.Queries.GetPendingApprovals;

[Authorize(Policy = PermissionCodes.PurchaseOrdersView)]
public class GetPendingApprovalsQuery : IRequest<List<PendingApprovalDto>>
{
    public string? DocumentType { get; init; }
}

public class GetPendingApprovalsQueryHandler(
    IApplicationDbContext context,
    IApprovalRuleEvaluationService evaluationService) : IRequestHandler<GetPendingApprovalsQuery, List<PendingApprovalDto>>
{
    public async Task<List<PendingApprovalDto>> Handle(
        GetPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var results = new List<PendingApprovalDto>();

        // Query submitted PurchaseOrders
        if (string.IsNullOrEmpty(request.DocumentType) || request.DocumentType == "PurchaseOrder")
        {
            var submittedPOs = await context.PurchaseOrders
                .Where(po => po.Status == ERP_Government.Domain.Procurement.Enums.PurchaseOrderStatus.Submitted)
                .ToListAsync(cancellationToken);

            foreach (var po in submittedPOs)
            {
                var rules = await evaluationService.EvaluateAsync(
                    "PurchaseOrder", po.GrandTotal ?? 0m, null, null, cancellationToken);

                if (rules.Any(r => !string.IsNullOrEmpty(r.RequiredRole)))
                {
                    results.Add(new PendingApprovalDto
                    {
                        DocumentType = "PurchaseOrder",
                        DocumentId = po.Id,
                        Amount = po.GrandTotal ?? 0m,
                        SubmittedByUserId = 0, // Would need to track submitter
                        SubmittedAt = po.Created,
                        RequiredRole = string.Join(", ", rules.Select(r => r.RequiredRole).Where(r => !string.IsNullOrEmpty(r)))
                    });
                }
            }
        }

        // Query submitted PaymentOrders
        if (string.IsNullOrEmpty(request.DocumentType) || request.DocumentType == "PaymentOrder")
        {
            var submittedPOs = await context.PaymentOrders
                .Where(po => po.Status == Domain.Payments.Enums.PaymentOrderStatus.Submitted)
                .ToListAsync(cancellationToken);

            foreach (var po in submittedPOs)
            {
                var rules = await evaluationService.EvaluateAsync(
                    "PaymentOrder", po.AmountGross, po.FundId, po.CurrencyId, cancellationToken);

                if (rules.Any(r => !string.IsNullOrEmpty(r.RequiredRole)))
                {
                    results.Add(new PendingApprovalDto
                    {
                        DocumentType = "PaymentOrder",
                        DocumentId = po.Id,
                        Amount = po.AmountGross,
                        SubmittedByUserId = 0,
                        SubmittedAt = po.Created,
                        RequiredRole = string.Join(", ", rules.Select(r => r.RequiredRole).Where(r => !string.IsNullOrEmpty(r)))
                    });
                }
            }
        }

        return results.OrderByDescending(r => r.SubmittedAt).ToList();
    }
}
