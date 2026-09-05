using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-008 — GenerateYearEndClosingCommand
[Authorize(Policy = PermissionCodes.ClosingEntriesGenerate)]
public class GenerateYearEndClosingCommand : IRequest<Result<int>>
{
    public int FiscalYearId { get; init; }
    public string? Description { get; init; }
}

public class GenerateYearEndClosingCommandValidator : AbstractValidator<GenerateYearEndClosingCommand>
{
    public GenerateYearEndClosingCommandValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year ID is required.");
    }
}
