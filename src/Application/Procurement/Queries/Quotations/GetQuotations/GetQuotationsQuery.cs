using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.Quotations.GetQuotations;

[Authorize(Policy = PermissionCodes.QuotationsView)]
public record GetQuotationsQuery(
    int? SupplierPartyId = null,
    QuotationStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<QuotationListItem>>>;

public record QuotationListItem(
    int Id,
    string QuotationNumber,
    int SupplierPartyId,
    DateTime QuotationDate,
    QuotationStatus Status,
    decimal? GrandTotal,
    DateTimeOffset Created);
