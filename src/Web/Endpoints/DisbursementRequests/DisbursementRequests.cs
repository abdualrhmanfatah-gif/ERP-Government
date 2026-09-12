using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CancelDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateAccrualEntry;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.RejectDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.UpdateDisbursementRequest;
using ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequestAccrual;
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
        group.MapPatch("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsUpdate)
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
        group.MapPost("/{id:int}/accrual-entry", HandleCreateAccrualEntry)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsCreateAccrual)
            .Produces<Result<int>>();
        group.MapGet("/{id:int}/accrual-entry", HandleGetAccrualEntry)
            .RequireAuthorization(PermissionCodes.DisbursementRequestsView)
            .Produces<AccrualEntryDto?>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        DisbursementRequestStatus? status = null,
        int? requestedById = null)
    {
        var result = await sender.Send(new GetDisbursementRequestsQuery(status, requestedById));
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

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateDisbursementRequestRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
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

    private static async Task<IResult> HandleCreateAccrualEntry(
        ISender sender,
        int id,
        CreateAccrualEntryRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleGetAccrualEntry(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetDisbursementRequestAccrualQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
}

public record CreateDisbursementRequestRequest(
    string BeneficiaryName,
    decimal RequestedAmount,
    int CurrencyId,
    string Purpose,
    int FinancialYearId,
    string? Notes)
{
    public CreateDisbursementRequestCommand ToCommand() => new()
    {
        BeneficiaryName = BeneficiaryName,
        RequestedAmount = RequestedAmount,
        CurrencyId = CurrencyId,
        Purpose = Purpose,
        FinancialYearId = FinancialYearId,
        Notes = Notes
    };
}

public record UpdateDisbursementRequestRequest(
    string? BeneficiaryName,
    decimal? RequestedAmount,
    int? CurrencyId,
    string? Purpose,
    int? FinancialYearId,
    string? Notes,
    byte[] RowVersion)
{
    public UpdateDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        BeneficiaryName = BeneficiaryName,
        RequestedAmount = RequestedAmount,
        CurrencyId = CurrencyId,
        Purpose = Purpose,
        FinancialYearId = FinancialYearId,
        Notes = Notes,
        RowVersion = RowVersion
    };
}

public record ApproveDisbursementRequestRequest(
    decimal? ApprovedAmount,
    string? Reason,
    byte[] RowVersion)
{
    public ApproveDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        ApprovedAmount = ApprovedAmount,
        Reason = Reason,
        RowVersion = RowVersion
    };
}

public record RejectDisbursementRequestRequest(string Reason, byte[] RowVersion)
{
    public RejectDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        Reason = Reason,
        RowVersion = RowVersion
    };
}

public record CancelDisbursementRequestRequest(string Reason, byte[] RowVersion)
{
    public CancelDisbursementRequestCommand ToCommand(int id) => new()
    {
        Id = id,
        Reason = Reason,
        RowVersion = RowVersion
    };
}

public record CreateAccrualEntryRequest(
    int ExpenseAccountId,
    int LiabilityAccountId,
    decimal Amount,
    int CurrencyId,
    int? CostCenterId,
    string? Narration)
{
    public CreateAccrualEntryCommand ToCommand(int id) => new()
    {
        DisbursementRequestId = id,
        ExpenseAccountId = ExpenseAccountId,
        LiabilityAccountId = LiabilityAccountId,
        Amount = Amount,
        CurrencyId = CurrencyId,
        CostCenterId = CostCenterId,
        Narration = Narration
    };
}
