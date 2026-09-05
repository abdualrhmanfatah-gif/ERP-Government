using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Application.Accounting.Commands.AccountingEvents.RetryEvent;

[Authorize(Policy = PermissionCodes.AccountingEventsRead)]
public class RetryAccountingEventCommand : IRequest<Result>
{
    public int EventId { get; init; }
}

public class RetryAccountingEventCommandHandler(
    IApplicationDbContext context,
    AccountingEventAuditor auditor) : IRequestHandler<RetryAccountingEventCommand, Result>
{
    public async Task<Result> Handle(
        RetryAccountingEventCommand request,
        CancellationToken cancellationToken)
    {
        var accountingEvent = await context.AccountingEvents
            .FindAsync(request.EventId, cancellationToken);

        if (accountingEvent is null)
            return Result.Failure(["Accounting event not found."]);

        if (accountingEvent.Status != EventStatus.Posted)
            return Result.Failure(["Only failed events can be retried."]);

        if (accountingEvent.RetryCount < 3)
            return Result.Failure(["Automatic retry is still available. Manual retry is only allowed after 3 failed attempts."]);

        // Reset for retry
        await auditor.ResetForRetryAsync(accountingEvent, cancellationToken);

        // Re-raise the domain event to re-invoke the pipeline
        // The event type name maps to a domain event class — we create a minimal re-dispatch
        // by updating the AccountingEvent back to Pending and letting the pipeline pick it up
        return Result.Success();
    }
}

public class RetryAccountingEventCommandValidator : AbstractValidator<RetryAccountingEventCommand>
{
    public RetryAccountingEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID is required.");
    }
}
