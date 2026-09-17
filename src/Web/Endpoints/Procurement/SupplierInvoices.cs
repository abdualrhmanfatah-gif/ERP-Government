using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.AcceptInvoiceWithNotes;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.CancelSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.CreateSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.MatchSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.SubmitSupplierInvoice;
using ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoiceById;
using ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoices;
using ERP_Government.Web.Infrastructure;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Web.Endpoints.Procurement;

public class SupplierInvoices : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesView)
            .Produces<object>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesView)
            .Produces<SupplierInvoiceDetailResponse>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesCreate)
            .Produces<int>();
        group.MapPatch("/{id:int}/submit", HandleSubmit)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesSubmit)
            .Produces<Result>();
        group.MapPatch("/{id:int}/match", HandleMatch)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesMatch)
            .Produces<Result>();
        group.MapPatch("/{id:int}/cancel", HandleCancel)
            .RequireAuthorization(PermissionCodes.SupplierInvoicesCancel)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? purchaseOrderId = null,
        ERP_Government.Domain.Procurement.Enums.SupplierInvoiceStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetSupplierInvoicesQuery(purchaseOrderId, status, search, page, pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetSupplierInvoiceByIdQuery(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreateSupplierInvoiceCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/SupplierInvoices/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleSubmit(ISender sender, int id)
    {
        var result = await sender.Send(new SubmitSupplierInvoiceCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleMatch(ISender sender, int id)
    {
        var result = await sender.Send(new MatchSupplierInvoiceCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCancel(ISender sender, int id, CancelSupplierInvoiceCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}
