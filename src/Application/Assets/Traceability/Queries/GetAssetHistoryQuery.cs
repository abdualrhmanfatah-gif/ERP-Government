using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Traceability.Queries;

[Authorize(Policy = PermissionCodes.AssetsView)]
public record GetAssetHistoryQuery(int AssetId) : IRequest<Result<List<TransactionSummaryResponse>>>;

public record TransactionSummaryResponse(
    int Id,
    string DocumentNumber,
    AssetTransactionType TransactionType,
    DateOnly TransactionDate,
    AssetTransactionStatus Status,
    decimal? Amount,
    int? JournalEntryId,
    string? JournalEntryNumber,
    bool IsReversal,
    int? ReversalOfId);

public class GetAssetHistoryQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetHistoryQuery, Result<List<TransactionSummaryResponse>>>
{
    public async Task<Result<List<TransactionSummaryResponse>>> Handle(GetAssetHistoryQuery request, CancellationToken ct)
    {
        var entries = await (
            from t in context.AssetTransactions
            where t.AssetId == request.AssetId
            join je in context.JournalEntries on t.JournalEntryId equals (int?)je.Id into journalEntries
            from je in journalEntries.DefaultIfEmpty()
            join imp in context.AssetImpairmentDetails on t.Id equals imp.AssetTransactionId into impairments
            from imp in impairments.DefaultIfEmpty()
            orderby t.TransactionDate descending, t.Id descending
            select new TransactionSummaryResponse(
                t.Id,
                t.TransactionNumber,
                t.TransactionType,
                t.TransactionDate,
                t.Status,
                null,
                t.JournalEntryId,
                je != null ? je.EntryNumber : null,
                imp != null && imp.ReversalOfTransactionId != null,
                imp != null ? imp.ReversalOfTransactionId : null))
            .ToListAsync(ct);

        return Result<List<TransactionSummaryResponse>>.Success(entries);
    }
}
