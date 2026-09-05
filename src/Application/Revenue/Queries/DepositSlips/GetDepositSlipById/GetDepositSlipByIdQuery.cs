using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.DepositSlips.GetDepositSlipById;

[Authorize(Policy = PermissionCodes.DepositSlipsView)]
public class GetDepositSlipByIdQuery : IRequest<Result<DepositSlipDto>>
{
    public int Id { get; init; }
}

public class GetDepositSlipByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDepositSlipByIdQuery, Result<DepositSlipDto>>
{
    public async Task<Result<DepositSlipDto>> Handle(
        GetDepositSlipByIdQuery request,
        CancellationToken cancellationToken)
    {
        var slip = await context.DepositSlips
            .Include(s => s.ReceiptVouchers)
                .ThenInclude(v => v.Lines)
            .Include(s => s.ReceiptVouchers)
                .ThenInclude(v => v.Party)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slip is null)
            return Result<DepositSlipDto>.Failure(new[] { "Deposit slip not found."});

        var dto = new DepositSlipDto
        {
            Id = slip.Id,
            SlipNumber = slip.SlipNumber,
            SlipDate = slip.SlipDate,
            FormType = slip.FormType,
            Status = slip.Status,
            StatusName = slip.Status.ToString(),
            TotalAmount = slip.TotalAmount,
            RowVersion = slip.RowVersion,
            ApprovedById = slip.ApprovedById,
            ApprovedAt = slip.ApprovedAt,
            ReceiptVouchers = slip.ReceiptVouchers.Select(v => new ReceiptVoucherDto
            {
                Id = v.Id,
                VoucherNumber = v.VoucherNumber,
                VoucherDate = v.VoucherDate,
                PartyId = v.PartyId,
                PartyName = v.Party?.NameAr ?? string.Empty,
                PaymentMethod = v.PaymentMethod,
                PaymentMethodName = v.PaymentMethod.ToString(),
                ReceivedFrom = v.ReceivedFrom,
                Status = v.Status,
                StatusName = v.Status.ToString(),
                TotalAmount = v.Lines.Sum(l => l.Amount)
            }).ToList()
        };

        return Result<DepositSlipDto>.Success(dto);
    }
}
