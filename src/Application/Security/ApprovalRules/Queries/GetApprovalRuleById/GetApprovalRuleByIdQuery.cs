using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.ApprovalRules.Queries.GetApprovalRuleById;

public class GetApprovalRuleByIdQuery : IRequest<ApprovalRuleDto?>
{
    public int Id { get; init; }
}

public class GetApprovalRuleByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetApprovalRuleByIdQuery, ApprovalRuleDto?>
{
    public async Task<ApprovalRuleDto?> Handle(
        GetApprovalRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ApprovalRules
            .Where(r => r.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
