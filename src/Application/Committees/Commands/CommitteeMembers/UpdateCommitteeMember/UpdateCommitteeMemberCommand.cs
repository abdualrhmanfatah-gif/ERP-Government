using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeMembers.UpdateCommitteeMember;

public class UpdateCommitteeMemberCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string MemberName { get; init; } = string.Empty;
    public CommitteeMemberRole MemberRole { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateCommitteeMemberCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateCommitteeMemberCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCommitteeMemberCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CommitteeMembers.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee member not found."]);

        var effectiveFrom = DateOnly.FromDateTime(request.EffectiveFrom);
        DateOnly? effectiveTo = request.EffectiveTo.HasValue
            ? DateOnly.FromDateTime(request.EffectiveTo.Value)
            : (DateOnly?)null;

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            return Result.Failure(["Effective To must be after Effective From."]);

        // Single chair check (skip self)
        if (request.MemberRole == CommitteeMemberRole.Chair)
        {
            var existingChair = await context.CommitteeMembers
                .AnyAsync(x => x.CommitteeId == entity.CommitteeId
                    && x.MemberRole == CommitteeMemberRole.Chair
                    && x.IsActive
                    && x.Id != request.Id,
                    cancellationToken);
            if (existingChair)
                return Result.Failure(["Committee already has an active Chair."]);
        }

        entity.MemberName = request.MemberName;
        entity.MemberRole = request.MemberRole;
        entity.EffectiveFrom = effectiveFrom;
        entity.EffectiveTo = effectiveTo;
        entity.RowVersion = request.RowVersion;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateCommitteeMemberCommandValidator : AbstractValidator<UpdateCommitteeMemberCommand>
{
    public UpdateCommitteeMemberCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.MemberName)
            .NotEmpty().WithMessage("Member name is required.")
            .MaximumLength(200).WithMessage("Member name must not exceed 200 characters.");

        RuleFor(x => x.MemberRole)
            .IsInEnum().WithMessage("Invalid member role.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Effective from date is required.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to must be after effective from.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required.");
    }
}
