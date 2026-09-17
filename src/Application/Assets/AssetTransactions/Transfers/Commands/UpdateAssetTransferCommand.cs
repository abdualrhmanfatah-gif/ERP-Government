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
public record UpdateAssetTransferCommand(
    int Id,
    DateOnly TransactionDate,
    int? ToLocationId,
    int? ToEmployeeId,
    string? Notes,
    byte[] RowVersion) : IRequest<Result<int>>;

public class UpdateAssetTransferCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssetTransferCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateAssetTransferCommand request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);

        if (transaction is null || transaction.TransactionType != AssetTransactionType.Transfer)
            return Result<int>.Failure(ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "معاملة النقل غير موجودة");

        if (transaction.Status != AssetTransactionStatus.Draft)
            return Result<int>.Failure(
                ErrorCodes.Assets.InvalidStatusTransition,
                ErrorCategory.Validation,
                "لا يمكن تعديل نقل ليس في حالة مسودة");

        if (!AssetTransferRules.RowVersionMatches(transaction.RowVersion, request.RowVersion))
            return AssetTransferRules.ConcurrencyConflict<int>();

        var asset = await context.Assets.FindAsync([transaction.AssetId], ct);
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

        var detail = await context.AssetTransferDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);

        if (detail is null)
            return Result<int>.Failure(ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "تفاصيل النقل غير موجودة");

        var effectiveToEmployeeId = request.ToEmployeeId ?? asset.EmployeeId;

        transaction.TransactionDate = request.TransactionDate;
        transaction.Notes = request.Notes;
        transaction.LastModified = DateTimeOffset.UtcNow;

        detail.FromLocationId = asset.LocationId;
        detail.ToLocationId = request.ToLocationId ?? asset.LocationId;
        detail.FromEmployeeId = asset.EmployeeId;
        detail.ToEmployeeId = effectiveToEmployeeId;
        detail.FromDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, asset.EmployeeId, ct);
        detail.ToDepartmentId = await AssetTransferRules.ResolveDepartmentIdAsync(context, effectiveToEmployeeId, ct);

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

public class UpdateAssetTransferCommandValidator : AbstractValidator<UpdateAssetTransferCommand>
{
    public UpdateAssetTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف النقل مطلوب");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("تاريخ النقل مطلوب");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار الصف مطلوب للتحقق من التزامن");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("الملاحظات يجب أن لا تتجاوز 2000 حرف");
    }
}
