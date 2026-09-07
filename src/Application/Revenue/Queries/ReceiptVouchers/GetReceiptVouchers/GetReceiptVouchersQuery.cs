using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchers;

[Authorize(Policy = PermissionCodes.ReceiptVouchersView)]
public class GetReceiptVouchersQuery : IRequest<Result<List<ReceiptVoucherDto>>>
{
    public int? PartyId { get; init; }
    public PaymentMethod? PaymentMethodFilter { get; init; }
    public ReceiptVoucherStatus? Status { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetReceiptVouchersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetReceiptVouchersQuery, Result<List<ReceiptVoucherDto>>>
{
    public async Task<Result<List<ReceiptVoucherDto>>> Handle(
        GetReceiptVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ReceiptVouchers
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .Include(v => v.DepositSlip)
            .AsQueryable();

        if (request.PartyId.HasValue)
            query = query.Where(v => v.PartyId == request.PartyId.Value);

        if (request.PaymentMethodFilter.HasValue)
            query = query.Where(v => v.PaymentMethod == request.PaymentMethodFilter.Value);

        if (request.Status.HasValue)
            query = query.Where(v => v.Status == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(v => v.VoucherDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(v => v.VoucherDate <= request.ToDate.Value);

        var vouchers = await query
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
                DepositSlipNumber = v.DepositSlip != null ? v.DepositSlip.SlipNumber : null,
                Status = v.Status,
                StatusName = v.Status.ToString(),
                TotalAmount = v.Lines.Sum(l => l.Amount),
                RowVersion = v.RowVersion,
                Created = v.Created,
                CreatedBy = v.CreatedBy,
                LastModified = v.LastModified,
                LastModifiedBy = v.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return Result<List<ReceiptVoucherDto>>.Success(vouchers);
    }
}
