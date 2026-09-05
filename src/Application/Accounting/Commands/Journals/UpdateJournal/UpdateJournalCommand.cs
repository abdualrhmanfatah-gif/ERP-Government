using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Journals.UpdateJournal;

[Authorize(Policy = PermissionCodes.JournalsEdit)]
public class UpdateJournalCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public JournalType Type { get; init; }
    public int? AccountId { get; init; }
    public int? SuspenseAccountId { get; init; }
    public bool AllowForeignCurrency { get; init; }
    public bool RequireApprovalBeforePosting { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateJournalCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateJournalCommand, Result>
{
    public async Task<Result> Handle(
        UpdateJournalCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Journals
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal not found."]);

        if (request.AccountId.HasValue)
        {
            var account = await context.Accounts
                .FindAsync(request.AccountId.Value, cancellationToken);

            if (account is null)
                return Result.Failure(["Account not found."]);
        }

        if (request.SuspenseAccountId.HasValue)
        {
            var suspenseAccount = await context.Accounts
                .FindAsync(request.SuspenseAccountId.Value, cancellationToken);

            if (suspenseAccount is null)
                return Result.Failure(["Suspense account not found."]);
        }

        entity.Name = request.Name;
        entity.Type = request.Type;
        entity.AccountId = request.AccountId;
        entity.SuspenseAccountId = request.SuspenseAccountId;
        entity.AllowForeignCurrency = request.AllowForeignCurrency;
        entity.RequireApprovalBeforePosting = request.RequireApprovalBeforePosting;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateJournalCommandValidator : AbstractValidator<UpdateJournalCommand>
{
    public UpdateJournalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid journal type.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
