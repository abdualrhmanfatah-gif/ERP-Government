using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Reports.BalanceSheet;

[Authorize(Policy = PermissionCodes.ViewBalanceSheet)]
public class GetBalanceSheetQuery : IRequest<BalanceSheetDto>
{
    public string? AsOfDate { get; init; }

    public int? FiscalPeriodId { get; init; }
}
