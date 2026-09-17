namespace ERP_Government.Application.Accounting.Reports.BalanceSheet;

public class GetBalanceSheetQueryValidator : AbstractValidator<GetBalanceSheetQuery>
{
    public GetBalanceSheetQueryValidator()
    {
        RuleFor(x => x.AsOfDate)
            .NotEmpty()
            .WithMessage("As of date is required.");

        RuleFor(x => x.AsOfDate)
            .Must(date => DateOnly.TryParse(date, out _))
            .When(x => !string.IsNullOrEmpty(x.AsOfDate))
            .WithMessage("Invalid date format. Use YYYY-MM-DD.");

        RuleFor(x => x.AsOfDate)
            .Must(date => DateOnly.TryParse(date, out var parsed) && parsed <= DateOnly.FromDateTime(DateTime.Today))
            .When(x => !string.IsNullOrEmpty(x.AsOfDate))
            .WithMessage("As of date cannot be in the future.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0)
            .When(x => x.FiscalPeriodId.HasValue)
            .WithMessage("Invalid fiscal period ID.");
    }
}
