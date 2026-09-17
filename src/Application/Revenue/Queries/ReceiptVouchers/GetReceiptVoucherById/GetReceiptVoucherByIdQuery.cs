using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetReceiptVoucherByIdQuery : IRequest<Result<ReceiptVoucherDto>>
{
    public int Id { get; init; }
}

public class GetReceiptVoucherByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReceiptVoucherByIdQuery, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        GetReceiptVoucherByIdQuery request,
        CancellationToken cancellationToken)
    {
        var v = await context.ReceiptVouchers
            .AsNoTracking()
            .Include(v => v.CollectionOrder)
            .Include(v => v.Party)
            .Include(v => v.DepositSlip47)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
                .ThenInclude(c => c.DepositSlip48)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (v is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.Revenue.ReceiptNotFound, ErrorCategory.NotFound, $"Receipt voucher with ID {request.Id} not found.");

        return Result<ReceiptVoucherDto>.Success(new ReceiptVoucherDto
        {
            Id = v.Id,
            CollectionOrderId = v.CollectionOrderId,
            CollectionOrderNumber = v.CollectionOrder?.OrderNumber ?? string.Empty,
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
        });
    }
}
