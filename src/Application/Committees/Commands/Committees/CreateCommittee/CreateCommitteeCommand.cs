using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.Committees.CreateCommittee;

[Authorize(Policy = PermissionCodes.CommitteesCreate)]
public class CreateCommitteeCommand : IRequest<Result>
{
    public string CommitteeNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CommitteeType CommitteeType { get; init; }
    public string FormationDecisionNumber { get; init; } = string.Empty;
    public DateTime FormationDecisionDate { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? Notes { get; init; }
}

public class CreateCommitteeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateCommitteeCommand, Result>
{
    public async Task<Result> Handle(
        CreateCommitteeCommand request,
        CancellationToken cancellationToken)
    {
        // Unique CommitteeNumber
        var exists = await context.Committees
            .AnyAsync(x => x.CommitteeNumber == request.CommitteeNumber, cancellationToken);
        if (exists)
            return Result.Failure([$"Committee number '{request.CommitteeNumber}' already exists."]);

        // ValidTo > ValidFrom
        if (request.ValidTo.HasValue && request.ValidTo.Value <= request.ValidFrom)
            return Result.Failure(["Valid To must be after Valid From."]);

        var entity = new Committee
        {
            CommitteeNumber = request.CommitteeNumber,
            Name = request.Name,
            CommitteeType = request.CommitteeType,
            FormationDecisionNumber = request.FormationDecisionNumber,
            FormationDecisionDate = DateOnly.FromDateTime(request.FormationDecisionDate),
            ValidFrom = DateOnly.FromDateTime(request.ValidFrom),
            ValidTo = request.ValidTo.HasValue ? DateOnly.FromDateTime(request.ValidTo.Value) : null,
            Status = CommitteeStatus.Active,
            Notes = request.Notes
        };

        context.Committees.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateCommitteeCommandValidator : AbstractValidator<CreateCommitteeCommand>
{
    public CreateCommitteeCommandValidator()
    {
        RuleFor(x => x.CommitteeNumber)
            .NotEmpty().WithMessage("Committee number is required.")
            .MaximumLength(50).WithMessage("Committee number must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.CommitteeType)
            .IsInEnum().WithMessage("Invalid committee type.");

        RuleFor(x => x.FormationDecisionNumber)
            .NotEmpty().WithMessage("Formation decision number is required.")
            .MaximumLength(50).WithMessage("Formation decision number must not exceed 50 characters.");

        RuleFor(x => x.FormationDecisionDate)
            .NotEmpty().WithMessage("Formation decision date is required.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .When(x => x.ValidTo.HasValue)
            .WithMessage("Valid to must be after valid from.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
