using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.ConfirmGRN;

[Authorize(Policy = PermissionCodes.GoodsReceiptsConfirm)]
public record ConfirmGRNCommand(int Id) : IRequest<Result>;
