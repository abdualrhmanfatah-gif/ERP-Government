using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.IssueFinalAccount;

[Authorize(Policy = PermissionCodes.FinancialControlApproveFinalAccount)]
public record IssueFinalAccountCommand(int FinalAccountId) : IRequest<Result>;

public class IssueFinalAccountCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<IssueFinalAccountCommand, Result>
{
    public async Task<Result> Handle(
        IssueFinalAccountCommand request,
        CancellationToken cancellationToken)
    {
        var finalAccount = await context.FinalAccounts.FindAsync(request.FinalAccountId, cancellationToken);
        if (finalAccount is null)
            return Result.Failure(new[] { "Final account not found." });

        if (finalAccount.Status == FinalAccountStatus.Issued)
            return Result.Failure(new[] { "Final account has already been issued." });

        finalAccount.Status = FinalAccountStatus.Issued;
        finalAccount.IssuedAt = DateTimeOffset.UtcNow;
        finalAccount.IssuedById = user.Id ?? 0;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
