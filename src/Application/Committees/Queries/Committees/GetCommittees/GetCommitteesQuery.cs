using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Queries.Committees.GetCommittees;

[Authorize(Policy = PermissionCodes.CommitteesView)]
public class GetCommitteesQuery : IRequest<List<CommitteeDto>>
{
    public CommitteeStatus? Status { get; init; }
    public CommitteeType? CommitteeType { get; init; }
}

public class GetCommitteesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteesQuery, List<CommitteeDto>>
{
    public async Task<List<CommitteeDto>> Handle(
        GetCommitteesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Committees.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.CommitteeType.HasValue)
            query = query.Where(x => x.CommitteeType == request.CommitteeType.Value);

        return await query
            .OrderByDescending(x => x.Created)
            .Select(x => new CommitteeDto
            {
                Id = x.Id,
                CommitteeNumber = x.CommitteeNumber,
                Name = x.Name,
                CommitteeType = x.CommitteeType,
                FormationDecisionNumber = x.FormationDecisionNumber,
                FormationDecisionDate = x.FormationDecisionDate.ToDateTime(TimeOnly.MinValue),
                ValidFrom = x.ValidFrom.ToDateTime(TimeOnly.MinValue),
                ValidTo = x.ValidTo.HasValue ? x.ValidTo.Value.ToDateTime(TimeOnly.MinValue) : null,
                Status = x.Status,
                Notes = x.Notes,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
