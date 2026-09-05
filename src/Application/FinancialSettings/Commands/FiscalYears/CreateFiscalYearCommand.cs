using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalYears;

// C-F002 — CreateFiscalYearCommand
[Authorize(Policy = PermissionCodes.FiscalYearsCreate)]
public class CreateFiscalYearCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public int YearNumber { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

public class CreateFiscalYearCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateFiscalYearCommand, Result>
{
    public async Task<Result> Handle(
        CreateFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.FiscalYears
            .AnyAsync(x => x.YearNumber == request.YearNumber, cancellationToken);

        if (exists)
            return Result.Failure(["Fiscal year number already exists."]);

        var entity = new FiscalYear
        {
            Name = request.Name,
            YearNumber = request.YearNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = FiscalYearStatus.Draft,
            IsClosed = false,
            IsActive = true
        };

        context.FiscalYears.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateFiscalYearCommandValidator : AbstractValidator<CreateFiscalYearCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateFiscalYearCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.YearNumber)
            .GreaterThan(0).WithMessage("Year number must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => new { x.StartDate, x.EndDate })
            .MustAsync(async (dates, ct) =>
            {
                return !await _context.FiscalYears.AnyAsync(
                    fy => fy.StartDate <= dates.EndDate && fy.EndDate >= dates.StartDate,
                    ct);
            })
            .WithMessage("Fiscal year date range overlaps with an existing fiscal year.");
    }
}
