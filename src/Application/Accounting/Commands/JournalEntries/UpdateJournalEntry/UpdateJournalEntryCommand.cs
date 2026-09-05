using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.UpdateJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesCreate)]
public class UpdateJournalEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Narration { get; init; }
    public string? Ref { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateJournalEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateJournalEntryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry not found."]);

        if (entity.EntryStatus != EntryStatus.Draft)
            return Result.Failure(["Only draft journal entries can be edited."]);

        entity.Narration = request.Narration;
        entity.Ref = request.Ref;

        // Concurrency check via RowVersion — EF will throw on mismatch
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(["RowVersion conflict — record modified by another user. Reload."]);
        }

        return Result.Success();
    }
}

public class UpdateJournalEntryCommandValidator : AbstractValidator<UpdateJournalEntryCommand>
{
    public UpdateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");

        RuleFor(x => x.Narration)
            .MaximumLength(1000).WithMessage("Narration must not exceed 1000 characters.");

        RuleFor(x => x.Ref)
            .MaximumLength(100).WithMessage("Ref must not exceed 100 characters.");
    }
}
