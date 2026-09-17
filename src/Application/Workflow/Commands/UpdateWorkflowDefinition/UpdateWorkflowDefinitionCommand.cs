using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Commands.UpdateWorkflowDefinition;

[Authorize(Policy = PermissionCodes.WorkflowDefinitionsManage)]
public class UpdateWorkflowDefinitionCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Description { get; init; }
    public WorkflowDefinitionStatus? Status { get; init; }
}

public class UpdateWorkflowDefinitionCommandValidator : AbstractValidator<UpdateWorkflowDefinitionCommand>
{
    public UpdateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Definition ID must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public class UpdateWorkflowDefinitionCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<UpdateWorkflowDefinitionCommand, Result>
{
    public async Task<Result> Handle(
        UpdateWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

        var definition = await context.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (definition == null)
            return Result.Failure(ErrorCodes.Workflow.DefinitionNotFound, ErrorCategory.NotFound, $"Workflow definition with ID {request.Id} not found.");

        var oldDescription = definition.Description;
        var oldStatus = definition.Status;

        if (request.Description != null)
            definition.Description = request.Description;

        if (request.Status.HasValue)
            definition.Status = request.Status.Value.ToString();

        definition.LastModified = DateTimeOffset.UtcNow;
        definition.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowDefinitionId = definition.Id,
            OldDescription = oldDescription,
            NewDescription = definition.Description,
            OldStatus = oldStatus,
            NewStatus = definition.Status
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "UpdateDefinition",
            UserId = userId,
            EntityName = "WorkflowDefinition",
            EntityId = definition.Id,
            Success = true,
            OldValues = JsonSerializer.Serialize(new { oldDescription, oldStatus }),
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}