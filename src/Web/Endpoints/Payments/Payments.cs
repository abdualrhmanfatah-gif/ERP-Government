using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Commands.Payments.RecordPayment;
using ERP_Government.Application.Payments.Queries.Payments.GetPayments;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Payments;

public class Payments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .Produces<IReadOnlyList<Application.Payments.Common.DTOs.PaymentDto>>()
            .RequireAuthorization(PermissionCodes.PaymentsView);

        group.MapGet("/{id:int}", HandleGetById)
            .Produces<Application.Payments.Common.DTOs.PaymentDto?>()
            .RequireAuthorization(PermissionCodes.PaymentsView);

        group.MapPost("/", HandleCreate)
            .Produces<Application.Payments.Common.DTOs.PaymentDto>()
            .RequireAuthorization(PermissionCodes.PaymentsCreate);
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        PaymentStatus? status = null,
        int? fundId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        var result = await sender.Send(new GetPaymentsQuery(status, fundId, fromDate, toDate));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new Application.Payments.Queries.Payments.GetPayments.GetPaymentsQuery());
        var payment = result.FirstOrDefault(p => p.Id == id);
        return payment is not null ? Results.Ok(payment) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        RecordPaymentRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors);
    }
}

public record RecordPaymentRequest(
    int PaymentOrderId,
    PaymentMethod PaymentMethod,
    string? ReferenceNumber,
    string? Notes)
{
    public RecordPaymentCommand ToCommand() => new()
    {
        PaymentOrderId = PaymentOrderId,
        PaymentMethod = PaymentMethod,
        ReferenceNumber = ReferenceNumber,
        Notes = Notes
    };
}
