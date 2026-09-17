using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.ApprovalRules.Queries.GetApprovalRuleById;

[Authorize(Policy = PermissionCodes.ApprovalRulesView)]
public class GetApprovalRuleByIdQuery : IRequest<Result<ApprovalRuleDto>>
{
    public int Id { get; init; }
}

public class GetApprovalRuleByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetApprovalRuleByIdQuery, Result<ApprovalRuleDto>>
{
    public async Task<Result<ApprovalRuleDto>> Handle(
        GetApprovalRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.ApprovalRules
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

        if (dto is null)
            return Result<ApprovalRuleDto>.Failure(ErrorCodes.SecurityApprovalRules.NotFound, ErrorCategory.NotFound, $"Approval rule with ID {request.Id} not found.");

        return Result<ApprovalRuleDto>.Success(dto);
    }
}
