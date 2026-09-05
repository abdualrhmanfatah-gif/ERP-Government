using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.Commands.RevenueReceipts.ApproveRevenueReceipt;

[Authorize(Policy = PermissionCodes.RevenueReceiptsApprove)]
public class ApproveRevenueReceiptCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ApproveRevenueReceiptCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ApproveRevenueReceiptCommand, Result>
{
    public async Task<Result> Handle(
        ApproveRevenueReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RevenueReceipts.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Revenue receipt not found."]);

        // Validate lines exist
        var lineCount = await context.RevenueReceiptLines
            .CountAsync(x => x.ReceiptId == request.Id, cancellationToken);
        if (lineCount == 0)
            return Result.Failure(["Receipt must have at least one line."]);

        // Validate line sum = header total
        var lineSum = await context.RevenueReceiptLines
            .Where(x => x.ReceiptId == request.Id)
            .SumAsync(x => x.Amount, cancellationToken);
        if (lineSum != entity.AmountTotal)
            return Result.Failure([$"Line sum ({lineSum:C}) does not match header total ({entity.AmountTotal:C})."]);

        entity.Status = RevenueReceiptStatus.Approved;

        entity.AddDomainEvent(new RevenueReceiptApproved
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow
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

public class ApproveRevenueReceiptCommandValidator : AbstractValidator<ApproveRevenueReceiptCommand>
{
    public ApproveRevenueReceiptCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid receipt ID.")
            .MustAsync(async (id, ct) =>
            {
                var receipt = await context.RevenueReceipts.FindAsync(id, ct);
                return receipt is not null && receipt.Status == RevenueReceiptStatus.Draft;
            })
            .WithMessage("Revenue receipt not found or not in draft status.");
    }
}
