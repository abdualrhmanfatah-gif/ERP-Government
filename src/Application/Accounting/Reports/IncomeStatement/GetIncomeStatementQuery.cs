using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Reports.IncomeStatement;

[Authorize(Policy = PermissionCodes.ViewIncomeStatement)]
public class GetIncomeStatementQuery : IRequest<IncomeStatementDto>
{
    public string? StartDate { get; init; }

    public string? EndDate { get; init; }
}
