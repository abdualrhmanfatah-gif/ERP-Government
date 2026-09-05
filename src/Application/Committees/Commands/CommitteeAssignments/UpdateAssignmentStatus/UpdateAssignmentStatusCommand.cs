using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeAssignments.UpdateAssignmentStatus;

[Authorize(Policy = PermissionCodes.CommitteeAssignmentsComplete)]
public class UpdateAssignmentStatusCommand : IRequest<Result>
{
    public int Id { get; init; }
    public CommitteeAssignmentStatus Status { get; init; }
    public string? DecisionNumber { get; init; }
    public DateTime? DecisionDate { get; init; }
    public string? Notes { get; init; }
}

public class UpdateAssignmentStatusCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssignmentStatusCommand, Result>
{
    private static readonly Dictionary<CommitteeAssignmentStatus, HashSet<CommitteeAssignmentStatus>> AllowedTransitions = new()
    {
        [CommitteeAssignmentStatus.Draft] = [CommitteeAssignmentStatus.Assigned, CommitteeAssignmentStatus.Cancelled],
        [CommitteeAssignmentStatus.Assigned] = [CommitteeAssignmentStatus.InProgress, CommitteeAssignmentStatus.Cancelled],
        [CommitteeAssignmentStatus.InProgress] = [CommitteeAssignmentStatus.Completed]
    };

    public async Task<Result> Handle(
        UpdateAssignmentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CommitteeAssignments.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee assignment not found."]);

        // Validate transition
        if (!AllowedTransitions.TryGetValue(entity.Status, out var allowed)
            || !allowed.Contains(request.Status))
            return Result.Failure([$"Cannot transition from '{entity.Status}' to '{request.Status}'."]);

        // Completed requires sufficient signatures
        if (request.Status == CommitteeAssignmentStatus.Completed
            && entity.ActualSignaturesCount < entity.RequiredSignaturesCount)
            return Result.Failure([$"Cannot complete: {entity.ActualSignaturesCount}/{entity.RequiredSignaturesCount} signatures collected."]);

        entity.Status = request.Status;
        if (request.DecisionNumber is not null)
            entity.DecisionNumber = request.DecisionNumber;
        if (request.DecisionDate.HasValue)
            entity.DecisionDate = DateOnly.FromDateTime(request.DecisionDate.Value);
        if (request.Notes is not null)
            entity.Notes = request.Notes;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateAssignmentStatusCommandValidator : AbstractValidator<UpdateAssignmentStatusCommand>
{
    public UpdateAssignmentStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid assignment status.");

        RuleFor(x => x.DecisionNumber)
            .MaximumLength(50).WithMessage("Decision number must not exceed 50 characters.")
            .When(x => x.DecisionNumber is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
