using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.SubmitReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchers;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchersByParty;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchersByPeriod;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Revenue;

public class ReceiptVouchers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetReceiptVouchers)
            .Produces<List<ReceiptVoucherDto>>()
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView);

        groupBuilder.MapGet("/{id:int}", GetReceiptVoucherById)
            .Produces<ReceiptVoucherDto?>()
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView);

        groupBuilder.MapGet("/by-party/{partyId:int}", GetReceiptVouchersByParty)
            .Produces<List<ReceiptVoucherDto>>()
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView);

        groupBuilder.MapGet("/by-period", GetReceiptVouchersByPeriod)
            .Produces<List<ReceiptVoucherDto>>()
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView);

        groupBuilder.MapPost("/", CreateReceiptVoucher)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersCreate);

        groupBuilder.MapPost("/{id:int}/submit", SubmitReceiptVoucher)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersSubmit);

        groupBuilder.MapPost("/{id:int}/approve", ApproveReceiptVoucher)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersApprove);

        groupBuilder.MapPost("/{id:int}/cancel", CancelReceiptVoucher)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersCancel);
    }

    [EndpointSummary("Get all receipt vouchers")]
    public static async Task<IResult> GetReceiptVouchers(
        [FromServices] ISender sender,
        [AsParameters] GetReceiptVouchersQuery query)
    {
        var result = await sender.Send(query);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get receipt voucher by ID")]
    public static async Task<IResult> GetReceiptVoucherById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetReceiptVoucherByIdQuery { Id = id });
        if (!result.Succeeded)
            return Results.NotFound();
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get receipt vouchers by party")]
    public static async Task<IResult> GetReceiptVouchersByParty(
        [FromServices] ISender sender,
        int partyId,
        [AsParameters] GetReceiptVouchersByPartyQuery query)
    {
        var result = await sender.Send(new GetReceiptVouchersByPartyQuery
        {
            PartyId = partyId,
            Page = query.Page,
            PageSize = query.PageSize
        });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get receipt vouchers by date period")]
    public static async Task<IResult> GetReceiptVouchersByPeriod(
        [FromServices] ISender sender,
        [AsParameters] GetReceiptVouchersByPeriodQuery query)
    {
        var result = await sender.Send(query);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Create a new receipt voucher")]
    public static async Task<IResult> CreateReceiptVoucher(
        [FromServices] ISender sender,
        [FromBody] CreateReceiptVoucherCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/ReceiptVouchers/{result.Value}", result.Value);
    }

    [EndpointSummary("Submit receipt voucher for review")]
    public static async Task<IResult> SubmitReceiptVoucher(
        [FromServices] ISender sender,
        int id,
        [FromBody] SubmitReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve receipt voucher (reviewer gate)")]
    public static async Task<IResult> ApproveReceiptVoucher(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApproveReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel receipt voucher")]
    public static async Task<IResult> CancelReceiptVoucher(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
