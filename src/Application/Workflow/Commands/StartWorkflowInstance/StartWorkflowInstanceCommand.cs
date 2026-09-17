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

namespace ERP_Government.Application.Workflow.Commands.StartWorkflowInstance;

[Authorize(Policy = PermissionCodes.WorkflowInstancesExecute)]
public class StartWorkflowInstanceCommand : IRequest<Result<int>>
{
    public int DefinitionId { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public int EntityId { get; init; }
}

public class StartWorkflowInstanceCommandValidator : AbstractValidator<StartWorkflowInstanceCommand>
{
    public StartWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.DefinitionId)
            .GreaterThan(0).WithMessage("Definition ID must be greater than 0.");

        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required.")
            .MaximumLength(50).WithMessage("Entity name must not exceed 50 characters.");

        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Entity ID must be greater than 0.");
    }
}

public class StartWorkflowInstanceCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<StartWorkflowInstanceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        StartWorkflowInstanceCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

        // Get active definition
        var definition = await context.WorkflowDefinitions
            .Include(d => d.Steps.Where(s => s.IsActive))
            .FirstOrDefaultAsync(d => d.Id == request.DefinitionId && d.IsActive, cancellationToken);

        if (definition == null)
            return Result<int>.Failure(ErrorCodes.Workflow.DefinitionNotFound, ErrorCategory.NotFound, $"Active workflow definition with ID {request.DefinitionId} not found.");

        // Check for existing active instance for this entity
        var existingInstance = await context.WorkflowInstances
            .AnyAsync(i => i.DefinitionId == request.DefinitionId
                && i.EntityName == request.EntityName
                && i.EntityId == request.EntityId
                && i.Status != WorkflowInstanceStatus.Completed.ToString()
                && i.Status != WorkflowInstanceStatus.Cancelled.ToString()
                && i.Status != WorkflowInstanceStatus.Failed.ToString(),
                cancellationToken);

        if (existingInstance)
            return Result<int>.Failure(ErrorCodes.Workflow.ActiveInstanceExists, ErrorCategory.Conflict, $"An active workflow instance already exists for {request.EntityName} with ID {request.EntityId}.");

        // Get first step
        var firstStep = definition.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (firstStep == null)
            return Result<int>.Failure(ErrorCodes.Workflow.NoActiveSteps, ErrorCategory.BusinessRule, "Workflow definition has no active steps.");

        var instance = new Domain.Workflow.Entities.WorkflowInstance
        {
            DefinitionId = request.DefinitionId,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            CurrentStepId = firstStep.Id,
            Status = WorkflowInstanceStatus.InProgress.ToString(),
            StartedAt = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.WorkflowInstances.Add(instance);
        await context.SaveChangesAsync(cancellationToken);

        // Create history entry
        context.WorkflowHistory.Add(new Domain.Workflow.Entities.WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = firstStep.Id,
            ActorUserId = userId,
            Decision = WorkflowDecision.Started.ToString(),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowInstanceId = instance.Id,
            DefinitionId = request.DefinitionId,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            FirstStepId = firstStep.Id
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "StartInstance",
            UserId = userId,
            EntityName = "WorkflowInstance",
            EntityId = instance.Id,
            Success = true,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(instance.Id);
    }
}