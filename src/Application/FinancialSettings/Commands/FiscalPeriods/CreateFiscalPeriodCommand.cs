using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsCreate)]
public class CreateFiscalPeriodCommand : IRequest<Result>
{
    public int FiscalYearId { get; init; }
    public int PeriodNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

public class CreateFiscalPeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateFiscalPeriodCommand, Result>
{
    public async Task<Result> Handle(
        CreateFiscalPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears
            .FindAsync(request.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result.Failure(["Fiscal year not found."]);

        var entity = new FiscalPeriod
        {
            FiscalYearId = request.FiscalYearId,
            PeriodNumber = request.PeriodNumber,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsLockedForPosting = false,
            IsActive = true
        };

        context.FiscalPeriods.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateFiscalPeriodCommandValidator : AbstractValidator<CreateFiscalPeriodCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateFiscalPeriodCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year ID is required.");

        RuleFor(x => x.PeriodNumber)
            .GreaterThan(0).WithMessage("Period number must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => new { x.FiscalYearId, x.PeriodNumber })
            .MustAsync(async (ids, ct) =>
            {
                return !await _context.FiscalPeriods.AnyAsync(
                    fp => fp.FiscalYearId == ids.FiscalYearId && fp.PeriodNumber == ids.PeriodNumber,
                    ct);
            })
            .WithMessage("Period number already exists for this fiscal year.");

        RuleFor(x => new { x.FiscalYearId, x.StartDate, x.EndDate })
            .MustAsync(async (dates, ct) =>
            {
                var fy = await _context.FiscalYears.FindAsync(dates.FiscalYearId, ct);
                if (fy is null) return false;
                return dates.StartDate >= fy.StartDate && dates.EndDate <= fy.EndDate;
            })
            .WithMessage("Period must fall within the fiscal year date range.");

        RuleFor(x => new { x.FiscalYearId, x.StartDate, x.EndDate })
            .MustAsync(async (dates, ct) =>
            {
                return !await _context.FiscalPeriods.AnyAsync(
                    fp => fp.FiscalYearId == dates.FiscalYearId
                        && fp.StartDate <= dates.EndDate
                        && fp.EndDate >= dates.StartDate,
                    ct);
            })
            .WithMessage("Period dates overlap with an existing period.");
    }
}
