using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeMembers.ActivateMember;

[Authorize(Policy = PermissionCodes.CommitteeMembersAdd)]
public class ActivateMemberCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ActivateMemberCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ActivateMemberCommand, Result>
{
    public async Task<Result> Handle(
        ActivateMemberCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CommitteeMembers.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee member not found."]);

        if (entity.IsActive)
            return Result.Failure(["Member is already active."]);

        // Cannot activate if past EffectiveTo
        if (entity.EffectiveTo.HasValue && entity.EffectiveTo.Value < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(["Cannot activate member past their Effective To date."]);

        // Committee must be active
        var committee = await context.Committees.FindAsync(entity.CommitteeId, cancellationToken);
        if (committee is null || committee.Status != CommitteeStatus.Active)
            return Result.Failure(["Committee must be active to activate members."]);

        entity.IsActive = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ActivateMemberCommandValidator : AbstractValidator<ActivateMemberCommand>
{
    public ActivateMemberCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
