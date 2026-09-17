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

[Authorize(Policy = PermissionCodes.AssetTransfersExecute)]
public record ExecuteAssetTransferCommand(
    int TransactionId,
    byte[] RowVersion,
    byte[] AssetRowVersion) : IRequest<Result<int>>;

public class ExecuteAssetTransferCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ExecuteAssetTransferCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ExecuteAssetTransferCommand request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.TransactionId], ct);

        if (transaction is null || transaction.TransactionType != AssetTransactionType.Transfer)
            return Result<int>.Failure(ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "معاملة النقل غير موجودة");

        // Idempotent: a repeated execute request returns the same result without a second effect.
        if (transaction.Status == AssetTransactionStatus.Executed)
            return Result<int>.Success(transaction.Id);

        if (transaction.Status != AssetTransactionStatus.Draft)
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                "لا يمكن تنفيذ معاملة ليست في حالة مسودة");

        if (!AssetTransferRules.RowVersionMatches(transaction.RowVersion, request.RowVersion))
            return AssetTransferRules.ConcurrencyConflict<int>();

        var detail = await context.AssetTransferDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.TransactionId, ct);

        if (detail is null)
            return Result<int>.Failure(ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "تفاصيل النقل غير موجودة");

        var asset = await context.Assets.FindAsync([transaction.AssetId], ct);
        if (asset is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        if (asset.Status != "Active")
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                "لا يمكن تنفيذ نقل لأصل غير نشط");

        if (!AssetTransferRules.RowVersionMatches(asset.RowVersion, request.AssetRowVersion))
            return AssetTransferRules.ConcurrencyConflict<int>();

        // Re-validate that the card still matches the draft's source values (FR-052).
        if (detail.FromLocationId != asset.LocationId || detail.FromEmployeeId != asset.EmployeeId)
            return AssetTransferRules.SourceChanged<int>();

        var fromDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, asset.EmployeeId, ct);

        if (detail.ToLocationId.HasValue && detail.ToLocationId != detail.FromLocationId)
            asset.LocationId = detail.ToLocationId;

        if (detail.ToEmployeeId.HasValue && detail.ToEmployeeId != detail.FromEmployeeId)
            asset.EmployeeId = detail.ToEmployeeId;

        var toDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, asset.EmployeeId, ct);

        detail.OccurredAt = DateTimeOffset.UtcNow;
        detail.FromDepartmentId = fromDepartmentId;
        detail.ToDepartmentId = toDepartmentId;

        asset.LastModified = DateTimeOffset.UtcNow;
        transaction.Status = AssetTransactionStatus.Executed;
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

public class ExecuteAssetTransferCommandValidator : AbstractValidator<ExecuteAssetTransferCommand>
{
    public ExecuteAssetTransferCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .GreaterThan(0).WithMessage("معرف المعاملة مطلوب");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار صف النقل مطلوب للتحقق من التزامن");

        RuleFor(x => x.AssetRowVersion)
            .NotEmpty().WithMessage("إصدار صف الأصل مطلوب للتحقق من التزامن");
    }
}
