using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Disposals.Commands;

[Authorize(Policy = PermissionCodes.AssetDisposalsCreate)]
public record CreateAssetDisposalCommand(
    int AssetId,
    DateOnly TransactionDate,
    decimal? SalePrice,
    decimal? DisposalCost,
    string? BuyerName,
    string? BuyerContact,
    string? Notes) : IRequest<Result<int>>;

public class CreateAssetDisposalCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetDisposalCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetDisposalCommand request, CancellationToken ct)
    {
        var asset = await context.Assets.FindAsync([request.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        if (asset.Status != "Active")
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن التخلص من أصل غير نشط");

        var hasExistingDisposal = await context.AssetTransactions
            .AnyAsync(t => t.AssetId == request.AssetId
                && t.TransactionType == AssetTransactionType.Disposal
                && t.Status != AssetTransactionStatus.Reversed, ct);

        if (hasExistingDisposal)
            return Result<int>.Failure(ErrorCodes.Assets.DuplicateDisposal, ErrorCategory.Validation, "تم التخلص من هذا الأصل مسبقاً");

        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetDisposal", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل في توليد رقم التخلص");
        }

        var bookValueAtDisposal = asset.CurrentValue ?? asset.OriginalValue - asset.AccumulatedDepreciation;
        var netProceeds = (request.SalePrice ?? 0) - (request.DisposalCost ?? 0);
        var gainOrLoss = netProceeds - bookValueAtDisposal;

        var transaction = new AssetTransaction
        {
            TransactionNumber = number,
            AssetId = request.AssetId,
            TransactionType = AssetTransactionType.Disposal,
            TransactionDate = request.TransactionDate,
            Status = AssetTransactionStatus.Draft,
            CurrencyId = asset.CurrencyId,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetTransactions.Add(transaction);
        await context.SaveChangesAsync(ct);

        var detail = new AssetDisposalDetail
        {
            AssetTransactionId = transaction.Id,
            DisposalMethod = "Sale",
            BookValueAtDisposal = bookValueAtDisposal,
            AccumulatedDepreciationAtDisposal = asset.AccumulatedDepreciation,
            SaleProceeds = request.SalePrice,
            DisposalCost = request.DisposalCost,
            NetProceeds = netProceeds,
            GainOrLoss = gainOrLoss,
            BuyerName = request.BuyerName,
            BuyerContact = request.BuyerContact
        };

        context.AssetDisposalDetails.Add(detail);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

public class CreateAssetDisposalCommandValidator : AbstractValidator<CreateAssetDisposalCommand>
{
    public CreateAssetDisposalCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0).WithMessage("معرف الأصل مطلوب");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("تاريخ التخلص مطلوب");
    }
}
