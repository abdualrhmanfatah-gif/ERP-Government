using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;

namespace ERP_Government.Application.Assets.Depreciation.Reverse.Commands;

public record ReverseDepreciationCommand(
    int RunId,
    DateOnly ReversalDate,
    string Reason) : IRequest<Result<int>>;

public class ReverseDepreciationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReverseDepreciationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ReverseDepreciationCommand request, CancellationToken ct)
    {
        var run = await context.AssetDepreciationRuns.FindAsync([request.RunId], ct);
        if (run is null)
            return Failure(ErrorCodes.Assets.DepreciationRunNotFound, ErrorCategory.NotFound, "عملية الإهلاك غير موجودة");
        if (run.Status != AssetDepreciationRunStatus.Posted || run.JournalEntryId is null)
            return Failure(ErrorCodes.Assets.InvalidStatusTransition, ErrorCategory.Validation, "لا يمكن عكس عملية لم تُرحّل");

        var period = await context.FiscalPeriods
            .Where(p => p.StartDate <= request.ReversalDate && p.EndDate >= request.ReversalDate)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync(ct);
        if (period is null)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, ErrorCategory.Validation, "لا توجد فترة مالية تغطي تاريخ العكس");
        if (period.IsLockedForPosting || !period.IsActive)
            return Failure(ErrorCodes.Accounting.PeriodClosed, ErrorCategory.Validation, "فترة العكس مغلقة أو غير مفعلة للترحيل");

        run.Status = AssetDepreciationRunStatus.Reversing;
        run.ReversalDate = request.ReversalDate;
        run.ReversalReason = request.Reason;
        run.AddDomainEvent(new DepreciationReversed
        {
            SourceEntityId = run.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            PeriodId = period.Id,
            ReversalDate = request.ReversalDate
        });
        await context.SaveChangesAsync(ct);
        return Result<int>.Success(run.Id);
    }

    private static Result<int> Failure(string code, ErrorCategory category, string message) =>
        Result<int>.Failure(code, category, message);
}

public class ReverseDepreciationCommandValidator : AbstractValidator<ReverseDepreciationCommand>
{
    public ReverseDepreciationCommandValidator()
    {
        RuleFor(x => x.RunId).GreaterThan(0);
        RuleFor(x => x.ReversalDate).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
