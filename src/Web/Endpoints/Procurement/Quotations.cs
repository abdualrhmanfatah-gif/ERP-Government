using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Procurement.Commands.Quotations.AwardQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.CompleteEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.RejectQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.SelectQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.StartEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.SubmitQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.UpdateQuotation;
using ERP_Government.Application.Procurement.Queries.Quotations.GetQuotationById;
using ERP_Government.Application.Procurement.Queries.Quotations.GetQuotations;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Web.Infrastructure;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Web.Endpoints.Procurement;

public class Quotations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.QuotationsView)
            .Produces<IReadOnlyList<QuotationListItem>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.QuotationsView)
            .Produces<QuotationDetailResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.QuotationsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.QuotationsCreate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/submit", HandleSubmit)
            .RequireAuthorization(PermissionCodes.QuotationsCreate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/start-evaluation", HandleStartEvaluation)
            .RequireAuthorization(PermissionCodes.QuotationsEvaluate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/complete-evaluation", HandleCompleteEvaluation)
            .RequireAuthorization(PermissionCodes.QuotationsEvaluate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/select", HandleSelect)
            .RequireAuthorization(PermissionCodes.QuotationsSelect)
            .Produces<Result>();
        group.MapPatch("/{id:int}/award", HandleAward)
            .RequireAuthorization(PermissionCodes.QuotationsAward)
            .Produces<Result>();
        group.MapPatch("/{id:int}/reject", HandleReject)
            .RequireAuthorization(PermissionCodes.QuotationsReject)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? supplierPartyId = null,
        QuotationStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetQuotationsQuery(supplierPartyId, status, search, page, pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetQuotationByIdQuery(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreateQuotationCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/Quotations/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(ISender sender, int id, UpdateQuotationCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleSubmit(ISender sender, int id)
    {
        var result = await sender.Send(new SubmitQuotationCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleStartEvaluation(ISender sender, int id)
    {
        var result = await sender.Send(new StartEvaluationCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCompleteEvaluation(ISender sender, int id, CompleteEvaluationRequest request)
    {
        var result = await sender.Send(new CompleteEvaluationCommand(id, request.TechnicalScore, request.FinancialScore, request.RejectionReason));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleSelect(ISender sender, int id, SelectQuotationRequest request)
    {
        var result = await sender.Send(new SelectQuotationCommand(id, request.SelectionReason));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleAward(ISender sender, int id)
    {
        var result = await sender.Send(new AwardQuotationCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleReject(ISender sender, int id, RejectQuotationRequest request)
    {
        var result = await sender.Send(new RejectQuotationCommand(id, request.RejectionReason));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}

public record CompleteEvaluationRequest(decimal? TechnicalScore, decimal? FinancialScore, string? RejectionReason);
public record SelectQuotationRequest(string SelectionReason);
public record RejectQuotationRequest(string RejectionReason);
