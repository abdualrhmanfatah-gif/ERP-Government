using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.Committees.DissolveCommittee;

[Authorize(Policy = PermissionCodes.CommitteesDissolve)]
public class DissolveCommitteeCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DissolveCommitteeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DissolveCommitteeCommand, Result>
{
    public async Task<Result> Handle(
        DissolveCommitteeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Committees.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee not found."]);

        if (entity.Status == CommitteeStatus.Dissolved)
            return Result.Failure(["Committee is already dissolved."]);

        // Check active assignments
        var hasActiveAssignments = await context.CommitteeAssignments
            .AnyAsync(x => x.CommitteeId == request.Id
                && x.Status != CommitteeAssignmentStatus.Completed
                && x.Status != CommitteeAssignmentStatus.Cancelled,
                cancellationToken);
        if (hasActiveAssignments)
            return Result.Failure(["Cannot dissolve committee with active assignments."]);

        // Deactivate all active members
        var activeMembers = await context.CommitteeMembers
            .Where(x => x.CommitteeId == request.Id && x.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var member in activeMembers)
            member.IsActive = false;

        entity.Status = CommitteeStatus.Dissolved;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DissolveCommitteeCommandValidator : AbstractValidator<DissolveCommitteeCommand>
{
    public DissolveCommitteeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
