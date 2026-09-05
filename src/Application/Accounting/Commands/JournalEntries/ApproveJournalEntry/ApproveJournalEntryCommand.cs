using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.ApproveJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesApprove)]
public class ApproveJournalEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveJournalEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ApproveJournalEntryCommand, Result>
{
    public async Task<Result> Handle(
        ApproveJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry not found."]);

        // Validate lifecycle: only Submitted entries can be approved
        if (entity.EntryStatus != EntryStatus.Submitted)
            return Result.Failure(["Only submitted journal entries can be approved."]);

        // Update status
        entity.EntryStatus = EntryStatus.Approved;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ApproveJournalEntryCommandValidator : AbstractValidator<ApproveJournalEntryCommand>
{
    public ApproveJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
