using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Queries.CommitteeAssignments.GetCommitteeAssignments;

[Authorize(Policy = PermissionCodes.CommitteeAssignmentsView)]
public class GetCommitteeAssignmentsQuery : IRequest<List<CommitteeAssignmentDto>>
{
    public int? CommitteeId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public CommitteeAssignmentStatus? Status { get; init; }
}

public class GetCommitteeAssignmentsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteeAssignmentsQuery, List<CommitteeAssignmentDto>>
{
    public async Task<List<CommitteeAssignmentDto>> Handle(
        GetCommitteeAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.CommitteeAssignments.AsQueryable();

        if (request.CommitteeId.HasValue)
            query = query.Where(x => x.CommitteeId == request.CommitteeId.Value);
        if (request.PurchaseOrderId.HasValue)
            query = query.Where(x => x.PurchaseOrderId == request.PurchaseOrderId.Value);
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        return await query
            .OrderByDescending(x => x.AssignmentDate)
            .Select(x => new CommitteeAssignmentDto
            {
                Id = x.Id,
                CommitteeId = x.CommitteeId,
                AssignmentType = x.AssignmentType,
                PurchaseOrderId = x.PurchaseOrderId,
                AssignmentDate = x.AssignmentDate.ToDateTime(TimeOnly.MinValue),
                DecisionNumber = x.DecisionNumber,
                DecisionDate = x.DecisionDate.HasValue ? x.DecisionDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                Status = x.Status,
                RequiredSignaturesCount = x.RequiredSignaturesCount,
                ActualSignaturesCount = x.ActualSignaturesCount,
                Notes = x.Notes,
                Created = x.Created,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
