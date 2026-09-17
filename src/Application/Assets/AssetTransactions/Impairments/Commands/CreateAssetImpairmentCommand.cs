using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Impairments.Commands;

[Authorize(Policy = PermissionCodes.AssetImpairmentsCreate)]
public record CreateAssetImpairmentCommand(
    int AssetId,
    DateOnly TransactionDate,
    decimal ImpairmentAmount,
    string? Notes) : IRequest<Result<int>>;

public class CreateAssetImpairmentCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetImpairmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetImpairmentCommand request, CancellationToken ct)
    {
        var asset = await context.Assets.FindAsync([request.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        if (asset.Status != "Active")
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن تسجيل انخفاض في قيمة أصل غير نشط");

        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetImpairment", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل في توليد رقم الانخفاض");
        }

        var transaction = new AssetTransaction
        {
            TransactionNumber = number,
            AssetId = request.AssetId,
            TransactionType = AssetTransactionType.Impairment,
            TransactionDate = request.TransactionDate,
            Status = AssetTransactionStatus.Draft,
            CurrencyId = asset.CurrencyId,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetTransactions.Add(transaction);
        await context.SaveChangesAsync(ct);

        var detail = new AssetImpairmentDetail
        {
            AssetTransactionId = transaction.Id,
            CarryingAmount = asset.CurrentValue ?? asset.OriginalValue,
            RecoverableAmount = (asset.CurrentValue ?? asset.OriginalValue) - request.ImpairmentAmount,
            LossAmount = request.ImpairmentAmount,
            Reason = request.Notes ?? string.Empty
        };

        context.AssetImpairmentDetails.Add(detail);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

public class CreateAssetImpairmentCommandValidator : AbstractValidator<CreateAssetImpairmentCommand>
{
    public CreateAssetImpairmentCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0).WithMessage("معرف الأصل مطلوب");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("تاريخ الانخفاض مطلوب");

        RuleFor(x => x.ImpairmentAmount)
            .GreaterThan(0).WithMessage("مبلغ الانخفاض يجب أن يكون أكبر من صفر");
    }
}
