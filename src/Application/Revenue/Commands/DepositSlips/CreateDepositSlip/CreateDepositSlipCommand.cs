using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip;

[Authorize(Policy = PermissionCodes.DepositSlipsCreate)]
public class CreateDepositSlipCommand : IRequest<Result<int>>
{
    public DateOnly SlipDate { get; init; }
    public FormType FormType { get; init; }
    public List<int> VoucherIds { get; init; } = [];
}

public class CreateDepositSlipCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateDepositSlipCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateDepositSlipCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(new[] { "User identity is required for this operation." });

        if (request.SlipDate > DateOnly.FromDateTime(DateTime.Today))
            return Result<int>.Failure(new[] { "Slip date cannot be in the future." });

        var vouchers = await context.ReceiptVouchers
            .Where(v => request.VoucherIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        if (vouchers.Count != request.VoucherIds.Count)
            return Result<int>.Failure(new[] { "One or more vouchers not found." });

        if (vouchers.Any(v => v.Status != ReceiptVoucherStatus.Approved))
            return Result<int>.Failure(new[] { "All vouchers must be Approved." });

        if (vouchers.Any(v => v.DepositSlipId.HasValue))
            return Result<int>.Failure(new[] { "One or more vouchers are already part of a deposit slip." });

        if (vouchers.Count > 0)
        {
            var latestVoucherDate = vouchers.Max(v => v.VoucherDate);
            if (request.SlipDate < latestVoucherDate)
                return Result<int>.Failure(new[] { "Slip date must be on or after the latest voucher date." });
        }

        foreach (var voucher in vouchers)
        {
            if (request.FormType == FormType.Form47 && voucher.PaymentMethod != PaymentMethod.Cash)
                return Result<int>.Failure(new[] { $"Voucher {voucher.VoucherNumber} is not a cash voucher and cannot be added to a Form 47 (cash-only) slip." });

            if (request.FormType == FormType.Form48 && voucher.PaymentMethod != PaymentMethod.Check)
                return Result<int>.Failure(new[] { $"Voucher {voucher.VoucherNumber} is not a check voucher and cannot be added to a Form 48 (checks-only) slip." });
        }

        var slipNumber = await sequenceService.GenerateNextNumberAsync("DepositSlip", cancellationToken);

        var slip = new DepositSlip
        {
            SlipNumber = slipNumber,
            SlipDate = request.SlipDate,
            FormType = request.FormType,
            Status = DepositSlipStatus.Draft,
            TotalAmount = vouchers.Sum(v => v.Lines.Sum(l => l.Amount))
        };

        context.DepositSlips.Add(slip);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var voucher in vouchers)
        {
            voucher.DepositSlipId = slip.Id;
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip),
            DocumentId = slip.Id,
            FromStatus = null!,
            ToStatus = DepositSlipStatus.Draft.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(new[] { "Deposit slip was modified by another user. Please refresh and try again." });
        }

        return Result<int>.Success(slip.Id);
    }
}

public class CreateDepositSlipCommandValidator : AbstractValidator<CreateDepositSlipCommand>
{
    public CreateDepositSlipCommandValidator()
    {
        RuleFor(x => x.SlipDate)
            .NotEmpty().WithMessage("Slip date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Slip date cannot be in the future.");

        RuleFor(x => x.FormType)
            .IsInEnum().WithMessage("Invalid form type.");
    }
}
