using ERP_Government.Application.Organization.Common.DTOs;
using ERP_Government.Application.Organization.Commands.Projects;
using ERP_Government.Application.Organization.Queries.Projects;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Organization;

public class Projects : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetProjects)
            .Produces<List<ProjectDto>>()
            .RequireAuthorization(PermissionCodes.ProjectsView);

        groupBuilder.MapGet("/{id:int}", GetProjectById)
            .Produces<ProjectDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.ProjectsView);

        groupBuilder.MapPost("/", CreateProject)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ProjectsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateProject)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ProjectsUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteProject)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ProjectsDelete);
    }

    [EndpointSummary("Get all projects")]
    public static async Task<List<ProjectDto>> GetProjects(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetProjectsQuery());
    }

    [EndpointSummary("Get project by ID")]
    public static async Task<IResult> GetProjectById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetProjectByIdQuery { Id = id });
        return result.ToProblemDetails();
    }

    [EndpointSummary("Create a new project")]
    public static async Task<IResult> CreateProject(
        [FromServices] ISender sender,
        [FromBody] CreateProjectCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a project")]
    public static async Task<IResult> UpdateProject(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateProjectRequest body)
    {
        var result = await sender.Send(new UpdateProjectCommand
        {
            Id = id,
            Code = body.Code,
            Name = body.Name,
            FundId = body.FundId,
            CostCenterId = body.CostCenterId,
            StartDate = body.StartDate,
            EndDate = body.EndDate,
            BudgetAmount = body.BudgetAmount,
            Status = body.Status,
            IsActive = body.IsActive
        });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Delete a project")]
    public static async Task<IResult> DeleteProject(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteProjectCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}

public record UpdateProjectRequest(
    string Code,
    string Name,
    int? FundId,
    int? CostCenterId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? BudgetAmount,
    string Status,
    bool IsActive);