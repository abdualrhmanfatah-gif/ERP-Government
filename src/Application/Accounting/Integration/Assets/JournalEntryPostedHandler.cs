using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Accounting;

namespace ERP_Government.Application.Accounting.Integration.Assets;

public class JournalEntryPostedHandler(IApplicationDbContext context)
    : INotificationHandler<JournalEntryPosted>
{
    public async Task Handle(JournalEntryPosted notification, CancellationToken ct)
    {
        var runId = await context.JournalEntryLines
            .Where(l => l.JournalEntryId == notification.JournalEntryId
                     && l.DepreciationScheduleLineId != null)
            .Select(l => l.DepreciationScheduleLine!.DepreciationRunId)
            .FirstOrDefaultAsync(ct);

        if (runId == 0) return;

        var run = await context.DepreciationRuns
            .Include(r => r.ScheduleLines)
                .ThenInclude(s => s.Asset)
            .FirstOrDefaultAsync(r => r.Id == runId, ct);

        if (run is null || run.Status == DepreciationRunStatus.Posted) return;

        foreach (var schedule in run.ScheduleLines)
        {
            var asset = schedule.Asset!;
            asset.AccumulatedDepreciation = schedule.ClosingAccumulatedDepreciation;
            asset.CurrentValue = schedule.ClosingBookValue;
            asset.LastDepreciationDate = run.DepreciationDate;
            asset.IsFullyDepreciated = schedule.ClosingAccumulatedDepreciation >= (asset.OriginalValue - CalculateResidualValue(asset));
        }

        run.Status = DepreciationRunStatus.Posted;
        run.PostedAt = DateTimeOffset.UtcNow;
        run.PostedBy = "System";
        await context.SaveChangesAsync(ct);
    }

    private static decimal CalculateResidualValue(Domain.Assets.Entities.Asset asset)
    {
        var residualPercentage = asset.AssetGroup?.ResidualValuePercentage ?? 0;
        return Math.Round(asset.OriginalValue * residualPercentage / 100, 2);
    }
}
