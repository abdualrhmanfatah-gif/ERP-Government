using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Commands.PostingRules.DeletePostingRule;

[Authorize(Policy = PermissionCodes.PostingRulesDelete)]
public class DeletePostingRuleCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeletePostingRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeletePostingRuleCommand, Result>
{
    public async Task<Result> Handle(
        DeletePostingRuleCommand request,
        CancellationToken cancellationToken)
    {
        var postingRule = await context.PostingRules
            .FindAsync(request.Id, cancellationToken);

        if (postingRule is null)
            return Result.Failure(["Posting rule not found."]);

        // Block deletion if pending or processing events exist
        var hasPendingEvents = await context.AccountingEvents
            .AnyAsync(e =>
                e.EventType.ToString() == postingRule.EventType &&
                (e.Status == EventStatus.Pending || e.Status == EventStatus.Posted),
                cancellationToken);

        if (hasPendingEvents)
            return Result.Failure(["Cannot delete PostingRule with pending AccountingEvents. Process or reset events first."]);

        context.PostingRules.Remove(postingRule);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeletePostingRuleCommandValidator : AbstractValidator<DeletePostingRuleCommand>
{
    public DeletePostingRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Posting rule ID is required.");
    }
}
