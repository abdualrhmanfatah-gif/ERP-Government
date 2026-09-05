using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Parties.Commands.CreateParty;
using ERP_Government.Application.Parties.Commands.TogglePartyActive;
using ERP_Government.Application.Parties.Commands.UpdateParty;
using ERP_Government.Application.Parties.Queries.GetParties;
using ERP_Government.Application.Parties.Queries.GetPartyById;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Parties;

public class Parties : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .Produces<IReadOnlyList<PartyResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .Produces<PartyResponse?>();
        group.MapPost("/", HandleCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        PartyType? partyType = null,
        bool? isActive = null,
        string? search = null)
    {
        var result = await sender.Send(new GetPartiesQuery(partyType, isActive, search));
        return Results.Ok(result.Select(p => p.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetPartyByIdQuery(id));
        return result is not null ? Results.Ok(result.ToResponse()) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreatePartyRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded ? Results.Created($"/api/Parties/{result.Value}", result.Value) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdatePartyRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new TogglePartyActiveCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}

public record PartyResponse(
    int Id,
    string PartyCode,
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive);

public record CreatePartyRequest(
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes)
{
    public CreatePartyCommand ToCommand() => new(
        PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes);
}

public record UpdatePartyRequest(
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes)
{
    public UpdatePartyCommand ToCommand(int id) => new(
        id, PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes);
}

internal static class PartyMappingExtensions
{
    public static PartyResponse ToResponse(this ERP_Government.Domain.Parties.Entities.Party p) => new(
        p.Id,
        p.PartyCode,
        p.PartyType,
        p.NameAr,
        p.NameEn,
        p.TaxNumber,
        p.NationalId,
        p.Phone,
        p.Email,
        p.Address,
        p.Notes,
        p.IsActive);
}
