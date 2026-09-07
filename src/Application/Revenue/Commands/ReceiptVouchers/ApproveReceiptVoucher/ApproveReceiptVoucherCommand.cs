using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersApprove)]
public class ApproveReceiptVoucherCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveReceiptVoucherCommand, Result>
{
    public async Task<Result> Handle(
        ApproveReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var voucher = await context.ReceiptVouchers.FindAsync(request.Id, cancellationToken);
        if (voucher is null)
            return Result.Failure(new[] { "Receipt voucher not found."});

        if (voucher.Status != ReceiptVoucherStatus.PendingReview)
            return Result.Failure(new[] { "Only Pending Review vouchers can be approved."});

        // No separation-of-duties restriction: submitter may approve their own voucher
        // (TRE-01 clarification session 2026-09-07). Decision still recorded in history.

        voucher.Status = ReceiptVoucherStatus.Approved;
        voucher.ReviewedById = userId;
        voucher.ReviewedAt = DateTimeOffset.UtcNow;
        voucher.RowVersion = request.RowVersion;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            ApprovalStep = 1,
            Action = ApprovalAction.Approve,
            ApproverUserId = userId,
            RequiredRole = "AccountsReviewer",
            Decision = "Approved",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = ReceiptVoucherStatus.PendingReview.ToString(),
            ToStatus = ReceiptVoucherStatus.Approved.ToString(),
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
            return Result.Failure(new[] { "Voucher was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }
}

public class ApproveReceiptVoucherCommandValidator : AbstractValidator<ApproveReceiptVoucherCommand>
{
    public ApproveReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Voucher ID is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
