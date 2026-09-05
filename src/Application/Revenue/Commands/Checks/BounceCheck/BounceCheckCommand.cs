using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;

[Authorize(Policy = PermissionCodes.ChecksBounce)]
public class BounceCheckCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public DateTimeOffset BouncedAt { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class BounceCheckCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<BounceCheckCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        BounceCheckCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(new[] { "User identity is required for this operation." });

        var check = await context.Checks
            .Include(c => c.ReceiptVoucher)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (check is null)
            return Result<int>.Failure(new[] { "Check not found."});

        if (check.Status != CheckStatus.UnderCollection)
            return Result<int>.Failure(new[] { "Only Under-Collection checks can be bounced."});

        check.Status = CheckStatus.Bounced;
        check.BouncedAt = request.BouncedAt;
        check.RowVersion = request.RowVersion;

        var originalVoucher = check.ReceiptVoucher;
        if (originalVoucher is null)
            return Result<int>.Failure(new[] { "Original voucher not found."});

        originalVoucher.DepositSlipId = null;

        var newVoucher = new ReceiptVoucher
        {
            VoucherNumber = await GenerateVoucherNumber(cancellationToken),
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = originalVoucher.PartyId,
            PaymentMethod = originalVoucher.PaymentMethod,
            ReceivedFrom = originalVoucher.ReceivedFrom,
            Notes = $"Replacement for bounced check {check.CheckNumber}",
            Status = ReceiptVoucherStatus.Draft
        };

        context.ReceiptVouchers.Add(newVoucher);
        await context.SaveChangesAsync(cancellationToken);

        check.ReplacementVoucherId = newVoucher.Id;

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(Check),
            DocumentId = check.Id,
            FromStatus = CheckStatus.UnderCollection.ToString(),
            ToStatus = CheckStatus.Bounced.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(new[] { "Check was modified by another user. Please refresh and try again."});
        }

        return Result<int>.Success(newVoucher.Id);
    }

    private async Task<string> GenerateVoucherNumber(CancellationToken cancellationToken)
    {
        var lastNumber = await context.ReceiptVouchers
            .OrderByDescending(v => v.VoucherNumber)
            .Select(v => v.VoucherNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastNumber is null)
            return "RCV-000001";

        var lastSeq = int.Parse(lastNumber.Substring(4));
        return $"RCV-{(lastSeq + 1):D6}";
    }
}

public class BounceCheckCommandValidator : AbstractValidator<BounceCheckCommand>
{
    public BounceCheckCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Check ID is required.");

        RuleFor(x => x.BouncedAt)
            .NotEmpty().WithMessage("Bounce date is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
