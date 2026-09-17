using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip47;

[Authorize(Policy = PermissionCodes.DepositSlipsCreate)]
public class CreateDepositSlip47Command : IRequest<Result<DepositSlip47Dto>>
{
    public DateOnly SlipDate { get; init; }
    public List<int> ReceiptVoucherIds { get; init; } = [];
}

public class CreateDepositSlip47CommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateDepositSlip47Command, Result<DepositSlip47Dto>>
{
    public async Task<Result<DepositSlip47Dto>> Handle(
        CreateDepositSlip47Command request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<DepositSlip47Dto>.Failure(["User identity is required."]);

        if (request.ReceiptVoucherIds.Count == 0)
            return Result<DepositSlip47Dto>.Failure(["At least one approved cash receipt voucher is required."]);

        var vouchers = await context.ReceiptVouchers
            .Include(v => v.Lines)
            .Where(v => request.ReceiptVoucherIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        if (vouchers.Count != request.ReceiptVoucherIds.Count)
            return Result<DepositSlip47Dto>.Failure(["One or more receipt vouchers were not found."]);

        if (vouchers.Any(v => v.Status != ReceiptVoucherStatus.Approved))
            return Result<DepositSlip47Dto>.Failure(["All vouchers in DepositSlip47 must be Approved."]);

        if (vouchers.Any(v => v.PaymentMethod != PaymentMethod.Cash))
            return Result<DepositSlip47Dto>.Failure(["DepositSlip47 only accepts Cash payment vouchers."]);

        if (vouchers.Any(v => v.DepositSlip47Id.HasValue))
            return Result<DepositSlip47Dto>.Failure(["One or more vouchers are already assigned to a deposit slip."]);

        var totalAmount = vouchers.Sum(v => v.Lines.Sum(l => l.Amount));
        var slipNumber = await sequenceService.GenerateNextNumberAsync("DepositSlip47", cancellationToken);

        var slip = new DepositSlip47
        {
            SlipNumber = slipNumber,
            SlipDate = request.SlipDate,
            TotalAmount = totalAmount,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        context.DepositSlips47.Add(slip);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var v in vouchers)
        {
            v.DepositSlip47Id = slip.Id;
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip47),
            DocumentId = slip.Id,
            FromStatus = "Draft",
            ToStatus = "Draft",
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<DepositSlip47Dto>.Success(new DepositSlip47Dto
        {
            Id = slip.Id,
            SlipNumber = slip.SlipNumber,
            SlipDate = slip.SlipDate,
            TotalAmount = slip.TotalAmount,
            RowVersion = slip.RowVersion,
            Created = slip.Created,
            CreatedBy = slip.CreatedBy
        });
    }
}

public class CreateDepositSlip47CommandValidator : AbstractValidator<CreateDepositSlip47Command>
{
    public CreateDepositSlip47CommandValidator()
    {
        RuleFor(x => x.SlipDate).NotEmpty().WithMessage("Slip date is required.");
        RuleFor(x => x.ReceiptVoucherIds).NotEmpty().WithMessage("At least one voucher ID is required.");
    }
}
