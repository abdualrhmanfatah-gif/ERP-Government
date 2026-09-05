using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-009 — ApproveClosingEntryCommand
[Authorize(Policy = PermissionCodes.ClosingEntriesApprove)]
public class ApproveClosingEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ApproveClosingEntryCommandValidator : AbstractValidator<ApproveClosingEntryCommand>
{
    public ApproveClosingEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Closing entry ID is required.");
    }
}
