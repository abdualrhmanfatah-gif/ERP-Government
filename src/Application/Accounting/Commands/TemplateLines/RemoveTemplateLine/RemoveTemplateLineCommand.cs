using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Commands.TemplateLines.RemoveTemplateLine;

[Authorize(Policy = PermissionCodes.TemplatesUpdate)]
public class RemoveTemplateLineCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
}

public class RemoveTemplateLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RemoveTemplateLineCommand, Result>
{
    public async Task<Result> Handle(
        RemoveTemplateLineCommand request,
        CancellationToken cancellationToken)
    {
        var template = await context.JournalEntryTemplates
            .FindAsync(request.TemplateId, cancellationToken);

        if (template is null)
            return Result.Failure(["Template not found."]);

        var entity = await context.JournalEntryTemplateLines
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Template line not found."]);

        if (entity.TemplateId != request.TemplateId)
            return Result.Failure(["Template line does not belong to the specified template."]);

        context.JournalEntryTemplateLines.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RemoveTemplateLineCommandValidator : AbstractValidator<RemoveTemplateLineCommand>
{
    public RemoveTemplateLineCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid template line ID.");

        RuleFor(x => x.TemplateId)
            .GreaterThan(0).WithMessage("Template is required.");
    }
}
