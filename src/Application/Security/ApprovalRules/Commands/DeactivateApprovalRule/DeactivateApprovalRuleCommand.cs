using ERP_Government.Application.Common.Interfaces;
using MediatR;

namespace ERP_Government.Application.Security.ApprovalRules.Commands.DeactivateApprovalRule;

public class DeactivateApprovalRuleCommand : IRequest
{
    public int Id { get; init; }
}

public class DeactivateApprovalRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateApprovalRuleCommand>
{
    public async Task Handle(
        DeactivateApprovalRuleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ApprovalRules.FindAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Approval rule with ID {request.Id} not found.");

        entity.IsActive = false;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
