using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.PostingRules.UpdatePostingRule;

[Authorize(Policy = PermissionCodes.PostingRulesEdit)]
public class UpdatePostingRuleCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int JournalId { get; init; }
    public int Priority { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public List<PostingRuleLineDto> Lines { get; init; } = [];
}

public class PostingRuleLineDto
{
    public int? Id { get; init; } // null for new lines
    public int Sequence { get; init; }
    public string AccountSource { get; init; } = string.Empty;
    public int? FixedAccountId { get; init; }
    public string DebitOrCredit { get; init; } = string.Empty;
    public string AmountSource { get; init; } = string.Empty;
    public bool FundDimensionRequired { get; init; }
    public bool CostCenterDimensionRequired { get; init; }
    public bool ProjectDimensionRequired { get; init; }
}

public class UpdatePostingRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdatePostingRuleCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePostingRuleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PostingRules
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Posting rule not found."]);

        // Validate Journal exists
        var journal = await context.Journals
            .FindAsync(request.JournalId, cancellationToken);

        if (journal is null)
            return Result.Failure(["Journal not found."]);

        // Validate fixed accounts exist (if provided)
        foreach (var line in request.Lines.Where(x => x.FixedAccountId.HasValue))
        {
            var account = await context.Accounts
                .FindAsync(line.FixedAccountId!.Value, cancellationToken);

            if (account is null)
                return Result.Failure([$"Account with ID {line.FixedAccountId} not found."]);

            if (!account.IsPostable)
                return Result.Failure([$"Account {account.Code} is not postable."]);
        }

        // Update entity
        entity.Name = request.Name;
        entity.EventType = request.EventType;
        entity.JournalId = request.JournalId;
        entity.Priority = request.Priority;
        entity.IsActive = request.IsActive;

        // Get existing lines
        var existingLines = await context.PostingRuleLines
            .Where(x => x.PostingRuleId == request.Id)
            .ToListAsync(cancellationToken);

        // Remove lines not in the update
        var lineIds = request.Lines.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var linesToRemove = existingLines.Where(x => !lineIds.Contains(x.Id)).ToList();
        context.PostingRuleLines.RemoveRange(linesToRemove);

        // Update existing lines and add new ones
        foreach (var lineDto in request.Lines)
        {
            if (lineDto.Id.HasValue)
            {
                // Update existing line
                var existingLine = existingLines.FirstOrDefault(x => x.Id == lineDto.Id.Value);
                if (existingLine is not null)
                {
                    existingLine.Sequence = lineDto.Sequence;
                    existingLine.AccountSource = Enum.Parse<AccountSource>(lineDto.AccountSource);
                    existingLine.FixedAccountId = lineDto.FixedAccountId;
                    existingLine.DebitOrCredit = Enum.Parse<DebitOrCredit>(lineDto.DebitOrCredit);
                    existingLine.AmountSource = Enum.Parse<AmountSource>(lineDto.AmountSource);
                    existingLine.FundDimensionRequired = lineDto.FundDimensionRequired;
                    existingLine.CostCenterDimensionRequired = lineDto.CostCenterDimensionRequired;
                    existingLine.ProjectDimensionRequired = lineDto.ProjectDimensionRequired;
                }
            }
            else
            {
                // Add new line
                var newLine = new PostingRuleLine
                {
                    PostingRuleId = entity.Id,
                    Sequence = lineDto.Sequence,
                    AccountSource = Enum.Parse<AccountSource>(lineDto.AccountSource),
                    FixedAccountId = lineDto.FixedAccountId,
                    DebitOrCredit = Enum.Parse<DebitOrCredit>(lineDto.DebitOrCredit),
                    AmountSource = Enum.Parse<AmountSource>(lineDto.AmountSource),
                    FundDimensionRequired = lineDto.FundDimensionRequired,
                    CostCenterDimensionRequired = lineDto.CostCenterDimensionRequired,
                    ProjectDimensionRequired = lineDto.ProjectDimensionRequired,
                    IsActive = true
                };

                context.PostingRuleLines.Add(newLine);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdatePostingRuleCommandValidator : AbstractValidator<UpdatePostingRuleCommand>
{
    public UpdatePostingRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid posting rule ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required.")
            .MaximumLength(100).WithMessage("Event type must not exceed 100 characters.");

        RuleFor(x => x.JournalId)
            .GreaterThan(0).WithMessage("Journal is required.");

        RuleFor(x => x.Priority)
            .GreaterThan(0).WithMessage("Priority must be greater than zero.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one posting rule line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.Sequence)
                .GreaterThan(0).WithMessage("Sequence must be greater than zero.");

            line.RuleFor(x => x.AccountSource)
                .NotEmpty().WithMessage("Account source is required.");

            line.RuleFor(x => x.DebitOrCredit)
                .NotEmpty().WithMessage("Debit or credit is required.")
                .Must(x => x == "Debit" || x == "Credit")
                .WithMessage("Debit or credit must be 'Debit' or 'Credit'.");

            line.RuleFor(x => x.AmountSource)
                .NotEmpty().WithMessage("Amount source is required.");
        });
    }
}
