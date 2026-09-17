using ERP_Government.Application.Assets.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Disposals.Commands;

public record AssetDisposalResponse(
    int Id, string TransactionNumber, int AssetId, DateOnly TransactionDate,
    string Status, decimal? NetBookValue, decimal? SalePrice, decimal? GainOrLoss);

[Authorize(Policy = PermissionCodes.AssetDisposalsView)]
public record GetAssetDisposalsQuery : IRequest<List<AssetDisposalResponse>>;

public class GetAssetDisposalsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetDisposalsQuery, List<AssetDisposalResponse>>
{
    public async Task<List<AssetDisposalResponse>> Handle(GetAssetDisposalsQuery request, CancellationToken ct)
    {
        return await context.AssetTransactions
            .Where(t => t.TransactionType == AssetTransactionType.Disposal)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new AssetDisposalResponse(
                t.Id, t.TransactionNumber, t.AssetId, t.TransactionDate,
                t.Status.ToString(), null, null, null))
            .ToListAsync(ct);
    }
}

[Authorize(Policy = PermissionCodes.AssetDisposalsView)]
public record GetAssetDisposalByIdQuery(int Id) : IRequest<Result<AssetDisposalResponse>>;

public class GetAssetDisposalByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetDisposalByIdQuery, Result<AssetDisposalResponse>>
{
    public async Task<Result<AssetDisposalResponse>> Handle(GetAssetDisposalByIdQuery request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (transaction is null)
            return Result<AssetDisposalResponse>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة التخلص غير موجودة");

        var detail = await context.AssetDisposalDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);

        return Result<AssetDisposalResponse>.Success(new AssetDisposalResponse(
            transaction.Id, transaction.TransactionNumber, transaction.AssetId, transaction.TransactionDate,
            transaction.Status.ToString(), detail?.BookValueAtDisposal, detail?.SaleProceeds, detail?.GainOrLoss));
    }
}

[Authorize(Policy = PermissionCodes.AssetDisposalsApprove)]
public record ApproveAssetDisposalCommand(int Id) : IRequest<Result<int>>;

public class ApproveAssetDisposalCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ApproveAssetDisposalCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ApproveAssetDisposalCommand request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (transaction is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة التخلص غير موجودة");

        if (transaction.TransactionType != AssetTransactionType.Disposal)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "هذه المعاملة ليست معاملة تخلص");

        if (transaction.Status != AssetTransactionStatus.Draft)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن الموافقة على معاملة التخلص إلا من حالة مسودة");

        transaction.Status = AssetTransactionStatus.Approved;
        transaction.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

[Authorize(Policy = PermissionCodes.AssetDisposalsPost)]
public record PostAssetDisposalCommand(int Id) : IRequest<Result<int>>;

public class PostAssetDisposalCommandHandler(
    IApplicationDbContext context) : IRequestHandler<PostAssetDisposalCommand, Result<int>>
{
    public async Task<Result<int>> Handle(PostAssetDisposalCommand request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (transaction is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة التخلص غير موجودة");

        if (transaction.Status != AssetTransactionStatus.Approved)
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن نشر معاملة التخلص إلا من حالة معتمدة");

        var asset = await context.Assets.FindAsync([transaction.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        var group = await context.AssetGroups.FindAsync([asset.AssetGroupId], ct);
        if (group?.DisposalAccountId is null || group.AssetAccountId is null)
            return Result<int>.Failure(ErrorCodes.Assets.PostingGateFailed, ErrorCategory.Validation, "مجموعة الأصل تفتقد حسابات الاستبعاد المطلوبة");

        var detail = await context.AssetDisposalDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);
        if (detail is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "تفاصيل التخلص غير موجودة");

        var periodResult = await AssetPostingGuards.ResolveOpenPeriodAsync(context, transaction.TransactionDate, ct);
        if (!periodResult.Succeeded)
            return Result<int>.Failure(periodResult.Code!, periodResult.Category!.Value, periodResult.Message!);

        var accountIds = new List<int> { group.DisposalAccountId.Value, group.AssetAccountId.Value };
        if (asset.AccumulatedDepreciation > 0 && group.AccumulatedDepreciationAccountId.HasValue)
            accountIds.Add(group.AccumulatedDepreciationAccountId.Value);

        if (!await AssetPostingGuards.AreAccountsPostableAsync(context, accountIds, ct))
            return Result<int>.Failure(ErrorCodes.Accounting.AccountNotPostable, ErrorCategory.Validation, "أحد حسابات التخلص غير قابل للترحيل");

        transaction.Status = AssetTransactionStatus.Posting;
        transaction.LastModified = DateTimeOffset.UtcNow;

        transaction.AddDomainEvent(new AssetDisposed
        {
            SourceEntityId = transaction.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            AssetId = transaction.AssetId,
            NetBookValue = detail.BookValueAtDisposal,
            SalePrice = detail.SaleProceeds,
            PeriodId = periodResult.Value!.Id
        });

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

public class ApproveAssetDisposalCommandValidator : AbstractValidator<ApproveAssetDisposalCommand>
{
    public ApproveAssetDisposalCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("معرف معاملة التخلص مطلوب");
    }
}
