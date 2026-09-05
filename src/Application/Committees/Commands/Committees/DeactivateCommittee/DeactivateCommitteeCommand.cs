using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.Committees.DeactivateCommittee;

[Authorize(Policy = PermissionCodes.CommitteesDeactivate)]
public class DeactivateCommitteeCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeactivateCommitteeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateCommitteeCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateCommitteeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Committees.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee not found."]);

        if (entity.Status == CommitteeStatus.Inactive)
            return Result.Failure(["Committee is already inactive."]);

        if (entity.Status == CommitteeStatus.Dissolved)
            return Result.Failure(["Cannot deactivate a dissolved committee."]);

        // Check active assignments
        var hasActiveAssignments = await context.CommitteeAssignments
            .AnyAsync(x => x.CommitteeId == request.Id
                && x.Status != CommitteeAssignmentStatus.Completed
                && x.Status != CommitteeAssignmentStatus.Cancelled,
                cancellationToken);
        if (hasActiveAssignments)
            return Result.Failure(["Cannot deactivate committee with active assignments."]);

        entity.Status = CommitteeStatus.Inactive;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeactivateCommitteeCommandValidator : AbstractValidator<DeactivateCommitteeCommand>
{
    public DeactivateCommitteeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
