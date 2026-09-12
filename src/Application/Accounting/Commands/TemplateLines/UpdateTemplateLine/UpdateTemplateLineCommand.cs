using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Commands.TemplateLines.UpdateTemplateLine;

[Authorize(Policy = PermissionCodes.TemplatesUpdate)]
public class UpdateTemplateLineCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; } = 1;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateTemplateLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateTemplateLineCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTemplateLineCommand request,
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

        var account = await context.Accounts
            .FindAsync(request.AccountId, cancellationToken);

        if (account is null)
            return Result.Failure(["Account not found."]);

        if (!account.IsPostable)
            return Result.Failure(["Account is not postable."]);

        if (!account.IsActive)
            return Result.Failure(["Account is not active."]);

        if (request.Debit > 0 && request.Credit > 0)
            return Result.Failure(["A line cannot have both debit and credit amounts."]);

        if (request.Debit == 0 && request.Credit == 0)
            return Result.Failure(["A line must have either a debit or credit amount."]);

        if (request.Debit < 0 || request.Credit < 0)
            return Result.Failure(["Amounts cannot be negative."]);

        entity.AccountId = request.AccountId;
        entity.Description = request.Description;
        entity.CurrencyId = request.CurrencyId;
        entity.ExchangeRate = request.ExchangeRate;
        entity.Debit = request.Debit;
        entity.Credit = request.Credit;
        entity.CostCenterId = request.CostCenterId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateTemplateLineCommandValidator : AbstractValidator<UpdateTemplateLineCommand>
{
    public UpdateTemplateLineCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid template line ID.");

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

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
