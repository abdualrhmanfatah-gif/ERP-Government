using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-011 — ApproveClosingEntryCommandHandler
// Transitions Draft → Approved and records approver identity
public class ApproveClosingEntryCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveClosingEntryCommand, Result>
{
    public async Task<Result> Handle(
        ApproveClosingEntryCommand request,
        CancellationToken cancellationToken)
    {
        var entry = await context.YearEndClosingEntries
            .FindAsync(request.Id, cancellationToken);

        if (entry is null)
            return Result.Failure(["Closing entry not found."]);

        if (entry.Status != ClosingEntryStatus.Draft)
            return Result.Failure(["Only draft closing entries can be approved."]);

        entry.Status = ClosingEntryStatus.Approved;
        entry.ApprovedById = user.Id?.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
