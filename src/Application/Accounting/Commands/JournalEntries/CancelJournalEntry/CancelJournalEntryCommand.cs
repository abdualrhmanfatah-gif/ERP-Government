using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.CancelJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesCancel)]
public class CancelJournalEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class CancelJournalEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelJournalEntryCommand, Result>
{
    public async Task<Result> Handle(
        CancelJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry not found."]);

        if (entity.EntryStatus != EntryStatus.Draft && entity.EntryStatus != EntryStatus.Submitted)
            return Result.Failure(["Only draft or submitted journal entries can be cancelled."]);

        if (entity.EntryStatus == EntryStatus.Cancelled)
            return Result.Failure(["Journal entry is already cancelled."]);

        entity.EntryStatus = EntryStatus.Cancelled;
        entity.CancelledAt = DateTimeOffset.UtcNow;
        // CancelledById left null — current user tracking via IUser not wired for this field yet; audit via CreatedBy/Interceptor covers actor

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

public class CancelJournalEntryCommandValidator : AbstractValidator<CancelJournalEntryCommand>
{
    public CancelJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
