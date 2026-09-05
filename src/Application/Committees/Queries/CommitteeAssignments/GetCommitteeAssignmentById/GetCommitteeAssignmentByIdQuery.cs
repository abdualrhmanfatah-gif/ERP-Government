using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;

namespace ERP_Government.Application.Committees.Queries.CommitteeAssignments.GetCommitteeAssignmentById;

[Authorize(Policy = PermissionCodes.CommitteeAssignmentsView)]
public class GetCommitteeAssignmentByIdQuery : IRequest<CommitteeAssignmentDto?>
{
    public int Id { get; init; }
}

public class GetCommitteeAssignmentByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteeAssignmentByIdQuery, CommitteeAssignmentDto?>
{
    public async Task<CommitteeAssignmentDto?> Handle(
        GetCommitteeAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CommitteeAssignments
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
