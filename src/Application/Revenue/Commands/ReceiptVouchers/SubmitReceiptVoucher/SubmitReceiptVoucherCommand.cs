using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.SubmitReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersSubmit)]
public class SubmitReceiptVoucherCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<SubmitReceiptVoucherCommand, Result>
{
    public async Task<Result> Handle(
        SubmitReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var voucher = await context.ReceiptVouchers
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);
        if (voucher is null)
            return Result.Failure(new[] { "Receipt voucher not found."});

        if (voucher.Status != ReceiptVoucherStatus.Draft)
            return Result.Failure(new[] { "Only Draft vouchers can be submitted for review."});

        if (voucher.Lines.Count == 0)
            return Result.Failure(new[] { "Voucher has no revenue lines and cannot be submitted."});

        if (voucher.Checks.Sum(c => c.Amount) > voucher.Lines.Sum(l => l.Amount))
            return Result.Failure(new[] { "The sum of check amounts exceeds the voucher total." });

        voucher.Status = ReceiptVoucherStatus.PendingReview;
        voucher.SubmittedById = userId;
        voucher.SubmittedAt = DateTimeOffset.UtcNow;
        voucher.RowVersion = request.RowVersion;

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = ReceiptVoucherStatus.Draft.ToString(),
            ToStatus = ReceiptVoucherStatus.PendingReview.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Voucher was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }
}

public class SubmitReceiptVoucherCommandValidator : AbstractValidator<SubmitReceiptVoucherCommand>
{
    public SubmitReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Voucher ID is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
