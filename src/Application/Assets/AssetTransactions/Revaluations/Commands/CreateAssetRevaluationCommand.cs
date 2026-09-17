using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Revaluations.Commands;

[Authorize(Policy = PermissionCodes.AssetRevaluationsCreate)]
public record CreateAssetRevaluationCommand(
    int AssetId,
    DateOnly TransactionDate,
    decimal NewValue,
    string? Notes) : IRequest<Result<int>>;

public class CreateAssetRevaluationCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetRevaluationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetRevaluationCommand request, CancellationToken ct)
    {
        var asset = await context.Assets.FindAsync([request.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        if (asset.Status != "Active")
            return Result<int>.Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن إعادة تقييم أصل غير نشط");

        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetRevaluation", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل في توليد رقم إعادة التقييم");
        }

        var currentValue = asset.CurrentValue ?? asset.OriginalValue;
        var revaluationAmount = request.NewValue - currentValue;

        var transaction = new AssetTransaction
        {
            TransactionNumber = number,
            AssetId = request.AssetId,
            TransactionType = AssetTransactionType.Revaluation,
            TransactionDate = request.TransactionDate,
            Status = AssetTransactionStatus.Draft,
            CurrencyId = asset.CurrencyId,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetTransactions.Add(transaction);
        await context.SaveChangesAsync(ct);

        var detail = new AssetRevaluationDetail
        {
            AssetTransactionId = transaction.Id,
            OldBookValue = currentValue,
            NewBookValue = request.NewValue,
            RevaluationAmount = revaluationAmount,
            RevaluationType = revaluationAmount > 0 ? "Up" : revaluationAmount < 0 ? "Down" : "None"
        };

        context.AssetRevaluationDetails.Add(detail);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

public class CreateAssetRevaluationCommandValidator : AbstractValidator<CreateAssetRevaluationCommand>
{
    public CreateAssetRevaluationCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0).WithMessage("معرف الأصل مطلوب");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("تاريخ إعادة التقييم مطلوب");

        RuleFor(x => x.NewValue)
            .GreaterThan(0).WithMessage("القيمة الجديدة يجب أن تكون أكبر من صفر");
    }
}
