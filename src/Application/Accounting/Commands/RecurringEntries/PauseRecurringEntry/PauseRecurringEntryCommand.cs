using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.RecurringEntries.PauseRecurringEntry;

[Authorize(Policy = PermissionCodes.RecurringEntriesPause)]
public class PauseRecurringEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class PauseRecurringEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<PauseRecurringEntryCommand, Result>
{
    public async Task<Result> Handle(
        PauseRecurringEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RecurringEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Recurring entry not found."]);

        // Validate lifecycle: only Active can be paused
        if (entity.Status != RecurringEntryStatus.Active)
            return Result.Failure(["Only active recurring entries can be paused."]);

        // Update status
        entity.Status = RecurringEntryStatus.Paused;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class PauseRecurringEntryCommandValidator : AbstractValidator<PauseRecurringEntryCommand>
{
    public PauseRecurringEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid recurring entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
