using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Post.Commands;

[Authorize(Policy = PermissionCodes.AssetDepreciationPost)]
public record PostDepreciationCommand(int RunId) : IRequest<Result<int>>;

public class PostDepreciationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<PostDepreciationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(PostDepreciationCommand request, CancellationToken ct)
    {
        var run = await context.DepreciationRuns
            .Include(r => r.ScheduleLines)
                .ThenInclude(s => s.Asset)
            .FirstOrDefaultAsync(r => r.Id == request.RunId, ct);

        if (run is null)
            return Failure(ErrorCodes.Assets.DepreciationRunNotFound, ErrorCategory.NotFound, "عملية الإهلاك غير موجودة");
        if (run.Status != DepreciationRunStatus.Draft)
            return Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن ترحيل العملية إلا من حالة مسودة");
        if (run.ScheduleLines.Count == 0 || run.TotalDepreciation <= 0)
            return Failure(ErrorCodes.Assets.NoSchedulesToPost, ErrorCategory.Validation, "لا توجد تفاصيل إهلاك قابلة للترحيل");

        var period = await context.FiscalPeriods.FindAsync([run.FiscalPeriodId], ct);
        if (period is null || period.FiscalYearId != run.FiscalYearId)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, ErrorCategory.Validation, "الفترة المالية غير صالحة");
        if (period.IsLockedForPosting || !period.IsActive)
            return Failure(ErrorCodes.Accounting.PeriodClosed, ErrorCategory.Validation, "الفترة المالية مغلقة أو غير مفعلة للترحيل");

        var groupIds = run.ScheduleLines.Select(s => s.Asset!.AssetGroupId).Distinct().ToList();
        var groups = await context.AssetGroups
            .Where(g => groupIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, ct);

        var errors = new List<string>();
        var accountIds = new HashSet<int>();
        foreach (var schedule in run.ScheduleLines.Where(s => s.Amount > 0))
        {
            var group = groups.GetValueOrDefault(schedule.Asset!.AssetGroupId);
            if (group?.DepreciationExpenseAccountId is null || group.AccumulatedDepreciationAccountId is null)
            {
                errors.Add($"المجموعة «{group?.Name ?? schedule.Asset!.AssetGroupId.ToString()}» لا تحدد حساب مصروف الإهلاك وحساب مجمع الإهلاك");
                continue;
            }

            accountIds.Add(group.DepreciationExpenseAccountId.Value);
            accountIds.Add(group.AccumulatedDepreciationAccountId.Value);
        }

        if (errors.Count > 0)
            return Failure(ErrorCodes.Assets.PostingGateFailed, ErrorCategory.Validation, string.Join(" | ", errors));

        var validAccountCount = await context.Accounts.CountAsync(a => accountIds.Contains(a.Id) && a.IsPostable && a.IsActive, ct);
        if (validAccountCount != accountIds.Count)
            return Failure(ErrorCodes.Accounting.AccountNotPostable, ErrorCategory.Validation, "أحد حسابات قيد الإهلاك غير قابل للترحيل أو غير نشط");

        run.Status = DepreciationRunStatus.Posting;
        run.AddDomainEvent(new DepreciationPosted
        {
            SourceEntityId = run.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            PeriodId = period.Id
        });
        await context.SaveChangesAsync(ct);
        return Result<int>.Success(run.Id);
    }

    private static Result<int> Failure(string code, ErrorCategory category, string message) =>
        Result<int>.Failure(code, category, message);
}

public class PostDepreciationCommandValidator : AbstractValidator<PostDepreciationCommand>
{
    public PostDepreciationCommandValidator() => RuleFor(x => x.RunId).GreaterThan(0);
}
