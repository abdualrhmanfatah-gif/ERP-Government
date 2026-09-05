using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Templates.CreateTemplate;

[Authorize(Policy = PermissionCodes.TemplatesCreate)]
public class CreateTemplateCommand : IRequest<Result>
{
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int JournalId { get; init; }
    public JournalEntryTemplateType TemplateType { get; init; }
    public bool IsSystemTemplate { get; init; }
}

public class CreateTemplateCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateTemplateCommand, Result>
{
    public async Task<Result> Handle(
        CreateTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var journal = await context.Journals
            .FindAsync(request.JournalId, cancellationToken);

        if (journal is null)
            return Result.Failure(["Journal not found."]);

        var entity = new JournalEntryTemplate
        {
            TemplateName = request.TemplateName,
            Description = request.Description,
            JournalId = request.JournalId,
            TemplateType = request.TemplateType,
            IsSystemTemplate = request.IsSystemTemplate,
            IsActive = true
        };

        context.JournalEntryTemplates.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters.");

        RuleFor(x => x.JournalId)
            .GreaterThan(0).WithMessage("Journal is required.");

        RuleFor(x => x.TemplateType)
            .IsInEnum().WithMessage("Invalid template type.");
    }
}
