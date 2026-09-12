using ERP_Government.Domain.Events.Payments;
using MediatR;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Forwarding handler — bridges PaymentOrderExecuted to PostingPipelineHandler.
/// MediatR notification handlers are not covariant, so INotificationHandler&lt;BaseEvent&gt;
/// does not catch derived events. Each concrete event needs its own handler.
/// </summary>
public class PaymentOrderExecutedHandler : INotificationHandler<PaymentOrderExecuted>
{
    private readonly PostingPipelineHandler _pipeline;

    public PaymentOrderExecutedHandler(PostingPipelineHandler pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task Handle(PaymentOrderExecuted notification, CancellationToken cancellationToken)
    {
        await _pipeline.Handle(notification, cancellationToken);
    }
}
