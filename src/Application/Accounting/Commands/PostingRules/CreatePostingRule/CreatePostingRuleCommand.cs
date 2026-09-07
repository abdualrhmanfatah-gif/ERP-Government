using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Commands.PostingRules.CreatePostingRule;

[Authorize(Policy = PermissionCodes.PostingRulesCreate)]
public class CreatePostingRuleCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int JournalId { get; init; }
    public int Priority { get; init; } = 100;
    public List<PostingRuleLineDto> Lines { get; init; } = [];
}

public class PostingRuleLineDto
{
    public int Sequence { get; init; }
    public string AccountSource { get; init; } = string.Empty;
    public int? FixedAccountId { get; init; }
    public string DebitOrCredit { get; init; } = string.Empty;
    public string AmountSource { get; init; } = string.Empty;
    public bool FundDimensionRequired { get; init; }
    public bool CostCenterDimensionRequired { get; init; }
    public bool ProjectDimensionRequired { get; init; }
}

public class CreatePostingRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreatePostingRuleCommand, Result>
{
    public async Task<Result> Handle(
        CreatePostingRuleCommand request,
        CancellationToken cancellationToken)
    {
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

        // Validate EventType is one of the known events
        var validEventTypes = new[]
        {
            "PurchaseOrder", "GoodsReceiptNote", "VendorPayment",
            "RevenueInvoice", "RevenueCollection", "PensionPayment", "ManualEntry",
            "StockTransaction", "StockTake",
            "AssetPurchase", "AssetDisposal", "AssetRevaluation", "AssetImpairment", "Depreciation",
            "BankReconciliation", "PeriodClosing", "Reversal",
            "AccruedExpense", "AccruedRevenue", "AccrualReversal"
        };

        if (!validEventTypes.Contains(request.EventType))
            return Result.Failure([$"Invalid event type: {request.EventType}. Valid types: {string.Join(", ", validEventTypes)}"]);

        var entity = new PostingRule
        {
            Name = request.Name,
            EventType = request.EventType,
            JournalId = request.JournalId,
            Priority = request.Priority,
            IsActive = true
        };

        context.PostingRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        // Add lines
        foreach (var lineDto in request.Lines)
        {
            var line = new PostingRuleLine
            {
                PostingRuleId = entity.Id,
                Sequence = lineDto.Sequence,
                AccountSource = Enum.Parse<Domain.Accounting.Enums.AccountSource>(lineDto.AccountSource),
                FixedAccountId = lineDto.FixedAccountId,
                DebitOrCredit = Enum.Parse<Domain.Accounting.Enums.DebitOrCredit>(lineDto.DebitOrCredit),
                AmountSource = Enum.Parse<Domain.Accounting.Enums.AmountSource>(lineDto.AmountSource),
                FundDimensionRequired = lineDto.FundDimensionRequired,
                CostCenterDimensionRequired = lineDto.CostCenterDimensionRequired,
                ProjectDimensionRequired = lineDto.ProjectDimensionRequired,
                IsActive = true
            };

            context.PostingRuleLines.Add(line);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreatePostingRuleCommandValidator : AbstractValidator<CreatePostingRuleCommand>
{
    public CreatePostingRuleCommandValidator()
    {
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
