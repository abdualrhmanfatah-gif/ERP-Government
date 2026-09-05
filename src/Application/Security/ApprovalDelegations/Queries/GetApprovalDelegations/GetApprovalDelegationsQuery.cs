using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.ApprovalDelegations.Queries.GetApprovalDelegations;

public class GetApprovalDelegationsQuery : IRequest<List<ApprovalDelegationDto>>
{
    public int? DelegateUserId { get; init; }
    public string? EntityType { get; init; }
    public string? Status { get; init; }
}

public class GetApprovalDelegationsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetApprovalDelegationsQuery, List<ApprovalDelegationDto>>
{
    public async Task<List<ApprovalDelegationDto>> Handle(
        GetApprovalDelegationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ApprovalDelegations.AsQueryable();

        if (request.DelegateUserId.HasValue)
            query = query.Where(d => d.DelegateUserId == request.DelegateUserId.Value);

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(d => d.EntityType == request.EntityType);

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<Domain.Security.Enums.DelegationStatus>(request.Status, true, out var status))
            query = query.Where(d => d.Status == status);

        return await query
            .OrderByDescending(d => d.Created)
            .Select(d => new ApprovalDelegationDto
            {
                Id = d.Id,
                DelegatorUserId = d.DelegatorUserId,
                DelegateUserId = d.DelegateUserId,
                EntityType = d.EntityType,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Status = d.Status,
                CanReDelegate = d.CanReDelegate,
                Reason = d.Reason
            })
            .ToListAsync(cancellationToken);
    }
}
