using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchersByPeriod;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetReceiptVouchersByPeriodQuery : IRequest<Result<List<ReceiptVoucherDto>>>
{
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetReceiptVouchersByPeriodQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetReceiptVouchersByPeriodQuery, Result<List<ReceiptVoucherDto>>>
{
    public async Task<Result<List<ReceiptVoucherDto>>> Handle(
        GetReceiptVouchersByPeriodQuery request,
        CancellationToken cancellationToken)
    {
        var vouchers = await context.ReceiptVouchers
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .Where(v => v.VoucherDate >= request.FromDate && v.VoucherDate <= request.ToDate)
            .OrderByDescending(v => v.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new ReceiptVoucherDto
            {
                Id = v.Id,
                VoucherNumber = v.VoucherNumber,
                VoucherDate = v.VoucherDate,
                PartyId = v.PartyId,
                PartyName = v.Party!.NameAr,
                PaymentMethod = v.PaymentMethod,
                PaymentMethodName = v.PaymentMethod.ToString(),
                ReceivedFrom = v.ReceivedFrom,
                Notes = v.Notes,
                DepositSlipId = v.DepositSlipId,
                Status = v.Status,
                StatusName = v.Status.ToString(),
                TotalAmount = v.Lines.Sum(l => l.Amount),
                Created = v.Created,
                CreatedBy = v.CreatedBy,
                LastModified = v.LastModified,
                LastModifiedBy = v.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return Result<List<ReceiptVoucherDto>>.Success(vouchers);
    }
}
