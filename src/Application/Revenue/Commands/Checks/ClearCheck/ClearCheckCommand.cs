using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;

[Authorize(Policy = PermissionCodes.ChecksClear)]
public class ClearCheckCommand : IRequest<Result>
{
    public int Id { get; init; }
    public DateTimeOffset ClearedAt { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ClearCheckCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ClearCheckCommand, Result>
{
    public async Task<Result> Handle(
        ClearCheckCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var check = await context.Checks.FindAsync(request.Id, cancellationToken);
        if (check is null)
            return Result.Failure(new[] { "Check not found."});

        if (check.Status != CheckStatus.UnderCollection)
            return Result.Failure(new[] { "Only Under-Collection checks can be cleared."});

        check.Status = CheckStatus.Cleared;
        check.ClearedAt = request.ClearedAt;
        check.RowVersion = request.RowVersion;

        var voucher = await context.ReceiptVouchers
            .Include(v => v.Party)
            .FirstOrDefaultAsync(v => v.Id == check.ReceiptVoucherId, cancellationToken);

        check.AddDomainEvent(new Domain.Events.Revenue.CheckCleared
        {
            SourceEntityId = check.Id,
            OccurredAt = request.ClearedAt,
            BankName = check.BankName,
            CheckNumber = check.CheckNumber,
            Amount = check.Amount,
            CurrencyId = 1
        });

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(Check),
            DocumentId = check.Id,
            FromStatus = CheckStatus.UnderCollection.ToString(),
            ToStatus = CheckStatus.Cleared.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Check was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }
}

public class ClearCheckCommandValidator : AbstractValidator<ClearCheckCommand>
{
    public ClearCheckCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Check ID is required.");

        RuleFor(x => x.ClearedAt)
            .NotEmpty().WithMessage("Clearing date is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
