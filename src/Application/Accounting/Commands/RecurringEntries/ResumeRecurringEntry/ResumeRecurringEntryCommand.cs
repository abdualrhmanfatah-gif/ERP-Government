using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.RecurringEntries.ResumeRecurringEntry;

[Authorize(Policy = PermissionCodes.RecurringEntriesResume)]
public class ResumeRecurringEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ResumeRecurringEntryCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ResumeRecurringEntryCommand, Result>
{
    public async Task<Result> Handle(
        ResumeRecurringEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.RecurringEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Recurring entry not found."]);

        if (entity.Status != RecurringEntryStatus.Paused)
            return Result.Failure(["Only paused recurring entries can be resumed."]);

        var previousStatus = entity.Status;
        entity.Status = RecurringEntryStatus.Active;

        await statusLogger.LogAsync(
            "RecurringEntry",
            entity.Id,
            previousStatus.ToString(),
            RecurringEntryStatus.Active.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ResumeRecurringEntryCommandValidator : AbstractValidator<ResumeRecurringEntryCommand>
{
    public ResumeRecurringEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid recurring entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
