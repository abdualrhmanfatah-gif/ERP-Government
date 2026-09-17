using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchers;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetReceiptVouchersQuery : IRequest<List<ReceiptVoucherDto>>
{
    public int? CollectionOrderId { get; init; }
    public int? PartyId { get; init; }
    public ReceiptVoucherStatus? Status { get; init; }
}

public class GetReceiptVouchersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReceiptVouchersQuery, List<ReceiptVoucherDto>>
{
    public async Task<List<ReceiptVoucherDto>> Handle(
        GetReceiptVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ReceiptVouchers
            .AsNoTracking()
            .Include(v => v.CollectionOrder)
            .Include(v => v.Party)
            .Include(v => v.DepositSlip47)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
                .ThenInclude(c => c.DepositSlip48)
            .AsQueryable();

        if (request.CollectionOrderId.HasValue)
            query = query.Where(v => v.CollectionOrderId == request.CollectionOrderId.Value);

        if (request.PartyId.HasValue)
            query = query.Where(v => v.PartyId == request.PartyId.Value);

        if (request.Status.HasValue)
            query = query.Where(v => v.Status == request.Status.Value);

        var vouchers = await query.OrderByDescending(v => v.Created).ToListAsync(cancellationToken);

        return vouchers.Select(v => new ReceiptVoucherDto
        {
            Id = v.Id,
            CollectionOrderId = v.CollectionOrderId,
            CollectionOrderNumber = v.CollectionOrder.OrderNumber,
            VoucherNumber = v.VoucherNumber,
            VoucherDate = v.VoucherDate,
            PartyId = v.PartyId,
            PartyName = v.Party?.NameAr ?? string.Empty,
            PaymentMethod = v.PaymentMethod,
            PaymentMethodName = v.PaymentMethod.ToString(),
            ReceivedFrom = v.ReceivedFrom,
            Notes = v.Notes,
            DepositSlip47Id = v.DepositSlip47Id,
            DepositSlip47Number = v.DepositSlip47?.SlipNumber,
            Status = v.Status,
            StatusName = v.Status.ToString(),
            TotalAmount = v.Lines.Sum(l => l.Amount),
            ApprovedById = v.ApprovedById,
            ApprovedAt = v.ApprovedAt,
            CancellationReason = v.CancellationReason,
            Lines = v.Lines.Select(l => new ReceiptVoucherLineDto
            {
                Id = l.Id,
                ReceiptVoucherId = l.ReceiptVoucherId,
                RevenueAccountId = l.RevenueAccountId,
                Amount = l.Amount,
                Description = l.Description
            }).ToList(),
            Checks = v.Checks.Select(c => new CheckDto
            {
                Id = c.Id,
                ReceiptVoucherId = c.ReceiptVoucherId,
                BankName = c.BankName,
                CheckNumber = c.CheckNumber,
                CheckDate = c.CheckDate,
                Amount = c.Amount,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                DepositSlip48Id = c.DepositSlip48Id,
                DepositSlip48Number = c.DepositSlip48?.SlipNumber,
                ClearedAt = c.ClearedAt,
                BouncedAt = c.BouncedAt,
                ReplacementVoucherId = c.ReplacementVoucherId,
                RowVersion = c.RowVersion,
                Created = c.Created
            }).ToList(),
            RowVersion = v.RowVersion,
            Created = v.Created,
            CreatedBy = v.CreatedBy
        }).ToList();
    }
}
