namespace ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.CreateDocumentSequence;

public class CreateDocumentSequenceValidator : AbstractValidator<CreateDocumentSequenceCommand>
{
    public CreateDocumentSequenceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.")
            .MaximumLength(50).WithMessage("Document type must not exceed 50 characters.");
    }
}
