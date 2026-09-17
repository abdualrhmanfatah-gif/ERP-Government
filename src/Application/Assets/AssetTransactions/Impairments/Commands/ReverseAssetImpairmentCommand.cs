using ERP_Government.Application.Assets.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Impairments.Commands;

[Authorize(Policy = PermissionCodes.AssetImpairmentsPost)]
public record ReverseAssetImpairmentCommand(int Id) : IRequest<Result<int>>;

public class ReverseAssetImpairmentCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<ReverseAssetImpairmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ReverseAssetImpairmentCommand request, CancellationToken ct)
    {
        var originalTransaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (originalTransaction is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة انخفاض القيمة غير موجودة");

        if (originalTransaction.TransactionType != AssetTransactionType.Impairment)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "هذه المعاملة ليست معاملة انخفاض قيمة");

        if (originalTransaction.Status != AssetTransactionStatus.Posted)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن عكس انخفاض القيمة إلا من حالة منشورة");

        var alreadyReversed = await context.AssetImpairmentDetails
            .AnyAsync(d => d.ReversalOfTransactionId == request.Id, ct);
        if (alreadyReversed)
            return Result<int>.Failure(ErrorCodes.Assets.ReversalAlreadyExists, ErrorCategory.Validation, "يوجد عكس مسجل لهذه المعاملة مسبقاً");

        var asset = await context.Assets.FindAsync([originalTransaction.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        var originalDetail = await context.AssetImpairmentDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);
        if (originalDetail is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "تفاصيل انخفاض القيمة غير موجودة");

        var reversalDate = DateOnly.FromDateTime(DateTime.Today);
        var periodResult = await AssetPostingGuards.ResolveOpenPeriodAsync(context, reversalDate, ct);
        if (!periodResult.Succeeded)
            return Result<int>.Failure(periodResult.Code!, periodResult.Category!.Value, periodResult.Message!);

        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetImpairment", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل في توليد رقم عكس الانخفاض");
        }

        var reversalTransaction = new AssetTransaction
        {
            TransactionNumber = number,
            AssetId = originalTransaction.AssetId,
            TransactionType = AssetTransactionType.Impairment,
            TransactionDate = reversalDate,
            Status = AssetTransactionStatus.Posting,
            CurrencyId = originalTransaction.CurrencyId,
            Notes = $"عكس انخفاض القيمة — المعاملة الأصلية #{originalTransaction.TransactionNumber}",
            JournalEntryId = null,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetTransactions.Add(reversalTransaction);
        await context.SaveChangesAsync(ct);

        var reversalDetail = new AssetImpairmentDetail
        {
            AssetTransactionId = reversalTransaction.Id,
            CarryingAmount = asset.CurrentValue ?? asset.OriginalValue,
            RecoverableAmount = (asset.CurrentValue ?? asset.OriginalValue) + originalDetail.LossAmount,
            LossAmount = -originalDetail.LossAmount,
            Reason = $"عكس — {originalDetail.Reason}",
            ReversalOfTransactionId = request.Id
        };

        context.AssetImpairmentDetails.Add(reversalDetail);

        originalTransaction.Status = AssetTransactionStatus.Reversed;
        originalTransaction.LastModified = DateTimeOffset.UtcNow;

        reversalTransaction.AddDomainEvent(new AssetImpairmentReversed
        {
            SourceEntityId = reversalTransaction.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            AssetId = reversalTransaction.AssetId,
            ReversalAmount = originalDetail.LossAmount,
            ReversalOfTransactionId = request.Id,
            PeriodId = periodResult.Value!.Id
        });

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(reversalTransaction.Id);
    }
}
