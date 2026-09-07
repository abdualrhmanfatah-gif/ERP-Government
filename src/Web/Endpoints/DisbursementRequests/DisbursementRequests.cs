using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CancelDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.RejectDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;
using ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequestById;
using ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequests;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.DisbursementRequests;

public class DisbursementRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsView)
            .Produces<IReadOnlyList<Application.Payments.Common.DTOs.DisbursementRequestDto>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsView)
            .Produces<Application.Payments.Common.DTOs.DisbursementRequestDetailDto?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsCreate)
            .Produces<Application.Payments.Common.DTOs.DisbursementRequestDto>();
        group.MapPatch("/{id:int}/submit", HandleSubmit)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsSubmit)
            .Produces<Result>();
        group.MapPatch("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsApprove)
            .Produces<Result>();
        group.MapPatch("/{id:int}/reject", HandleReject)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsReject)
            .Produces<Result>();
        group.MapPatch("/{id:int}/cancel", HandleCancel)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsCancel)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        DisbursementRequestStatus? status = null,
        int? fundId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        int? paymentOrderId = null)
    {
        var result = await sender.Send(new GetDisbursementRequestsQuery(status, fundId, fromDate, toDate, paymentOrderId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetDisbursementRequestByIdQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateDisbursementRequestRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/DisbursementRequests/{result.Value!.Id}", result.Value)
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleSubmit(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new SubmitDisbursementRequestCommand { Id = id });
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleApprove(
        ISender sender,
        int id,
        ApproveDisbursementRequestRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleReject(
        ISender sender,
        int id,
        RejectDisbursementRequestRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleCancel(
        ISender sender,
        int id,
        CancelDisbursementRequestRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
    }
}

public record CreateDisbursementRequestRequest(
    int PaymentOrderId,
    string? Notes)
{
    public CreateDisbursementRequestCommand ToCommand() => new()
    {
        PaymentOrderId = PaymentOrderId,
        Notes = Notes
    };
}

public record ApproveDisbursementRequestRequest(string? Reason)
{
    public ApproveDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        Reason = Reason
    };
}

public record RejectDisbursementRequestRequest(string Reason)
{
    public RejectDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        Reason = Reason
    };
}

public record CancelDisbursementRequestRequest(string Reason)
{
    public CancelDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        Reason = Reason
    };
}
