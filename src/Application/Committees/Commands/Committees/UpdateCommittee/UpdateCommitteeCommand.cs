using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.Committees.UpdateCommittee;

[Authorize(Policy = PermissionCodes.CommitteesUpdate)]
public class UpdateCommitteeCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? FormationDecisionNumber { get; init; }
    public DateTime? FormationDecisionDate { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public string? Notes { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateCommitteeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateCommitteeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCommitteeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Committees.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee not found."]);

        // Cannot update dissolved committees
        if (entity.Status == CommitteeStatus.Dissolved)
            return Result.Failure(["Cannot update a dissolved committee."]);

        var validFrom = request.ValidFrom.HasValue
            ? DateOnly.FromDateTime(request.ValidFrom.Value)
            : entity.ValidFrom;
        var validTo = request.ValidTo.HasValue
            ? DateOnly.FromDateTime(request.ValidTo.Value)
            : entity.ValidTo;

        if (validTo.HasValue && validTo.Value <= validFrom)
            return Result.Failure(["Valid To must be after Valid From."]);

        entity.Name = request.Name;
        if (request.FormationDecisionNumber is not null)
            entity.FormationDecisionNumber = request.FormationDecisionNumber;
        if (request.FormationDecisionDate.HasValue)
            entity.FormationDecisionDate = DateOnly.FromDateTime(request.FormationDecisionDate.Value);
        if (request.ValidFrom.HasValue)
            entity.ValidFrom = DateOnly.FromDateTime(request.ValidFrom.Value);
        if (request.ValidTo.HasValue)
            entity.ValidTo = DateOnly.FromDateTime(request.ValidTo.Value);
        entity.Notes = request.Notes;
        entity.RowVersion = request.RowVersion;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateCommitteeCommandValidator : AbstractValidator<UpdateCommitteeCommand>
{
    public UpdateCommitteeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.FormationDecisionNumber)
            .MaximumLength(50).WithMessage("Formation decision number must not exceed 50 characters.")
            .When(x => x.FormationDecisionNumber is not null);

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .When(x => x.ValidTo.HasValue && x.ValidFrom.HasValue)
            .WithMessage("Valid to must be after valid from.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required.");
    }
}
