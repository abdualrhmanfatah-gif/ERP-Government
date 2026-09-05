namespace ERP_Government.Application.Accounting.Reports.GeneralLedger;

public class GetGeneralLedgerQueryValidator : AbstractValidator<GetGeneralLedgerQuery>
{
    public GetGeneralLedgerQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .When(x => x.AccountId.HasValue)
            .WithMessage("Invalid account ID.");

        RuleFor(x => x.AccountCode)
            .NotEmpty()
            .When(x => x.AccountCode != null)
            .WithMessage("Account code cannot be empty.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .When(x => x.PageSize.HasValue)
            .WithMessage("Page size must be between 1 and 1000.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .When(x => x.Page.HasValue)
            .WithMessage("Page must be greater than 0.");

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
