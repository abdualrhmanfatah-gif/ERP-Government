using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.UpdateReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersUpdate)]
public class UpdateReceiptVoucherCommand : IRequest<Result<ReceiptVoucherDto>>
{
    public int Id { get; init; }
    public DateOnly VoucherDate { get; init; }
    public int PartyId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string ReceivedFrom { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<CreateReceiptVoucherLineDto> Lines { get; init; } = [];
    public List<CreateCheckDto> Checks { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<UpdateReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        UpdateReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<ReceiptVoucherDto>.Failure(["User identity is required."]);

        var voucher = await context.ReceiptVouchers
            .Include(v => v.CollectionOrder)
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher is null)
            return Result<ReceiptVoucherDto>.Failure(["Receipt voucher not found."]);

        if (voucher.Status != ReceiptVoucherStatus.Draft)
            return Result<ReceiptVoucherDto>.Failure(["Only Draft vouchers can be edited."]);

        if (request.Lines.Count == 0)
            return Result<ReceiptVoucherDto>.Failure(["At least one revenue line is required."]);

        if (request.Lines.Any(l => l.Amount <= 0))
            return Result<ReceiptVoucherDto>.Failure(["Each revenue line amount must be greater than zero."]);

        var party = await context.Parties.FindAsync(request.PartyId, cancellationToken);
        if (party is null || !party.IsActive)
            return Result<ReceiptVoucherDto>.Failure(["Party not found or inactive."]);

        if (request.PaymentMethod == PaymentMethod.Check && request.Checks.Count == 0)
            return Result<ReceiptVoucherDto>.Failure(["At least one check detail is required for check payments."]);

        if (request.PaymentMethod == PaymentMethod.Cash && request.Checks.Count > 0)
            return Result<ReceiptVoucherDto>.Failure(["Check details are not allowed for cash payments."]);

        voucher.VoucherDate = request.VoucherDate;
        voucher.PartyId = request.PartyId;
        voucher.PaymentMethod = request.PaymentMethod;
        voucher.ReceivedFrom = request.ReceivedFrom;
        voucher.Notes = request.Notes;
        voucher.RowVersion = request.RowVersion;
        voucher.LastModified = DateTimeOffset.UtcNow;
        voucher.LastModifiedBy = userId.ToString();

        var existingLines = voucher.Lines.ToList();
        context.ReceiptVoucherLines.RemoveRange(existingLines);

        foreach (var lineDto in request.Lines)
        {
            context.ReceiptVoucherLines.Add(new ERP_Government.Domain.Revenue.Entities.ReceiptVoucherLine
            {
                ReceiptVoucherId = voucher.Id,
                RevenueAccountId = lineDto.RevenueAccountId,
                Amount = lineDto.Amount,
                Description = lineDto.Description
            });
        }

        var existingChecks = voucher.Checks.ToList();
        context.Checks.RemoveRange(existingChecks);

        if (request.PaymentMethod == PaymentMethod.Check)
        {
            foreach (var checkDto in request.Checks)
            {
                context.Checks.Add(new ERP_Government.Domain.Revenue.Entities.Check
                {
                    ReceiptVoucherId = voucher.Id,
                    BankName = checkDto.BankName,
                    CheckNumber = checkDto.CheckNumber,
                    CheckDate = checkDto.CheckDate,
                    Amount = checkDto.Amount,
                    Status = CheckStatus.Received,
                    Created = DateTimeOffset.UtcNow,
                    CreatedBy = userId.ToString()
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var totalAmount = request.Lines.Sum(l => l.Amount);

        return Result<ReceiptVoucherDto>.Success(new ReceiptVoucherDto
        {
            Id = voucher.Id,
            CollectionOrderId = voucher.CollectionOrderId,
            CollectionOrderNumber = voucher.CollectionOrder.OrderNumber,
            VoucherNumber = voucher.VoucherNumber,
            VoucherDate = voucher.VoucherDate,
            PartyId = voucher.PartyId,
            PartyName = party.NameAr,
            PaymentMethod = voucher.PaymentMethod,
            PaymentMethodName = voucher.PaymentMethod.ToString(),
            ReceivedFrom = voucher.ReceivedFrom,
            Notes = voucher.Notes,
            Status = voucher.Status,
            StatusName = voucher.Status.ToString(),
            TotalAmount = totalAmount,
            RowVersion = voucher.RowVersion,
            Created = voucher.Created,
            CreatedBy = voucher.CreatedBy
        });
    }
}

public class UpdateReceiptVoucherCommandValidator : AbstractValidator<UpdateReceiptVoucherCommand>
{
    public UpdateReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Voucher ID is required.");
        RuleFor(x => x.VoucherDate).NotEmpty().WithMessage("Voucher date is required.");
        RuleFor(x => x.PartyId).GreaterThan(0).WithMessage("Party is required.");
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Invalid payment method.");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
