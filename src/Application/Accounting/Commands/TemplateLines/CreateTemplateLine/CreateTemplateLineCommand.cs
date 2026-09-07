using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Commands.TemplateLines.CreateTemplateLine;

[Authorize(Policy = PermissionCodes.TemplatesUpdate)]
public class CreateTemplateLineCommand : IRequest<Result<int>>
{
    public int TemplateId { get; init; }
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; } = 1;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
}

public class CreateTemplateLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateTemplateLineCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateTemplateLineCommand request,
        CancellationToken cancellationToken)
    {
        var template = await context.JournalEntryTemplates
            .FindAsync(request.TemplateId, cancellationToken);

        if (template is null)
            return Result<int>.Failure(["Template not found."]);

        var account = await context.Accounts
            .FindAsync(request.AccountId, cancellationToken);

        if (account is null)
            return Result<int>.Failure(["Account not found."]);

        if (!account.IsPostable)
            return Result<int>.Failure(["Account is not postable."]);

        if (!account.IsActive)
            return Result<int>.Failure(["Account is not active."]);

        if (request.Debit > 0 && request.Credit > 0)
            return Result<int>.Failure(["A line cannot have both debit and credit amounts."]);

        if (request.Debit == 0 && request.Credit == 0)
            return Result<int>.Failure(["A line must have either a debit or credit amount."]);

        if (request.Debit < 0 || request.Credit < 0)
            return Result<int>.Failure(["Amounts cannot be negative."]);

        var maxSequence = await context.JournalEntryTemplateLines
            .Where(x => x.TemplateId == request.TemplateId)
            .MaxAsync(x => (int?)x.Sequence, cancellationToken) ?? 0;

        var entity = new JournalEntryTemplateLine
        {
            TemplateId = request.TemplateId,
            Sequence = maxSequence + 1,
            AccountId = request.AccountId,
            Description = request.Description,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            Debit = request.Debit,
            Credit = request.Credit,
            CostCenterId = request.CostCenterId
        };

        context.JournalEntryTemplateLines.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateTemplateLineCommandValidator : AbstractValidator<CreateTemplateLineCommand>
{
    public CreateTemplateLineCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .GreaterThan(0).WithMessage("Template is required.");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Debit cannot be negative.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit cannot be negative.");
    }
}
