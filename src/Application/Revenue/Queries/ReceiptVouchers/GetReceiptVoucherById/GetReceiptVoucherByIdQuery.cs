using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetReceiptVoucherByIdQuery : IRequest<Result<ReceiptVoucherDto>>
{
    public int Id { get; init; }
}

public class GetReceiptVoucherByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetReceiptVoucherByIdQuery, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        GetReceiptVoucherByIdQuery request,
        CancellationToken cancellationToken)
    {
        var voucher = await context.ReceiptVouchers
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher is null)
            return Result<ReceiptVoucherDto>.Failure(new[] { "Receipt voucher not found."});

        var dto = new ReceiptVoucherDto
        {
            Id = voucher.Id,
            VoucherNumber = voucher.VoucherNumber,
            VoucherDate = voucher.VoucherDate,
            PartyId = voucher.PartyId,
            PartyName = voucher.Party?.NameAr ?? string.Empty,
            PaymentMethod = voucher.PaymentMethod,
            PaymentMethodName = voucher.PaymentMethod.ToString(),
            ReceivedFrom = voucher.ReceivedFrom,
            Notes = voucher.Notes,
            DepositSlipId = voucher.DepositSlipId,
            Status = voucher.Status,
            StatusName = voucher.Status.ToString(),
            TotalAmount = voucher.Lines.Sum(l => l.Amount),
            SubmittedById = voucher.SubmittedById,
            SubmittedAt = voucher.SubmittedAt,
            ReviewedById = voucher.ReviewedById,
            ReviewedAt = voucher.ReviewedAt,
            CancellationReason = voucher.CancellationReason,
            RowVersion = voucher.RowVersion,
            Created = voucher.Created,
            CreatedBy = voucher.CreatedBy,
            LastModified = voucher.LastModified,
            LastModifiedBy = voucher.LastModifiedBy,
            Lines = voucher.Lines.Select(l => new ReceiptVoucherLineDto
            {
                Id = l.Id,
                ReceiptVoucherId = l.ReceiptVoucherId,
                RevenueAccountId = l.RevenueAccountId,
                Amount = l.Amount,
                Description = l.Description
            }).ToList(),
            Checks = voucher.Checks.Select(c => new CheckDto
            {
                Id = c.Id,
                ReceiptVoucherId = c.ReceiptVoucherId,
                BankName = c.BankName,
                CheckNumber = c.CheckNumber,
                CheckDate = c.CheckDate,
                Amount = c.Amount,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                ClearedAt = c.ClearedAt,
                BouncedAt = c.BouncedAt,
                ReplacementVoucherId = c.ReplacementVoucherId
            }).ToList()
        };

        return Result<ReceiptVoucherDto>.Success(dto);
    }
}
