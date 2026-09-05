using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeAssignments.CreateCommitteeAssignment;

[Authorize(Policy = PermissionCodes.CommitteeAssignmentsCreate)]
public class CreateCommitteeAssignmentCommand : IRequest<Result>
{
    public int CommitteeId { get; init; }
    public CommitteeAssignmentType AssignmentType { get; init; }
    public int? PurchaseOrderId { get; init; }
    public DateTime AssignmentDate { get; init; }
    public string? DecisionNumber { get; init; }
    public DateTime? DecisionDate { get; init; }
    public int RequiredSignaturesCount { get; init; } = 1;
    public string? Notes { get; init; }
}

public class CreateCommitteeAssignmentCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateCommitteeAssignmentCommand, Result>
{
    public async Task<Result> Handle(
        CreateCommitteeAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        // Committee must exist and be active
        var committee = await context.Committees.FindAsync(request.CommitteeId, cancellationToken);
        if (committee is null)
            return Result.Failure(["Committee not found."]);
        if (committee.Status != CommitteeStatus.Active)
            return Result.Failure(["Committee must be active for assignments."]);

        // Quorum check: at least 3 active members (recommendation — INFERRED)
        var activeMemberCount = await context.CommitteeMembers
            .CountAsync(x => x.CommitteeId == request.CommitteeId && x.IsActive, cancellationToken);
        if (activeMemberCount < 3)
            return Result.Failure(["Committee must have at least 3 active members (quorum)."]);

        // AssignmentDate within committee validity
        var assignmentDate = DateOnly.FromDateTime(request.AssignmentDate);
        if (assignmentDate < committee.ValidFrom)
            return Result.Failure(["Assignment date cannot be before committee Valid From."]);
        if (committee.ValidTo.HasValue && assignmentDate > committee.ValidTo.Value)
            return Result.Failure(["Assignment date cannot be after committee Valid To."]);

        // At least one document reference
        if (!request.PurchaseOrderId.HasValue)
            return Result.Failure(["Purchase order reference is required."]);

        // Validate PurchaseOrder
        var poExists = await context.PurchaseOrders
            .AnyAsync(x => x.Id == request.PurchaseOrderId.Value, cancellationToken);
        if (!poExists)
            return Result.Failure(["Purchase order not found."]);

        // RequiredSignaturesCount >= 1
        if (request.RequiredSignaturesCount < 1)
            return Result.Failure(["Required signatures count must be at least 1."]);

        DateOnly? decisionDate = request.DecisionDate.HasValue
            ? DateOnly.FromDateTime(request.DecisionDate.Value)
            : (DateOnly?)null;

        var entity = new CommitteeAssignment
        {
            CommitteeId = request.CommitteeId,
            AssignmentType = request.AssignmentType,
            PurchaseOrderId = request.PurchaseOrderId,
            AssignmentDate = assignmentDate,
            DecisionNumber = request.DecisionNumber,
            DecisionDate = decisionDate,
            Status = CommitteeAssignmentStatus.Draft,
            RequiredSignaturesCount = request.RequiredSignaturesCount,
            ActualSignaturesCount = 0,
            Notes = request.Notes
        };

        context.CommitteeAssignments.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateCommitteeAssignmentCommandValidator : AbstractValidator<CreateCommitteeAssignmentCommand>
{
    public CreateCommitteeAssignmentCommandValidator()
    {
        RuleFor(x => x.CommitteeId)
            .GreaterThan(0).WithMessage("Committee id must be greater than 0.");

        RuleFor(x => x.AssignmentType)
            .IsInEnum().WithMessage("Invalid assignment type.");

        RuleFor(x => x.PurchaseOrderId)
            .GreaterThan(0).WithMessage("Purchase order id must be greater than 0.")
            .When(x => x.PurchaseOrderId.HasValue);

        RuleFor(x => x.AssignmentDate)
            .NotEmpty().WithMessage("Assignment date is required.");

        RuleFor(x => x.DecisionNumber)
            .MaximumLength(50).WithMessage("Decision number must not exceed 50 characters.")
            .When(x => x.DecisionNumber is not null);

        RuleFor(x => x.RequiredSignaturesCount)
            .GreaterThanOrEqualTo(1).WithMessage("Required signatures count must be at least 1.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
