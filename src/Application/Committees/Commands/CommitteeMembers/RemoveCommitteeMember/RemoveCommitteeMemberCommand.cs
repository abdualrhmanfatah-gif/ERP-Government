using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Committees.Commands.CommitteeMembers.RemoveCommitteeMember;

[Authorize(Policy = PermissionCodes.CommitteeMembersRemove)]
public class RemoveCommitteeMemberCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class RemoveCommitteeMemberCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveCommitteeMemberCommand, Result>
{
    public async Task<Result> Handle(
        RemoveCommitteeMemberCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CommitteeMembers.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee member not found."]);

        if (!entity.IsActive)
            return Result.Failure(["Member is already inactive."]);

        entity.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RemoveCommitteeMemberCommandValidator : AbstractValidator<RemoveCommitteeMemberCommand>
{
    public RemoveCommitteeMemberCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
