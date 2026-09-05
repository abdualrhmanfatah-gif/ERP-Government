using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsCreate)]
public class BulkGeneratePeriodsCommand : IRequest<Result>
{
    public int FiscalYearId { get; init; }
}

public class BulkGeneratePeriodsCommandHandler(
    IApplicationDbContext context) : IRequestHandler<BulkGeneratePeriodsCommand, Result>
{
    private static readonly string[] ArabicMonthNames =
    [
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    ];

    public async Task<Result> Handle(
        BulkGeneratePeriodsCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears
            .FindAsync(request.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result.Failure(["Fiscal year not found."]);

        if (fiscalYear.Status is FiscalYearStatus.SoftClosed or FiscalYearStatus.HardClosed)
            return Result.Failure(["Cannot generate periods for a closed fiscal year."]);

        var hasExistingPeriods = await context.FiscalPeriods
            .AnyAsync(fp => fp.FiscalYearId == request.FiscalYearId, cancellationToken);

        if (hasExistingPeriods)
            return Result.Failure(["Periods already exist for this fiscal year."]);

        var year = fiscalYear.StartDate.Year;
        var periods = new List<FiscalPeriod>();

        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            periods.Add(new FiscalPeriod
            {
                FiscalYearId = request.FiscalYearId,
                PeriodNumber = month,
                Name = $"{ArabicMonthNames[month - 1]} {year}",
                StartDate = startDate,
                EndDate = endDate,
                IsLockedForPosting = false,
                IsActive = true
            });
        }

        context.FiscalPeriods.AddRange(periods);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class BulkGeneratePeriodsCommandValidator : AbstractValidator<BulkGeneratePeriodsCommand>
{
    public BulkGeneratePeriodsCommandValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year ID is required.");
    }
}
