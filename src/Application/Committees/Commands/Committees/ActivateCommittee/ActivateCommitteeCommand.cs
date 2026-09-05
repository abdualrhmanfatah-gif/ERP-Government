using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.Committees.ActivateCommittee;

[Authorize(Policy = PermissionCodes.CommitteesActivate)]
public class ActivateCommitteeCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ActivateCommitteeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ActivateCommitteeCommand, Result>
{
    public async Task<Result> Handle(
        ActivateCommitteeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Committees.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee not found."]);

        if (entity.Status == CommitteeStatus.Active)
            return Result.Failure(["Committee is already active."]);

        if (entity.Status == CommitteeStatus.Dissolved)
            return Result.Failure(["Cannot activate a dissolved committee."]);

        entity.Status = CommitteeStatus.Active;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ActivateCommitteeCommandValidator : AbstractValidator<ActivateCommitteeCommand>
{
    public ActivateCommitteeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
