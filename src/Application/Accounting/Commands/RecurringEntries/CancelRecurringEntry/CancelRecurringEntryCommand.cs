using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.RecurringEntries.CancelRecurringEntry;

[Authorize(Policy = PermissionCodes.RecurringEntriesCancel)]
public class CancelRecurringEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class CancelRecurringEntryCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CancelRecurringEntryCommand, Result>
{
    public async Task<Result> Handle(
        CancelRecurringEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.RecurringEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Recurring entry not found."]);

        if (entity.Status == RecurringEntryStatus.Completed)
            return Result.Failure(["Completed recurring entries cannot be cancelled."]);

        if (entity.Status == RecurringEntryStatus.Cancelled)
            return Result.Failure(["Cancelled recurring entries cannot be cancelled."]);

        var previousStatus = entity.Status;
        entity.Status = RecurringEntryStatus.Cancelled;
        entity.IsActive = false;

        await statusLogger.LogAsync(
            "RecurringEntry",
            entity.Id,
            previousStatus.ToString(),
            RecurringEntryStatus.Cancelled.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelRecurringEntryCommandValidator : AbstractValidator<CancelRecurringEntryCommand>
{
    public CancelRecurringEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid recurring entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
