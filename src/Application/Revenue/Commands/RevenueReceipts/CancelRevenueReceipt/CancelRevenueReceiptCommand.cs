using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.Commands.RevenueReceipts.CancelRevenueReceipt;

[Authorize(Policy = PermissionCodes.RevenueReceiptsCancel)]
public class CancelRevenueReceiptCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}

public class CancelRevenueReceiptCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelRevenueReceiptCommand, Result>
{
    public async Task<Result> Handle(
        CancelRevenueReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RevenueReceipts.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Revenue receipt not found."]);

        entity.CancellationReason = request.Reason;
        entity.Status = RevenueReceiptStatus.Cancelled;

        entity.AddDomainEvent(new RevenueReceiptCancelled
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            RevenueReceiptId = entity.Id,
            Reason = request.Reason ?? string.Empty
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(["Receipt was modified by another user. Please refresh and try again."]);
        }

        return Result.Success();
    }
}

public class CancelRevenueReceiptCommandValidator : AbstractValidator<CancelRevenueReceiptCommand>
{
    public CancelRevenueReceiptCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid receipt ID.")
            .MustAsync(async (id, ct) =>
            {
                var receipt = await context.RevenueReceipts.FindAsync(id, ct);
                if (receipt is null) return false;
                return receipt.Status != RevenueReceiptStatus.Cancelled
                    && receipt.Status != RevenueReceiptStatus.Posted;
            })
            .WithMessage("Revenue receipt not found, already cancelled, or posted.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
