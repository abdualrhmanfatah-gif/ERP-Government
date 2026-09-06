using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Entities;
using ERP_Government.Domain.Workflow.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Commands.AdvanceWorkflowInstance;

public class AdvanceWorkflowInstanceCommand : IRequest<Unit>
{
    public int InstanceId { get; init; }
    public int StepId { get; init; }
    public WorkflowDecision Decision { get; init; }
    public string? Reason { get; init; }
}

public class AdvanceWorkflowInstanceCommandValidator : AbstractValidator<AdvanceWorkflowInstanceCommand>
{
    public AdvanceWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .GreaterThan(0).WithMessage("Instance ID must be greater than 0.");

        RuleFor(x => x.StepId)
            .GreaterThan(0).WithMessage("Step ID must be greater than 0.");

        RuleFor(x => x.Decision)
            .IsInEnum().WithMessage("Decision must be a valid workflow decision.");
    }
}

public class AdvanceWorkflowInstanceCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<AdvanceWorkflowInstanceCommand, Unit>
{
    public async Task<Unit> Handle(
        AdvanceWorkflowInstanceCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            throw new InvalidOperationException("User identity is required for this operation.");

        var instance = await context.WorkflowInstances
            .Include(i => i.Definition)
            .ThenInclude(d => d!.Steps.Where(s => s.IsActive))
            .FirstOrDefaultAsync(i => i.Id == request.InstanceId, cancellationToken);

        if (instance?.Definition == null)
            throw new InvalidOperationException($"Workflow instance with ID {request.InstanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress.ToString())
            throw new InvalidOperationException($"Workflow instance is not in progress. Current status: {instance.Status}.");

        // Verify step belongs to this instance's definition
        var currentStep = instance.Definition.Steps.FirstOrDefault(s => s.Id == instance.CurrentStepId);
        if (currentStep == null || currentStep.Id != request.StepId)
            throw new InvalidOperationException($"Step {request.StepId} is not the current step of this workflow instance.");

        // Find next step based on decision
        WorkflowStep? nextStep = null;
        if (request.Decision == WorkflowDecision.Approved || request.Decision == WorkflowDecision.Received)
        {
            // Move to next step in sequence
            nextStep = instance.Definition.Steps
                .Where(s => s.StepOrder > currentStep.StepOrder)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault();
        }
        else if (request.Decision == WorkflowDecision.Rejected)
        {
            // If there's an escalation step, go there
            nextStep = currentStep.EscalateToStepId.HasValue
                ? instance.Definition.Steps.FirstOrDefault(s => s.Id == currentStep.EscalateToStepId.Value)
                : null;
        }

        if (nextStep == null)
        {
            // No next step - workflow completes or fails
            if (request.Decision == WorkflowDecision.Approved || request.Decision == WorkflowDecision.Received)
            {
                instance.Status = WorkflowInstanceStatus.Completed.ToString();
                instance.CompletedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                instance.Status = WorkflowInstanceStatus.Failed.ToString();
                instance.CompletedAt = DateTimeOffset.UtcNow;
            }
            instance.CurrentStepId = null;
        }
        else
        {
            instance.CurrentStepId = nextStep.Id;
        }

        instance.LastModified = DateTimeOffset.UtcNow;
        instance.LastModifiedBy = "system"; // TODO: Get from current user

        // Create history entry
        context.WorkflowHistory.Add(new Domain.Workflow.Entities.WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = request.StepId,
            ActorUserId = userId,
            Decision = request.Decision.ToString(),
            Reason = request.Reason,
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowInstanceId = instance.Id,
            StepId = request.StepId,
            Decision = request.Decision.ToString(),
            Reason = request.Reason,
            NextStepId = nextStep?.Id,
            NewStatus = instance.Status
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "AdvanceInstance",
            UserId = userId,
            EntityName = "WorkflowInstance",
            EntityId = instance.Id,
            Success = true,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}