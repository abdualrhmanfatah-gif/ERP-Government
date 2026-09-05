using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.RevenueReceipts.GetRevenueReceipts;

[Authorize(Policy = PermissionCodes.RevenueReceiptsView)]
public class GetRevenueReceiptsQuery : IRequest<List<RevenueReceiptDto>>
{
    public RevenueReceiptStatus? Status { get; init; }
    public RevenueReceiptType? ReceiptType { get; init; }
    public int? FundId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetRevenueReceiptsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetRevenueReceiptsQuery, List<RevenueReceiptDto>>
{
    public async Task<List<RevenueReceiptDto>> Handle(
        GetRevenueReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.RevenueReceipts.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.ReceiptType.HasValue)
            query = query.Where(x => x.ReceiptType == request.ReceiptType.Value);
        if (request.FundId.HasValue)
            query = query.Where(x => x.FundId == request.FundId.Value);
        if (request.FromDate.HasValue)
            query = query.Where(x => x.ReceiptDate >= DateOnly.FromDateTime(request.FromDate.Value));
        if (request.ToDate.HasValue)
            query = query.Where(x => x.ReceiptDate <= DateOnly.FromDateTime(request.ToDate.Value));

        var receipts = await query
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Created)
            .Select(x => new RevenueReceiptDto
            {
                Id = x.Id,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptType = x.ReceiptType,
                ReceiptDate = x.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                PayerName = x.PayerName,
                PayerNationalId = x.PayerNationalId,
                FundId = x.FundId,
                BudgetClassificationId = x.BudgetClassificationId,
                CurrencyId = x.CurrencyId,
                AmountTotal = x.AmountTotal,
                PaymentMethod = x.PaymentMethod,
                ExternalTransactionRef = x.ExternalTransactionRef,
                JournalEntryId = x.JournalEntryId,
                Status = x.Status,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        var receiptIds = receipts.Select(r => r.Id).ToList();
        var linesByReceipt = await context.RevenueReceiptLines
            .Where(l => receiptIds.Contains(l.ReceiptId))
            .GroupBy(l => l.ReceiptId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(l => new RevenueReceiptLineDto
                {
                    Id = l.Id,
                    ReceiptId = l.ReceiptId,
                    AccountId = l.AccountId,
                    Description = l.Description,
                    Amount = l.Amount
                }).ToList(),
                cancellationToken);

        foreach (var receipt in receipts)
        {
            receipt.Lines = linesByReceipt.TryGetValue(receipt.Id, out var lines) ? lines : [];
        }

        return receipts;
    }
}
