using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Events.Revenue;

namespace ERP_Government.Application.Revenue.Commands.RevenueReceipts.PostRevenueReceipt;

[Authorize(Policy = PermissionCodes.RevenueReceiptsPost)]
public class PostRevenueReceiptCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class PostRevenueReceiptCommandHandler(
    IApplicationDbContext context) : IRequestHandler<PostRevenueReceiptCommand, Result>
{
    public async Task<Result> Handle(
        PostRevenueReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RevenueReceipts.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(new[] { "Revenue receipt not found."});

        // Idempotency: already posted → return success
        if (entity.JournalEntryId.HasValue)
            return Result.Success();

        // Resolve journal from PostingRules
        var postingRule = await context.PostingRules
            .FirstOrDefaultAsync(x => x.EventType == "RevenueReceiptPosted" && x.IsActive, cancellationToken);
        if (postingRule is null)
            return Result.Failure(new[] { "No posting rule configured for revenue receipts."});

        // Resolve debit account from Fund default
        var fund = await context.Funds.FindAsync(entity.FundId, cancellationToken);
        if (fund is null || fund.DefaultRevenueDebitAccountId is null)
            return Result.Failure(new[] { "No default revenue debit account configured for fund."});

        // Validate fiscal year open
        var fiscalYear = await context.FiscalYears
            .FirstOrDefaultAsync(x => x.StartDate <= entity.ReceiptDate && x.EndDate >= entity.ReceiptDate, cancellationToken);
        if (fiscalYear is null || fiscalYear.Status == ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.HardClosed)
            return Result.Failure(new[] { "No open fiscal year for receipt date."});

        // Validate fiscal period open
        var fiscalPeriod = await context.FiscalPeriods
            .FirstOrDefaultAsync(x => x.FiscalYearId == fiscalYear.Id
                && x.StartDate <= entity.ReceiptDate && x.EndDate >= entity.ReceiptDate
                && !x.IsLockedForPosting, cancellationToken);
        if (fiscalPeriod is null)
            return Result.Failure(new[] { "No open fiscal period for receipt date."});

        // Set status — GL posting delegated to PostingRules/AccountingEvent pipeline
        entity.Status = RevenueReceiptStatus.Posted;

        // Raise domain event — DomainEventHandler creates AccountingEvent, JournalEntryGenerator creates JournalEntry
        entity.AddDomainEvent(new RevenueReceiptPosted
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            PayerName = entity.PayerName,
            TotalAmount = entity.AmountTotal,
            CurrencyId = entity.CurrencyId
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Receipt was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }
}

public class PostRevenueReceiptCommandValidator : AbstractValidator<PostRevenueReceiptCommand>
{
    public PostRevenueReceiptCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid receipt ID.")
            .MustAsync(async (id, ct) =>
            {
                var receipt = await context.RevenueReceipts.FindAsync(id, ct);
                return receipt is not null && receipt.Status == RevenueReceiptStatus.Approved;
            })
            .WithMessage("Revenue receipt not found or not in approved status.");
    }
}
