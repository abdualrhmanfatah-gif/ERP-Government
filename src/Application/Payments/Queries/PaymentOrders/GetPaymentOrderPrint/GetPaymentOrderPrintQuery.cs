using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;
using MediatR;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderPrint;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public record GetPaymentOrderPrintQuery : IRequest<PaymentOrderPrintDto?>
{
    public int Id { get; init; }
}
