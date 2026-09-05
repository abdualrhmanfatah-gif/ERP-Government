using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Queries.CommitteeMembers.GetCommitteeMembers;

[Authorize(Policy = PermissionCodes.CommitteeMembersView)]
public class GetCommitteeMembersQuery : IRequest<List<CommitteeMemberDto>>
{
    public int CommitteeId { get; init; }
    public bool? IsActive { get; init; }
    public CommitteeMemberRole? MemberRole { get; init; }
}

public class GetCommitteeMembersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteeMembersQuery, List<CommitteeMemberDto>>
{
    public async Task<List<CommitteeMemberDto>> Handle(
        GetCommitteeMembersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.CommitteeMembers
            .Where(x => x.CommitteeId == request.CommitteeId);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.MemberRole.HasValue)
            query = query.Where(x => x.MemberRole == request.MemberRole.Value);

        return await query
            .OrderBy(x => x.MemberRole)
            .ThenBy(x => x.MemberName)
            .Select(x => new CommitteeMemberDto
            {
                Id = x.Id,
                CommitteeId = x.CommitteeId,
                EmployeeId = x.EmployeeId,
                MemberName = x.MemberName,
                MemberRole = x.MemberRole,
                EffectiveFrom = x.EffectiveFrom.ToDateTime(TimeOnly.MinValue),
                EffectiveTo = x.EffectiveTo.HasValue ? x.EffectiveTo.Value.ToDateTime(TimeOnly.MinValue) : null,
                IsActive = x.IsActive,
                Created = x.Created,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
