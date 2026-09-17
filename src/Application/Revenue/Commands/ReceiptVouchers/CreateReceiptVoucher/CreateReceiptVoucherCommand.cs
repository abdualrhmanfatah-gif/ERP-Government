using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersCreate)]
public class CreateReceiptVoucherCommand : IRequest<Result<ReceiptVoucherDto>>
{
    public int CollectionOrderId { get; init; }
    public DateOnly VoucherDate { get; init; }
    public int PartyId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string ReceivedFrom { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<CreateReceiptVoucherLineDto> Lines { get; init; } = [];
    public List<CreateCheckDto> Checks { get; init; } = [];
}

public class CreateReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        CreateReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<ReceiptVoucherDto>.Failure(["User identity is required."]);

        if (request.Lines.Count == 0)
            return Result<ReceiptVoucherDto>.Failure(["At least one revenue line is required."]);

        if (request.Lines.Any(l => l.Amount <= 0))
            return Result<ReceiptVoucherDto>.Failure(["Each revenue line amount must be greater than zero."]);

        var order = await context.CollectionOrders
            .Include(o => o.ReceiptVouchers)
                .ThenInclude(v => v.Lines)
            .Include(o => o.ReceiptVouchers)
                .ThenInclude(v => v.Checks)
            .Include(o => o.RevenueClaim)
            .FirstOrDefaultAsync(o => o.Id == request.CollectionOrderId, cancellationToken);

        if (order is null)
            return Result<ReceiptVoucherDto>.Failure(["Collection order not found."]);

        if (order.Status != CollectionOrderStatus.Approved && order.Status != CollectionOrderStatus.PartiallyCollected)
            return Result<ReceiptVoucherDto>.Failure(["Receipt vouchers can only be created for Approved or PartiallyCollected collection orders."]);

        var party = await context.Parties.FindAsync(request.PartyId, cancellationToken);
        if (party is null || !party.IsActive)
            return Result<ReceiptVoucherDto>.Failure(["Party not found or inactive."]);

        var totalAmount = request.Lines.Sum(l => l.Amount);

        var (_, _, _, availableOnOrder) = RevenueMetricsCalculator.CalculateOrderMetrics(order.AuthorizedAmount, order.ReceiptVouchers);

        if (totalAmount > availableOnOrder)
            return Result<ReceiptVoucherDto>.Failure([$"Voucher total ({totalAmount:N2}) exceeds available amount on collection order ({availableOnOrder:N2})."]);

        if (request.PaymentMethod == PaymentMethod.Check)
        {
            if (request.Checks.Count == 0)
                return Result<ReceiptVoucherDto>.Failure(["At least one check detail is required for check payments."]);

            var checksTotal = request.Checks.Sum(c => c.Amount);
            if (checksTotal != totalAmount)
                return Result<ReceiptVoucherDto>.Failure([$"Check amounts total ({checksTotal:N2}) must equal voucher total ({totalAmount:N2})."]);
        }
        else if (request.PaymentMethod == PaymentMethod.Cash && request.Checks.Count > 0)
        {
            return Result<ReceiptVoucherDto>.Failure(["Check details are not allowed for cash payments."]);
        }

        var voucherNumber = await sequenceService.GenerateNextNumberAsync("ReceiptVoucher", cancellationToken);

        var voucher = new ReceiptVoucher
        {
            CollectionOrderId = request.CollectionOrderId,
            VoucherNumber = voucherNumber,
            VoucherDate = request.VoucherDate,
            PartyId = request.PartyId,
            PaymentMethod = request.PaymentMethod,
            ReceivedFrom = request.ReceivedFrom,
            Notes = request.Notes,
            Status = ReceiptVoucherStatus.Draft,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        context.ReceiptVouchers.Add(voucher);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var lineDto in request.Lines)
        {
            context.ReceiptVoucherLines.Add(new ReceiptVoucherLine
            {
                ReceiptVoucherId = voucher.Id,
                RevenueAccountId = lineDto.RevenueAccountId,
                Amount = lineDto.Amount,
                Description = lineDto.Description
            });
        }

        if (request.PaymentMethod == PaymentMethod.Check)
        {
            foreach (var checkDto in request.Checks)
            {
                context.Checks.Add(new Check
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

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = ReceiptVoucherStatus.Draft.ToString(),
            ToStatus = ReceiptVoucherStatus.Draft.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<ReceiptVoucherDto>.Success(new ReceiptVoucherDto
        {
            Id = voucher.Id,
            CollectionOrderId = voucher.CollectionOrderId,
            CollectionOrderNumber = order.OrderNumber,
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

public class CreateReceiptVoucherCommandValidator : AbstractValidator<CreateReceiptVoucherCommand>
{
    public CreateReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.CollectionOrderId).GreaterThan(0).WithMessage("Collection order is required.");
        RuleFor(x => x.VoucherDate).NotEmpty().WithMessage("Voucher date is required.");
        RuleFor(x => x.PartyId).GreaterThan(0).WithMessage("Party is required.");
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Invalid payment method.");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required.");
    }
}
