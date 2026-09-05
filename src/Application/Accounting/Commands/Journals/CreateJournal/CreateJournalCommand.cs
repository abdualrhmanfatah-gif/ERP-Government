using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Journals.CreateJournal;

[Authorize(Policy = PermissionCodes.JournalsCreate)]
public class CreateJournalCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public JournalType Type { get; init; }
    public int? AccountId { get; init; }
    public int? SuspenseAccountId { get; init; }
    public bool AllowForeignCurrency { get; init; }
    public int? SequenceId { get; init; }
    public bool RequireApprovalBeforePosting { get; init; }
}

public class CreateJournalCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateJournalCommand, Result>
{
    public async Task<Result> Handle(
        CreateJournalCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Journals
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Journal code already exists."]);

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

        var entity = new Journal
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            AccountId = request.AccountId,
            SuspenseAccountId = request.SuspenseAccountId,
            AllowForeignCurrency = request.AllowForeignCurrency,
            SequenceId = request.SequenceId,
            RequireApprovalBeforePosting = request.RequireApprovalBeforePosting,
            IsActive = true
        };

        context.Journals.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateJournalCommandValidator : AbstractValidator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid journal type.");
    }
}
