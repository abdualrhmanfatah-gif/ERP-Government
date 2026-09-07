using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
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
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<PauseRecurringEntryCommand, Result>
{
    public async Task<Result> Handle(
        PauseRecurringEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.RecurringEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Recurring entry not found."]);

        if (entity.Status != RecurringEntryStatus.Active)
            return Result.Failure(["Only active recurring entries can be paused."]);

        var previousStatus = entity.Status;
        entity.Status = RecurringEntryStatus.Paused;

        await statusLogger.LogAsync(
            "RecurringEntry",
            entity.Id,
            previousStatus.ToString(),
            RecurringEntryStatus.Paused.ToString(),
            userId,
            request.Reason,
            cancellationToken);

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
