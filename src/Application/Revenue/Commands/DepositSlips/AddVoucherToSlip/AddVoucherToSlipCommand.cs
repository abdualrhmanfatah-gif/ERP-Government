using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.AddVoucherToSlip;

[Authorize(Policy = PermissionCodes.DepositSlipsUpdate)]
public class AddVoucherToSlipCommand : IRequest<Result>
{
    public int SlipId { get; init; }
    public int VoucherId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class AddVoucherToSlipCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AddVoucherToSlipCommand, Result>
{
    public async Task<Result> Handle(
        AddVoucherToSlipCommand request,
        CancellationToken cancellationToken)
    {
        var slip = await context.DepositSlips.FindAsync(request.SlipId, cancellationToken);
        if (slip is null)
            return Result.Failure(new[] { "Deposit slip not found." });

        if (slip.Status != DepositSlipStatus.Draft)
            return Result.Failure(new[] { "Only Draft slips can be modified." });

        var voucher = await context.ReceiptVouchers.FindAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
            return Result.Failure(new[] { "Receipt voucher not found." });

        if (voucher.Status != ReceiptVoucherStatus.Approved)
            return Result.Failure(new[] { "Voucher must be Approved." });

        if (voucher.DepositSlipId.HasValue)
            return Result.Failure(new[] { "Voucher is already part of a deposit slip." });

        if (slip.FormType == FormType.Form47 && voucher.PaymentMethod != PaymentMethod.Cash)
            return Result.Failure(new[] { "Voucher is not a cash voucher and cannot be added to a Form 47 (cash-only) slip." });

        if (slip.FormType == FormType.Form48 && voucher.PaymentMethod != PaymentMethod.Check)
            return Result.Failure(new[] { "Voucher is not a check voucher and cannot be added to a Form 48 (checks-only) slip." });

        voucher.DepositSlipId = slip.Id;

        var allMembers = slip.ReceiptVouchers.Append(voucher).ToList();
        slip.TotalAmount = allMembers.Sum(v => v.Lines.Sum(l => l.Amount));
        slip.RowVersion = request.RowVersion;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Deposit slip was modified by another user. Please refresh and try again." });
        }

        return Result.Success();
    }
}

public class AddVoucherToSlipCommandValidator : AbstractValidator<AddVoucherToSlipCommand>
{
    public AddVoucherToSlipCommandValidator()
    {
        RuleFor(x => x.SlipId)
            .GreaterThan(0).WithMessage("Slip ID is required.");

        RuleFor(x => x.VoucherId)
            .GreaterThan(0).WithMessage("Voucher ID is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
