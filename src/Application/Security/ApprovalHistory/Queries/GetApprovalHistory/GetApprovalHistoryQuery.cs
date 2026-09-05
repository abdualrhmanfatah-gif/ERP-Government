using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.ApprovalHistory.Queries.GetApprovalHistory;

public class GetApprovalHistoryQuery : IRequest<List<ApprovalHistoryDto>>
{
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
    public int? ApproverUserId { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class GetApprovalHistoryQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetApprovalHistoryQuery, List<ApprovalHistoryDto>>
{
    public async Task<List<ApprovalHistoryDto>> Handle(
        GetApprovalHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ApprovalHistory.AsQueryable();

        if (!string.IsNullOrEmpty(request.DocumentType))
            query = query.Where(h => h.DocumentType == request.DocumentType);

        if (request.DocumentId.HasValue)
            query = query.Where(h => h.DocumentId == request.DocumentId.Value);

        if (request.ApproverUserId.HasValue)
            query = query.Where(h => h.ApproverUserId == request.ApproverUserId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(h => h.DecisionAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(h => h.DecisionAt <= request.ToDate.Value);

        return await query
            .OrderByDescending(h => h.DecisionAt)
            .Select(h => new ApprovalHistoryDto
            {
                Id = h.Id,
                DocumentType = h.DocumentType,
                DocumentId = h.DocumentId,
                ApproverUserId = h.ApproverUserId,
                RequiredRole = h.RequiredRole,
                Decision = h.Decision,
                DecisionAt = h.DecisionAt,
                Reason = h.Reason,
                EvaluationSnapshot = h.EvaluationSnapshot
            })
            .ToListAsync(cancellationToken);
    }
}
