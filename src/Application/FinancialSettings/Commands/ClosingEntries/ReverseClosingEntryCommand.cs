using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-029 — ReverseClosingEntryCommand
[Authorize(Policy = PermissionCodes.ClosingEntriesReverse)]
public class ReverseClosingEntryCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}

public class ReverseClosingEntryCommandValidator : AbstractValidator<ReverseClosingEntryCommand>
{
    public ReverseClosingEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Closing entry ID is required.");
    }
}
