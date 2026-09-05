using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip;

[Authorize(Policy = PermissionCodes.DepositSlipsApprove)]
public class ApproveDepositSlipCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveDepositSlipCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveDepositSlipCommand, Result>
{
    public async Task<Result> Handle(
        ApproveDepositSlipCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var slip = await context.DepositSlips
            .Include(s => s.ReceiptVouchers)
                .ThenInclude(v => v.Checks)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slip is null)
            return Result.Failure(new[] { "Deposit slip not found." });

        if (slip.Status != DepositSlipStatus.Draft)
            return Result.Failure(new[] { "Only Draft slips can be approved." });

        slip.Status = DepositSlipStatus.Approved;
        slip.ApprovedById = userId;
        slip.ApprovedAt = DateTimeOffset.UtcNow;
        slip.RowVersion = request.RowVersion;

        if (slip.FormType == FormType.Form47)
        {
            foreach (var voucher in slip.ReceiptVouchers)
            {
                voucher.AddDomainEvent(new Domain.Events.Revenue.ReceiptVoucherCollected
                {
                    SourceEntityId = voucher.Id,
                    OccurredAt = DateTimeOffset.UtcNow,
                    PartyName = voucher.ReceivedFrom,
                    TotalAmount = voucher.Lines.Sum(l => l.Amount),
                    CurrencyId = 1,
                    FormType = slip.FormType
                });
            }
        }
        else if (slip.FormType == FormType.Form48)
        {
            foreach (var voucher in slip.ReceiptVouchers)
            {
                foreach (var check in voucher.Checks.Where(c => c.Status == CheckStatus.UnderCollection))
                {
                    check.Status = CheckStatus.UnderCollection;
                }
            }
        }

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = nameof(DepositSlip),
            DocumentId = slip.Id,
            ApprovalStep = 1,
            Action = ApprovalAction.Approve,
            ApproverUserId = userId,
            RequiredRole = "TreasuryManager",
            Decision = "Approved",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip),
            DocumentId = slip.Id,
            FromStatus = DepositSlipStatus.Draft.ToString(),
            ToStatus = DepositSlipStatus.Approved.ToString(),
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
            return Result.Failure(new[] { "Deposit slip was modified by another user. Please refresh and try again." });
        }

        return Result.Success();
    }
}

public class ApproveDepositSlipCommandValidator : AbstractValidator<ApproveDepositSlipCommand>
{
    public ApproveDepositSlipCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Slip ID is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
