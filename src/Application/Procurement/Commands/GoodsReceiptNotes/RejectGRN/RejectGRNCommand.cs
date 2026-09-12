using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.RejectGRN;

[Authorize(Policy = PermissionCodes.GoodsReceiptsReject)]
public record RejectGRNCommand(int Id, string? Notes) : IRequest<Result>;
