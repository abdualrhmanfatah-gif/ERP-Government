using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.AccountingEvents.ProcessEvent;

[Authorize(Policy = PermissionCodes.AccountingEventsRead)]
public class ProcessAccountingEventCommand : IRequest<Result>
{
    public int EventId { get; init; }
}

public class ProcessAccountingEventCommandHandler(
    IApplicationDbContext context,
    AccountingEventAuditor auditor) : IRequestHandler<ProcessAccountingEventCommand, Result>
{
    public async Task<Result> Handle(
        ProcessAccountingEventCommand request,
        CancellationToken cancellationToken)
    {
        // Get the event
        var accountingEvent = await context.AccountingEvents
            .FindAsync(request.EventId, cancellationToken);

        if (accountingEvent is null)
            return Result.Failure(["Accounting event not found."]);

        // Validate event is in Failed status (only failed events can be retried)
        if (accountingEvent.Status != EventStatus.Posted)
            return Result.Failure(["Only failed events can be retried."]);

        // Reset event for retry using standard auditor
        await auditor.ResetForRetryAsync(accountingEvent, cancellationToken);

        // Note: After reset, the event is now in Pending status.
        // The OutboxProcessorService will pick it up on the next poll cycle
        // and re-process it through the standard pipeline.
        // No need to manually re-raise the event — the outbox handles retries.

        return Result.Success();
    }
}

public class ProcessAccountingEventCommandValidator : AbstractValidator<ProcessAccountingEventCommand>
{
    public ProcessAccountingEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID is required.");
    }
}
