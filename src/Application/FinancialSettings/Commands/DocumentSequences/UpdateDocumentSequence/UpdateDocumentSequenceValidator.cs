namespace ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.UpdateDocumentSequence;

public class UpdateDocumentSequenceValidator : AbstractValidator<UpdateDocumentSequenceCommand>
{
    public UpdateDocumentSequenceValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid sequence ID.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty.");
        });
    }
}
