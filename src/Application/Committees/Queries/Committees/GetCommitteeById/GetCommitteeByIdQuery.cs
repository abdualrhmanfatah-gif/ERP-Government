using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;

namespace ERP_Government.Application.Committees.Queries.Committees.GetCommitteeById;

[Authorize(Policy = PermissionCodes.CommitteesView)]
public class GetCommitteeByIdQuery : IRequest<CommitteeDto?>
{
    public int Id { get; init; }
}

public class GetCommitteeByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteeByIdQuery, CommitteeDto?>
{
    public async Task<CommitteeDto?> Handle(
        GetCommitteeByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Committees
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
