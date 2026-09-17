using Microsoft.AspNetCore.Mvc;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.IssueFinalAccount;
using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetFinalAccount;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.FinancialControl;

public class FinalAccounts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/Generate", HandleGenerate)
            .RequireAuthorization(PermissionCodes.FinancialControlApproveFinalAccount)
            .Produces<GenerateFinalAccountResponse>();
        group.MapPost("/{id:int}/Issue", HandleIssue)
            .RequireAuthorization(PermissionCodes.FinancialControlApproveFinalAccount)
            .Produces<Result>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.FinancialControlApproveFinalAccount)
            .Produces<FinalAccountResponse?>();
    }

    private static async Task<IResult> HandleGenerate(
        ISender sender,
        GenerateFinalAccountRequest request)
    {
        var result = await sender.Send(new GenerateFinalAccountCommand(request.FiscalYearId));
        return result.Succeeded && result.Value is not null
            ? Results.Ok(result.Value.ToResponse())
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleIssue(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new IssueFinalAccountCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetFinalAccountQuery(id));
        if (result is null) return Results.Problem(
                detail: "Final account not found",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                type: "about:blank");

        return Results.Ok(new FinalAccountResponse(
            result.Id,
            result.FiscalYearId,
            result.Status,
            result.GeneratedAt,
            result.IssuedAt,
            result.Lines.Select(l => new FinalAccountLineResponse(
                l.Dimension,
                l.DimensionId,
                l.DimensionCode,
                l.DimensionName,
                l.OriginalBudgetAmount,
                l.ActualAmount,
                l.VarianceAmount)).ToList()));
    }
}

public record GenerateFinalAccountRequest(int FiscalYearId);

public record GenerateFinalAccountResponse(
    int FinalAccountId,
    int FiscalYearId,
    string FiscalYearName,
    FinalAccountStatus Status,
    int LineCount);

public record FinalAccountResponse(
    int Id,
    int FiscalYearId,
    FinalAccountStatus Status,
    DateTimeOffset GeneratedAt,
    DateTimeOffset? IssuedAt,
    List<FinalAccountLineResponse> Lines);

public record FinalAccountLineResponse(
    FinalAccountLineDimension Dimension,
    int? DimensionId,
    string DimensionCode,
    string DimensionName,
    decimal OriginalBudgetAmount,
    decimal ActualAmount,
    decimal VarianceAmount);

internal static class FinalAccountMappingExtensions
{
    public static GenerateFinalAccountResponse ToResponse(this GenerateFinalAccountResult result) => new(
        result.FinalAccountId,
        result.FiscalYearId,
        result.FiscalYearName,
        FinalAccountStatus.Draft,
        result.LineCount);
}
