using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntryLines.RemoveJournalEntryLine;

[Authorize(Policy = PermissionCodes.JournalEntriesUpdateLines)]
public class RemoveJournalEntryLineCommand : IRequest<Result>
{
    public long Id { get; init; }
    public int JournalEntryId { get; init; }
}

public class RemoveJournalEntryLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveJournalEntryLineCommand, Result>
{
    public async Task<Result> Handle(
        RemoveJournalEntryLineCommand request,
        CancellationToken cancellationToken)
    {
        // Validate JournalEntry exists and is in Draft status
        var journalEntry = await context.JournalEntries
            .FindAsync(request.JournalEntryId, cancellationToken);

        if (journalEntry is null)
            return Result.Failure(["Journal entry not found."]);

        if (journalEntry.EntryStatus != EntryStatus.Draft)
            return Result.Failure(["Can only remove lines from journal entries in Draft status."]);

        // Validate JournalEntryLine exists
        var entity = await context.JournalEntryLines
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry line not found."]);

        if (entity.JournalEntryId != request.JournalEntryId)
            return Result.Failure(["Journal entry line does not belong to the specified journal entry."]);

        context.JournalEntryLines.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RemoveJournalEntryLineCommandValidator : AbstractValidator<RemoveJournalEntryLineCommand>
{
    public RemoveJournalEntryLineCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry line ID.");

        RuleFor(x => x.JournalEntryId)
            .GreaterThan(0).WithMessage("Journal entry is required.");
    }
}
