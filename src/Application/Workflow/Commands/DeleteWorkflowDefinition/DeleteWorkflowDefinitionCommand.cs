using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Security.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Commands.DeleteWorkflowDefinition;

[Authorize(Policy = PermissionCodes.WorkflowDefinitionsManage)]
public class DeleteWorkflowDefinitionCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteWorkflowDefinitionCommandValidator : AbstractValidator<DeleteWorkflowDefinitionCommand>
{
    public DeleteWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Definition ID must be greater than 0.");
    }
}

public class DeleteWorkflowDefinitionCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<DeleteWorkflowDefinitionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

        var definition = await context.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (definition == null)
            return Result.Failure(ErrorCodes.Workflow.DefinitionNotFound, ErrorCategory.NotFound, $"Workflow definition with ID {request.Id} not found.");

        // Check for active instances
        var hasActiveInstances = await context.WorkflowInstances
            .AnyAsync(i => i.DefinitionId == request.Id
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Completed.ToString()
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Cancelled.ToString()
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Failed.ToString(),
                cancellationToken);

        if (hasActiveInstances)
            return Result.Failure(ErrorCodes.Workflow.ActiveInstancesExist, ErrorCategory.Conflict, "Cannot delete workflow definition with active instances.");

        // Soft delete - set IsActive = false
        definition.IsActive = false;
        definition.LastModified = DateTimeOffset.UtcNow;
        definition.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowDefinitionId = definition.Id,
            EntityName = definition.EntityName,
            Version = definition.Version
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "DeleteDefinition",
            UserId = userId,
            EntityName = "WorkflowDefinition",
            EntityId = definition.Id,
            Success = true,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}