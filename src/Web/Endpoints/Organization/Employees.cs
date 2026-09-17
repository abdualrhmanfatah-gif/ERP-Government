using ERP_Government.Application.Organization.Common.DTOs;
using ERP_Government.Application.Organization.Commands.Employees;
using ERP_Government.Application.Organization.Queries.Employees;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Organization;

public class Employees : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetEmployees)
            .Produces<List<EmployeeDto>>()
            .RequireAuthorization(PermissionCodes.EmployeesView);

        groupBuilder.MapGet("/{id:int}", GetEmployeeById)
            .Produces<EmployeeDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.EmployeesView);

        groupBuilder.MapPost("/", CreateEmployee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EmployeesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateEmployee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EmployeesUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteEmployee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EmployeesDelete);
    }

    [EndpointSummary("Get all employees")]
    public static async Task<List<EmployeeDto>> GetEmployees(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetEmployeesQuery());
    }

    [EndpointSummary("Get employee by ID")]
    public static async Task<IResult> GetEmployeeById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetEmployeeByIdQuery { Id = id });
        return result.ToProblemDetails();
    }

    [EndpointSummary("Create a new employee")]
    public static async Task<IResult> CreateEmployee(
        [FromServices] ISender sender,
        [FromBody] CreateEmployeeCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update an employee")]
    public static async Task<IResult> UpdateEmployee(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateEmployeeRequest body)
    {
        var result = await sender.Send(new UpdateEmployeeCommand
        {
            Id = id,
            EmployeeNumber = body.EmployeeNumber,
            Name = body.Name,
            OrganizationalUnitId = body.OrganizationalUnitId,
            JobTitle = body.JobTitle,
            JobGrade = body.JobGrade,
            HireDate = body.HireDate,
            EmploymentStatus = body.EmploymentStatus,
            IsActive = body.IsActive
        });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Delete an employee")]
    public static async Task<IResult> DeleteEmployee(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteEmployeeCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}

public record UpdateEmployeeRequest(
    string EmployeeNumber,
    string Name,
    int OrganizationalUnitId,
    string JobTitle,
    string? JobGrade,
    DateOnly HireDate,
    string EmploymentStatus,
    bool IsActive);
