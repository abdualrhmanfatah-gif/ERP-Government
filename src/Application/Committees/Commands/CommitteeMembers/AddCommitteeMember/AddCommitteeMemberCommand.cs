using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeMembers.AddCommitteeMember;

[Authorize(Policy = PermissionCodes.CommitteeMembersAdd)]
public class AddCommitteeMemberCommand : IRequest<Result>
{
    public int CommitteeId { get; init; }
    public int? EmployeeId { get; init; }
    public string MemberName { get; init; } = string.Empty;
    public CommitteeMemberRole MemberRole { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public class AddCommitteeMemberCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AddCommitteeMemberCommand, Result>
{
    public async Task<Result> Handle(
        AddCommitteeMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Committee must exist and be active
        var committee = await context.Committees.FindAsync(request.CommitteeId, cancellationToken);
        if (committee is null)
            return Result.Failure(["Committee not found."]);
        if (committee.Status != CommitteeStatus.Active)
            return Result.Failure(["Committee must be active to add members."]);

        // Employee must exist if provided
        if (request.EmployeeId.HasValue)
        {
            var employeeExists = await context.Employees
                .AnyAsync(x => x.Id == request.EmployeeId.Value, cancellationToken);
            if (!employeeExists)
                return Result.Failure(["Employee not found."]);
        }

        var effectiveFrom = DateOnly.FromDateTime(request.EffectiveFrom);
        DateOnly? effectiveTo = request.EffectiveTo.HasValue
            ? DateOnly.FromDateTime(request.EffectiveTo.Value)
            : (DateOnly?)null;

        // Date validation
        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            return Result.Failure(["Effective To must be after Effective From."]);

        // No overlapping employee in same committee
        if (request.EmployeeId.HasValue)
        {
            var hasOverlap = await context.CommitteeMembers
                .AnyAsync(x => x.CommitteeId == request.CommitteeId
                    && x.EmployeeId == request.EmployeeId.Value
                    && x.IsActive
                    && (x.EffectiveTo == null || x.EffectiveTo > effectiveFrom)
                    && x.EffectiveFrom < (effectiveTo ?? DateOnly.MaxValue),
                    cancellationToken);
            if (hasOverlap)
                return Result.Failure(["Employee is already an active member of this committee for the overlapping period."]);
        }

        // Single chair check (INFERRED)
        if (request.MemberRole == CommitteeMemberRole.Chair)
        {
            var existingChair = await context.CommitteeMembers
                .AnyAsync(x => x.CommitteeId == request.CommitteeId
                    && x.MemberRole == CommitteeMemberRole.Chair
                    && x.IsActive,
                    cancellationToken);
            if (existingChair)
                return Result.Failure(["Committee already has an active Chair."]);
        }

        var entity = new CommitteeMember
        {
            CommitteeId = request.CommitteeId,
            EmployeeId = request.EmployeeId,
            MemberName = request.MemberName,
            MemberRole = request.MemberRole,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true
        };

        context.CommitteeMembers.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class AddCommitteeMemberCommandValidator : AbstractValidator<AddCommitteeMemberCommand>
{
    public AddCommitteeMemberCommandValidator()
    {
        RuleFor(x => x.CommitteeId)
            .GreaterThan(0).WithMessage("Committee id must be greater than 0.");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Employee id must be greater than 0.")
            .When(x => x.EmployeeId.HasValue);

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
    }
}
