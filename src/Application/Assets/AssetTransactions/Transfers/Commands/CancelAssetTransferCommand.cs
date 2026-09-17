using ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;

[Authorize(Policy = PermissionCodes.AssetTransfersCreate)]
public record CancelAssetTransferCommand(int Id, byte[] RowVersion) : IRequest<Result<int>>;

public class CancelAssetTransferCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelAssetTransferCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CancelAssetTransferCommand request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);

        if (transaction is null || transaction.TransactionType != AssetTransactionType.Transfer)
            return Result<int>.Failure(ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "معاملة النقل غير موجودة");

        if (transaction.Status != AssetTransactionStatus.Draft)
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                "لا يمكن إلغاء نقل ليس في حالة مسودة");

        if (!AssetTransferRules.RowVersionMatches(transaction.RowVersion, request.RowVersion))
            return AssetTransferRules.ConcurrencyConflict<int>();

        transaction.Status = AssetTransactionStatus.Cancelled;
        transaction.LastModified = DateTimeOffset.UtcNow;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return AssetTransferRules.ConcurrencyConflict<int>();
        }

        return Result<int>.Success(transaction.Id);
    }
}

public class CancelAssetTransferCommandValidator : AbstractValidator<CancelAssetTransferCommand>
{
    public CancelAssetTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف النقل مطلوب");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار الصف مطلوب للتحقق من التزامن");
    }
}
