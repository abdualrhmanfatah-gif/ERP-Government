using ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Transfers.Queries;

[Authorize(Policy = PermissionCodes.AssetTransfersView)]
public record GetAssetTransferByIdQuery(int Id) : IRequest<Result<AssetTransferDetailResponse>>;

public class GetAssetTransferByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetTransferByIdQuery, Result<AssetTransferDetailResponse>>
{
    public async Task<Result<AssetTransferDetailResponse>> Handle(
        GetAssetTransferByIdQuery request,
        CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);

        if (transaction is null || transaction.TransactionType != AssetTransactionType.Transfer)
            return Result<AssetTransferDetailResponse>.Failure(
                ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "معاملة النقل غير موجودة");

        var asset = await context.Assets.FindAsync([transaction.AssetId], ct);
        if (asset is null)
            return Result<AssetTransferDetailResponse>.Failure(
                ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "الأصل غير موجود");

        var detail = await context.AssetTransferDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);

        if (detail is null)
            return Result<AssetTransferDetailResponse>.Failure(
                ErrorCodes.Assets.TransferNotFound, ErrorCategory.NotFound, "تفاصيل النقل غير موجودة");

        var fromLocationName = await context.Locations
            .Where(l => l.Id == detail.FromLocationId)
            .Select(l => l.Name)
            .FirstOrDefaultAsync(ct);

        var toLocationName = await context.Locations
            .Where(l => l.Id == detail.ToLocationId)
            .Select(l => l.Name)
            .FirstOrDefaultAsync(ct);

        var fromEmployeeName = await context.Employees
            .Where(e => e.Id == detail.FromEmployeeId)
            .Select(e => e.Name)
            .FirstOrDefaultAsync(ct);

        var toEmployeeName = await context.Employees
            .Where(e => e.Id == detail.ToEmployeeId)
            .Select(e => e.Name)
            .FirstOrDefaultAsync(ct);

        var fromDepartmentName = await context.OrganizationalUnits
            .Where(u => u.Id == detail.FromDepartmentId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(ct);

        var toDepartmentName = await context.OrganizationalUnits
            .Where(u => u.Id == detail.ToDepartmentId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(ct);

        return Result<AssetTransferDetailResponse>.Success(new AssetTransferDetailResponse(
            transaction.Id,
            transaction.TransactionNumber,
            transaction.AssetId,
            asset.Code,
            asset.Name,
            transaction.TransactionDate,
            transaction.Status.ToString(),
            transaction.CurrencyId,
            transaction.Notes,
            detail.OccurredAt,
            detail.FromLocationId,
            fromLocationName,
            detail.ToLocationId,
            toLocationName,
            detail.FromEmployeeId,
            fromEmployeeName,
            detail.ToEmployeeId,
            toEmployeeName,
            detail.FromDepartmentId,
            fromDepartmentName,
            detail.ToDepartmentId,
            toDepartmentName,
            transaction.RowVersion,
            asset.RowVersion));
    }
}
