using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.ApprovalRules.Commands.DeactivateApprovalRule;

[Authorize(Policy = PermissionCodes.ApprovalRulesManage)]
public class DeactivateApprovalRuleCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeactivateApprovalRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateApprovalRuleCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateApprovalRuleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ApprovalRules.FindAsync(request.Id, cancellationToken);

        if (entity == null)
            return Result.Failure(ErrorCodes.SecurityApprovalRules.NotFound, ErrorCategory.NotFound, $"Approval rule with ID {request.Id} not found.");

        entity.IsActive = false;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
