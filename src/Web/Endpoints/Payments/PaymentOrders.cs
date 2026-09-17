using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Application.Payments.Commands.PaymentOrders.CreatePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.UpdatePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.SubmitPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.ApprovePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.RejectPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.CancelPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.SendToTreasury;
using ERP_Government.Application.Payments.Commands.PaymentOrders.VoidPaymentOrder;
using ERP_Government.Application.Payments.Queries.GetPaymentOrderTotals;
using ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderById;
using ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrders;
using ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderPrint;
using ERP_Government.Application.Common.Security;
using ERP_Government.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Payments;

public class PaymentOrders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetPaymentOrders)
            .Produces<List<PaymentOrderDto>>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapGet("/{id:int}", GetPaymentOrderById)
            .Produces<PaymentOrderDto?>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapGet("/{id:int}/totals", GetPaymentOrderTotals)
            .Produces<PaymentOrderTotalsDto?>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapGet("/{id:int}/export-pdf", ExportPaymentOrderPdf)
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapPost("/", CreatePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersCreate);

        groupBuilder.MapPost("/{id:int}/submit", SubmitPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersSubmit);

        groupBuilder.MapPost("/{id:int}/approve", ApprovePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersApprove);

        groupBuilder.MapPost("/{id:int}/reject", RejectPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersReject);

        groupBuilder.MapPost("/{id:int}/cancel", CancelPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersCancel);

        groupBuilder.MapPost("/{id:int}/send-to-treasury", SendToTreasury)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersSendToTreasury);

        groupBuilder.MapPost("/{id:int}/void", VoidPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersVoid);

        groupBuilder.MapPut("/{id:int}", UpdatePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersUpdate);
    }

    [EndpointSummary("Get all payment orders")]
    public static async Task<List<PaymentOrderDto>> GetPaymentOrders(
        [FromServices] ISender sender,
        [AsParameters] GetPaymentOrdersQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get payment order by ID")]
    public static async Task<IResult> GetPaymentOrderById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetPaymentOrderByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Get computed totals for a payment order")]
    public static async Task<IResult> GetPaymentOrderTotals(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetPaymentOrderTotalsQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Export payment order as PDF")]
    public static async Task<IResult> ExportPaymentOrderPdf(
        [FromServices] ISender sender,
        [FromServices] PaymentOrderPdfExporter exporter,
        int id)
    {
        var dto = await sender.Send(new GetPaymentOrderPrintQuery { Id = id });
        if (!dto.Succeeded)
            return dto.ToProblemDetails();

        var stream = new MemoryStream();
        await exporter.ExportAsync(dto.Value!, stream);
        stream.Position = 0;

        return Results.File(stream, "application/pdf",
            $"PaymentOrder-{dto.Value!.OrderNumber}.pdf");
    }

    [EndpointSummary("Create a new payment order")]
    public static async Task<IResult> CreatePaymentOrder(
        [FromServices] ISender sender,
        [FromBody] CreatePaymentOrderCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a draft payment order (header, lines and deductions are replaced)")]
    public static async Task<IResult> UpdatePaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdatePaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Submit payment order for approval")]
    public static async Task<IResult> SubmitPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] SubmitPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Approve a submitted payment order")]
    public static async Task<IResult> ApprovePaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApprovePaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Reject a submitted payment order")]
    public static async Task<IResult> RejectPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] RejectPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a payment order")]
    public static async Task<IResult> CancelPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Send payment order to treasury")]
    public static async Task<IResult> SendToTreasury(
        [FromServices] ISender sender,
        int id,
        [FromBody] SendToTreasuryCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Void a payment order")]
    public static async Task<IResult> VoidPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] VoidPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
