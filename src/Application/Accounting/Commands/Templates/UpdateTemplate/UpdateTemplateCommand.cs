using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Templates.UpdateTemplate;

[Authorize(Policy = PermissionCodes.TemplatesUpdate)]
public class UpdateTemplateCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int JournalId { get; init; }
    public JournalEntryTemplateType TemplateType { get; init; }
    public bool IsSystemTemplate { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateTemplateCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateTemplateCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntryTemplates
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Template not found."]);

        var journal = await context.Journals
            .FindAsync(request.JournalId, cancellationToken);

        if (journal is null)
            return Result.Failure(["Journal not found."]);

        entity.TemplateName = request.TemplateName;
        entity.Description = request.Description;
        entity.JournalId = request.JournalId;
        entity.TemplateType = request.TemplateType;
        entity.IsSystemTemplate = request.IsSystemTemplate;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid template ID.");

        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters.");

        RuleFor(x => x.JournalId)
            .GreaterThan(0).WithMessage("Journal is required.");

        RuleFor(x => x.TemplateType)
            .IsInEnum().WithMessage("Invalid template type.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
