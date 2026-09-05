using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.RecurringEntries.CreateRecurringEntry;

[Authorize(Policy = PermissionCodes.RecurringEntriesCreate)]
public class CreateRecurringEntryCommand : IRequest<Result>
{
    public int? TemplateId { get; init; }
    public int JournalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public RecurringFrequency Frequency { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal? Amount { get; init; }
    public int? CurrencyId { get; init; }
    public int? FundId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
    public string? DescriptionTemplate { get; init; }
}

public class CreateRecurringEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateRecurringEntryCommand, Result>
{
    public async Task<Result> Handle(
        CreateRecurringEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TemplateId.HasValue)
        {
            var template = await context.JournalEntryTemplates
                .FindAsync(request.TemplateId.Value, cancellationToken);

            if (template is null)
                return Result.Failure(["Template not found."]);
        }

        var journal = await context.Journals
            .FindAsync(request.JournalId, cancellationToken);

        if (journal is null)
            return Result.Failure(["Journal not found."]);

        var entity = new RecurringEntry
        {
            EntryNumber = $"RE-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TemplateId = request.TemplateId,
            JournalId = request.JournalId,
            Name = request.Name,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextExecutionDate = request.StartDate,
            Amount = request.Amount,
            CurrencyId = request.CurrencyId,
            FundId = request.FundId,
            CostCenterId = request.CostCenterId,
            ProjectId = request.ProjectId,
            DescriptionTemplate = request.DescriptionTemplate,
            Status = RecurringEntryStatus.Active,
            IsActive = true
        };

        context.RecurringEntries.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateRecurringEntryCommandValidator : AbstractValidator<CreateRecurringEntryCommand>
{
    public CreateRecurringEntryCommandValidator()
    {
        RuleFor(x => x.JournalId)
            .GreaterThan(0).WithMessage("Journal is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Invalid frequency.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
    }
}
