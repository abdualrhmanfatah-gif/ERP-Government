using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Reports.GeneralLedger;

[Authorize(Policy = PermissionCodes.ViewGeneralLedger)]
public class GetGeneralLedgerQuery : IRequest<GeneralLedgerDto>
{
    public int? AccountId { get; init; }

    public string? AccountCode { get; init; }

    public int? FiscalPeriodId { get; init; }

    public string? StartDate { get; init; }

    public string? EndDate { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
