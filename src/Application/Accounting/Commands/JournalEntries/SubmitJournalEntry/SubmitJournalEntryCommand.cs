using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.SubmitJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesSubmit)]
public class SubmitJournalEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitJournalEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SubmitJournalEntryCommand, Result>
{
    public async Task<Result> Handle(
        SubmitJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry not found."]);

        // Validate lifecycle: only Draft entries can be submitted
        if (entity.EntryStatus != EntryStatus.Draft)
            return Result.Failure(["Only draft journal entries can be submitted."]);

        // Validate entry has lines
        var hasLines = await context.JournalEntryLines
            .AnyAsync(x => x.JournalEntryId == request.Id, cancellationToken);

        if (!hasLines)
            return Result.Failure(["Journal entry must have at least one line."]);

        // Update status
        entity.EntryStatus = EntryStatus.Submitted;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SubmitJournalEntryCommandValidator : AbstractValidator<SubmitJournalEntryCommand>
{
    public SubmitJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
