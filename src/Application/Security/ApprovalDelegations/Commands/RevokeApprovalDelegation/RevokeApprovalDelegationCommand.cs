using ERP_Government.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace ERP_Government.Application.Security.ApprovalDelegations.Commands.RevokeApprovalDelegation;

public class RevokeApprovalDelegationCommand : IRequest
{
    public int Id { get; init; }
}

public class RevokeApprovalDelegationCommandValidator : AbstractValidator<RevokeApprovalDelegationCommand>
{
    public RevokeApprovalDelegationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid delegation ID.");
    }
}

public class RevokeApprovalDelegationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RevokeApprovalDelegationCommand>
{
    public async Task Handle(
        RevokeApprovalDelegationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ApprovalDelegations.FindAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Approval delegation with ID {request.Id} not found.");

        entity.Status = Domain.Security.Enums.DelegationStatus.Revoked;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
