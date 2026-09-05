using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.RevenueReceipts.GetRevenueReceiptById;

[Authorize(Policy = PermissionCodes.RevenueReceiptsView)]
public class GetRevenueReceiptByIdQuery : IRequest<RevenueReceiptDto?>
{
    public int Id { get; init; }
}

public class GetRevenueReceiptByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetRevenueReceiptByIdQuery, RevenueReceiptDto?>
{
    public async Task<RevenueReceiptDto?> Handle(
        GetRevenueReceiptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.RevenueReceipts
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is not null)
        {
            dto.Lines = await context.RevenueReceiptLines
                .Where(l => l.ReceiptId == request.Id)
                .Select(l => new RevenueReceiptLineDto
                {
                    Id = l.Id,
                    ReceiptId = l.ReceiptId,
                    AccountId = l.AccountId,
                    Description = l.Description,
                    Amount = l.Amount
                })
                .ToListAsync(cancellationToken);
        }

        return dto;
    }
}
