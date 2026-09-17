using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Impairments.Commands;

public record AssetImpairmentResponse(
    int Id, string TransactionNumber, int AssetId, DateOnly TransactionDate,
    string Status, decimal CarryingAmount, decimal RecoverableAmount, decimal LossAmount);

[Authorize(Policy = PermissionCodes.AssetImpairmentsView)]
public record GetAssetImpairmentsQuery : IRequest<List<AssetImpairmentResponse>>;

public class GetAssetImpairmentsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetImpairmentsQuery, List<AssetImpairmentResponse>>
{
    public async Task<List<AssetImpairmentResponse>> Handle(GetAssetImpairmentsQuery request, CancellationToken ct)
    {
        return await context.AssetTransactions
            .Where(t => t.TransactionType == AssetTransactionType.Impairment)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new AssetImpairmentResponse(
                t.Id, t.TransactionNumber, t.AssetId, t.TransactionDate,
                t.Status.ToString(), 0, 0, 0))
            .ToListAsync(ct);
    }
}

[Authorize(Policy = PermissionCodes.AssetImpairmentsView)]
public record GetAssetImpairmentByIdQuery(int Id) : IRequest<Result<AssetImpairmentResponse>>;

public class GetAssetImpairmentByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetImpairmentByIdQuery, Result<AssetImpairmentResponse>>
{
    public async Task<Result<AssetImpairmentResponse>> Handle(GetAssetImpairmentByIdQuery request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (transaction is null)
            return Result<AssetImpairmentResponse>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة انخفاض القيمة غير موجودة");

        var detail = await context.AssetImpairmentDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);

        return Result<AssetImpairmentResponse>.Success(new AssetImpairmentResponse(
            transaction.Id, transaction.TransactionNumber, transaction.AssetId, transaction.TransactionDate,
            transaction.Status.ToString(), detail?.CarryingAmount ?? 0, detail?.RecoverableAmount ?? 0, detail?.LossAmount ?? 0));
    }
}
