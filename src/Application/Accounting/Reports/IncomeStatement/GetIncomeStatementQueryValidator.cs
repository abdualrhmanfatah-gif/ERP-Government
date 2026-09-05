namespace ERP_Government.Application.Accounting.Reports.IncomeStatement;

public class GetIncomeStatementQueryValidator : AbstractValidator<GetIncomeStatementQuery>
{
    public GetIncomeStatementQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.");

        RuleFor(x => x.StartDate)
            .Must(date => DateOnly.TryParse(date, out _))
            .When(x => !string.IsNullOrEmpty(x.StartDate))
            .WithMessage("Invalid start date format. Use YYYY-MM-DD.");

        RuleFor(x => x.EndDate)
            .Must(date => DateOnly.TryParse(date, out _))
            .When(x => !string.IsNullOrEmpty(x.EndDate))
            .WithMessage("Invalid end date format. Use YYYY-MM-DD.");
    }
}
