using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.RemoveVoucherFromSlip;

[Authorize(Policy = PermissionCodes.DepositSlipsUpdate)]
public class RemoveVoucherFromSlipCommand : IRequest<Result>
{
    public int SlipId { get; init; }
    public int VoucherId { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class RemoveVoucherFromSlipCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveVoucherFromSlipCommand, Result>
{
    public async Task<Result> Handle(
        RemoveVoucherFromSlipCommand request,
        CancellationToken cancellationToken)
    {
        var slip = await context.DepositSlips.FindAsync(request.SlipId, cancellationToken);
        if (slip is null)
            return Result.Failure(new[] { "Deposit slip not found." });

        if (slip.Status != DepositSlipStatus.Draft)
            return Result.Failure(new[] { "Only Draft slips can be modified." });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(new[] { "Removal reason is required." });

        var voucher = await context.ReceiptVouchers.FindAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
            return Result.Failure(new[] { "Receipt voucher not found." });

        if (voucher.DepositSlipId != slip.Id)
            return Result.Failure(new[] { "Voucher is not part of this deposit slip." });

        voucher.DepositSlipId = null;

        var remainingMembers = slip.ReceiptVouchers.Where(v => v.Id != voucher.Id).ToList();
        slip.TotalAmount = remainingMembers.Sum(v => v.Lines.Sum(l => l.Amount));
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

public class RemoveVoucherFromSlipCommandValidator : AbstractValidator<RemoveVoucherFromSlipCommand>
{
    public RemoveVoucherFromSlipCommandValidator()
    {
        RuleFor(x => x.SlipId)
            .GreaterThan(0).WithMessage("Slip ID is required.");

        RuleFor(x => x.VoucherId)
            .GreaterThan(0).WithMessage("Voucher ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Removal reason is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
