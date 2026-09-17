using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Revaluations.Commands;

public record AssetRevaluationResponse(
    int Id, string TransactionNumber, int AssetId, DateOnly TransactionDate,
    string Status, decimal PreviousValue, decimal NewValue, decimal RevaluationAmount);

[Authorize(Policy = PermissionCodes.AssetRevaluationsView)]
public record GetAssetRevaluationsQuery : IRequest<List<AssetRevaluationResponse>>;

public class GetAssetRevaluationsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetRevaluationsQuery, List<AssetRevaluationResponse>>
{
    public async Task<List<AssetRevaluationResponse>> Handle(GetAssetRevaluationsQuery request, CancellationToken ct)
    {
        return await context.AssetTransactions
            .Where(t => t.TransactionType == AssetTransactionType.Revaluation)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new AssetRevaluationResponse(
                t.Id, t.TransactionNumber, t.AssetId, t.TransactionDate,
                t.Status.ToString(), 0, 0, 0))
            .ToListAsync(ct);
    }
}

[Authorize(Policy = PermissionCodes.AssetRevaluationsView)]
public record GetAssetRevaluationByIdQuery(int Id) : IRequest<Result<AssetRevaluationResponse>>;

public class GetAssetRevaluationByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetRevaluationByIdQuery, Result<AssetRevaluationResponse>>
{
    public async Task<Result<AssetRevaluationResponse>> Handle(GetAssetRevaluationByIdQuery request, CancellationToken ct)
    {
        var transaction = await context.AssetTransactions.FindAsync([request.Id], ct);
        if (transaction is null)
            return Result<AssetRevaluationResponse>.Failure(ErrorCodes.Assets.AssetNotFound, ErrorCategory.NotFound, "معاملة إعادة التقييم غير موجودة");

        var detail = await context.AssetRevaluationDetails
            .FirstOrDefaultAsync(d => d.AssetTransactionId == request.Id, ct);

        return Result<AssetRevaluationResponse>.Success(new AssetRevaluationResponse(
            transaction.Id, transaction.TransactionNumber, transaction.AssetId, transaction.TransactionDate,
            transaction.Status.ToString(), detail?.OldBookValue ?? 0, detail?.NewBookValue ?? 0, detail?.RevaluationAmount ?? 0));
    }
}
