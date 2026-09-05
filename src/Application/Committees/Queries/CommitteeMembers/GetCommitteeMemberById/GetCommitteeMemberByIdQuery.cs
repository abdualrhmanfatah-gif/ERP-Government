using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Committees.Common.DTOs;

namespace ERP_Government.Application.Committees.Queries.CommitteeMembers.GetCommitteeMemberById;

[Authorize(Policy = PermissionCodes.CommitteeMembersView)]
public class GetCommitteeMemberByIdQuery : IRequest<CommitteeMemberDto?>
{
    public int Id { get; init; }
}

public class GetCommitteeMemberByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCommitteeMemberByIdQuery, CommitteeMemberDto?>
{
    public async Task<CommitteeMemberDto?> Handle(
        GetCommitteeMemberByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CommitteeMembers
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
