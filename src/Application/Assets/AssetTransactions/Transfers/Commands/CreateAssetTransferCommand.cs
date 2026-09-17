using ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;

[Authorize(Policy = PermissionCodes.AssetTransfersCreate)]
public record CreateAssetTransferCommand(
    int AssetId,
    DateOnly TransactionDate,
    int? ToLocationId,
    int? ToEmployeeId,
    string? Notes) : IRequest<Result<int>>;

public class CreateAssetTransferCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetTransferCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetTransferCommand request, CancellationToken ct)
    {
        var asset = await context.Assets.FindAsync([request.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        if (asset.Status != "Active")
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                "لا يمكن نقل أصل غير نشط");

        var destination = await AssetTransferRules.ValidateDestinationAsync(
            context,
            asset.LocationId,
            asset.EmployeeId,
            request.ToLocationId,
            request.ToEmployeeId,
            ct);

        if (!destination.Succeeded)
            return Result<int>.Failure(destination.Code!, destination.Category!.Value, destination.Message!);

        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetTransfer", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(
                ErrorCodes.Request.InternalError,
                ErrorCategory.Internal,
                "فشل في توليد رقم النقل");
        }

        var effectiveToEmployeeId = request.ToEmployeeId ?? asset.EmployeeId;

        var transaction = new AssetTransaction
        {
            TransactionNumber = number,
            AssetId = request.AssetId,
            TransactionType = AssetTransactionType.Transfer,
            TransactionDate = request.TransactionDate,
            Status = AssetTransactionStatus.Draft,
            CurrencyId = asset.CurrencyId,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        var detail = new AssetTransferDetail
        {
            AssetTransaction = transaction,
            OccurredAt = DateTimeOffset.UtcNow,
            FromLocationId = asset.LocationId,
            ToLocationId = request.ToLocationId ?? asset.LocationId,
            FromEmployeeId = asset.EmployeeId,
            ToEmployeeId = effectiveToEmployeeId,
            FromDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, asset.EmployeeId, ct),
            ToDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, effectiveToEmployeeId, ct)
        };

        context.AssetTransactions.Add(transaction);
        context.AssetTransferDetails.Add(detail);

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}

public class CreateAssetTransferCommandValidator : AbstractValidator<CreateAssetTransferCommand>
{
    public CreateAssetTransferCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0).WithMessage("معرف الأصل مطلوب");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("تاريخ النقل مطلوب");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("الملاحظات يجب أن لا تتجاوز 2000 حرف");
    }
}
