using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.ApprovalRules.Queries.GetApprovalRules;

public class GetApprovalRulesQuery : IRequest<List<ApprovalRuleDto>>
{
    public string? DocumentType { get; init; }
    public bool? IsActive { get; init; }
}

public class GetApprovalRulesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetApprovalRulesQuery, List<ApprovalRuleDto>>
{
    public async Task<List<ApprovalRuleDto>> Handle(
        GetApprovalRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ApprovalRules.AsQueryable();

        if (!string.IsNullOrEmpty(request.DocumentType))
            query = query.Where(r => r.DocumentType == request.DocumentType);

        if (request.IsActive.HasValue)
            query = query.Where(r => r.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(r => r.DocumentType)
            .ThenBy(r => r.Sequence)
            .Select(r => new ApprovalRuleDto
            {
                Id = r.Id,
                DocumentType = r.DocumentType,
                FundId = r.FundId,
                AmountThreshold = r.AmountThreshold,
                CurrencyId = r.CurrencyId,
                ApproverRole = r.ApproverRole,
                ApproverRoleId = r.ApproverRoleId,
                Sequence = r.Sequence,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
