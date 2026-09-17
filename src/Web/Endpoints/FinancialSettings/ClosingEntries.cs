using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;
using ERP_Government.Application.FinancialSettings.Queries.ClosingEntries;
using ERP_Government.Application.Common.Security;
using MediatR;
using ERP_Government.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

// T-017-014 — ClosingEntries Minimal API Endpoints
public class ClosingEntries : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/by-fiscal-year/{fiscalYearId:int}", GetClosingEntriesByFiscalYear)
            .Produces<List<ClosingEntryDto>>()
            .RequireAuthorization(PermissionCodes.ClosingEntriesView);

        groupBuilder.MapGet("/{id:int}", GetClosingEntryById)
            .Produces<ClosingEntryDto?>()
            .RequireAuthorization(PermissionCodes.ClosingEntriesView);

        groupBuilder.MapPost("/generate", GenerateClosingEntry)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ClosingEntriesGenerate);

        groupBuilder.MapPost("/{id:int}/approve", ApproveClosingEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ClosingEntriesApprove);

        groupBuilder.MapPost("/{id:int}/reverse", ReverseClosingEntry)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ClosingEntriesReverse);
    }

    [EndpointSummary("Get closing entries by fiscal year")]
    public static async Task<List<ClosingEntryDto>> GetClosingEntriesByFiscalYear(
        [FromServices] ISender sender,
        int fiscalYearId)
    {
        return await sender.Send(new GetClosingEntriesByFiscalYearQuery { FiscalYearId = fiscalYearId });
    }

    [EndpointSummary("Get closing entry by ID")]
    public static async Task<IResult> GetClosingEntryById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetClosingEntryByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Generate year-end closing entry")]
    public static async Task<IResult> GenerateClosingEntry(
        [FromServices] ISender sender,
        [FromBody] GenerateYearEndClosingCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Approve closing entry")]
    public static async Task<IResult> ApproveClosingEntry(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new ApproveClosingEntryCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Reverse closing entry")]
    public static async Task<IResult> ReverseClosingEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] ReverseClosingEntryCommand command)
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
        return Results.Ok(result.Value);
    }
}
