using ERP_Government.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace ERP_Government.Application.Security.ApprovalDelegations.Commands.CreateApprovalDelegation;

public class CreateApprovalDelegationCommand : IRequest<int>
{
    public int DelegatorUserId { get; init; }
    public int DelegateUserId { get; init; }
    public string? EntityType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool CanReDelegate { get; init; }
    public string? Reason { get; init; }
}

public class CreateApprovalDelegationCommandValidator : AbstractValidator<CreateApprovalDelegationCommand>
{
    public CreateApprovalDelegationCommandValidator()
    {
        RuleFor(x => x.DelegatorUserId)
            .GreaterThan(0).WithMessage("Delegator user ID is required.");

        RuleFor(x => x.DelegateUserId)
            .GreaterThan(0).WithMessage("Delegate user ID is required.");

        RuleFor(x => x.DelegateUserId)
            .NotEqual(x => x.DelegatorUserId).WithMessage("Delegator and delegate cannot be the same user.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after start date.");

        RuleFor(x => x.EntityType)
            .MaximumLength(50).WithMessage("Entity type must not exceed 50 characters.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}

public class CreateApprovalDelegationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateApprovalDelegationCommand, int>
{
    public async Task<int> Handle(
        CreateApprovalDelegationCommand request,
        CancellationToken cancellationToken)
    {
        var delegatorExists = await context.Users.AnyAsync(u => u.Id == request.DelegatorUserId, cancellationToken);
        if (!delegatorExists)
            throw new InvalidOperationException($"Delegator user with ID {request.DelegatorUserId} not found.");

        var delegateExists = await context.Users.AnyAsync(u => u.Id == request.DelegateUserId, cancellationToken);
        if (!delegateExists)
            throw new InvalidOperationException($"Delegate user with ID {request.DelegateUserId} not found.");

        var entity = new Domain.Security.Entities.ApprovalDelegation
        {
            DelegatorUserId = request.DelegatorUserId,
            DelegateUserId = request.DelegateUserId,
            EntityType = request.EntityType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = Domain.Security.Enums.DelegationStatus.Active,
            CanReDelegate = request.CanReDelegate,
            Reason = request.Reason,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.ApprovalDelegations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
