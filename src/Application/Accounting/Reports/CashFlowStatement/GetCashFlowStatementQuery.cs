using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Reports.CashFlowStatement;

[Authorize(Policy = PermissionCodes.ViewCashFlow)]
public class GetCashFlowStatementQuery : IRequest<CashFlowStatementDto>
{
    public string? StartDate { get; init; }

    public string? EndDate { get; init; }
}
