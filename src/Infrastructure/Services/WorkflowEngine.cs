using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using ERP_Government.Application.Workflow;
using ERP_Government.Domain.Workflow.Entities;
using ERP_Government.Domain.Workflow.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly IApplicationDbContext _context;
    private readonly IApprovalRuleEvaluationService _evaluationService;

    public WorkflowEngine(IApplicationDbContext context, IApprovalRuleEvaluationService evaluationService)
    {
        _context = context;
        _evaluationService = evaluationService;
    }

    public async Task<WorkflowInstance> StartInstanceAsync(string entityName, int entityId, CancellationToken cancellationToken = default)
    {
        // Find active definition for this entity
        var definition = await _context.WorkflowDefinitions
            .Include(d => d.Steps.Where(s => s.IsActive))
            .FirstOrDefaultAsync(d => d.EntityName == entityName && d.IsActive, cancellationToken);

        if (definition == null)
            throw new InvalidOperationException($"No active workflow definition found for entity '{entityName}'.");

        // Get first step
        var firstStep = definition.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (firstStep == null)
            throw new InvalidOperationException("Workflow definition has no active steps.");

        var instance = new WorkflowInstance
        {
            DefinitionId = definition.Id,
            EntityName = entityName,
            EntityId = entityId,
            CurrentStepId = firstStep.Id,
            Status = WorkflowInstanceStatus.InProgress.ToString(),
            StartedAt = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        // Record history entry for instance start
        _context.WorkflowHistory.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = firstStep.Id,
            ActorUserId = 0,
            Decision = WorkflowDecision.Started.ToString(),
            Timestamp = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return instance;
    }

    public async Task<WorkflowInstance> AdvanceInstanceAsync(int instanceId, int stepId, string decision, string? reason, CancellationToken cancellationToken = default)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.Definition)
            .ThenInclude(d => d!.Steps.Where(s => s.IsActive))
            .FirstOrDefaultAsync(i => i.Id == instanceId, cancellationToken);

        if (instance?.Definition == null)
            throw new InvalidOperationException($"Workflow instance with ID {instanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress.ToString())
            throw new InvalidOperationException($"Workflow instance is not in progress. Current status: {instance.Status}.");

        // Verify step belongs to this instance's definition
        var currentStep = instance.Definition.Steps.FirstOrDefault(s => s.Id == instance.CurrentStepId);
        if (currentStep == null || currentStep.Id != stepId)
            throw new InvalidOperationException($"Step {stepId} is not the current step of this workflow instance.");

        // Evaluate approval rules if configured for this step
        string? evaluationSnapshot = null;
        if (currentStep.UseApprovalRules)
        {
            var evaluationResults = await _evaluationService.EvaluateAsync(
                instance.EntityName,
                0m, // Amount could be resolved from entity if needed
                null,
                null,
                cancellationToken);

            if (evaluationResults.Count > 0)
            {
                evaluationSnapshot = System.Text.Json.JsonSerializer.Serialize(evaluationResults.Select(r => new
                {
                    r.Sequence,
                    r.RequiredRole,
                    r.ApproverRoleId
                }));
            }
        }

        // Find next step based on decision
        WorkflowStep? nextStep = null;
        if (decision == WorkflowDecision.Approved.ToString() || decision == WorkflowDecision.Received.ToString())
        {
            // Move to next step in sequence
            nextStep = instance.Definition.Steps
                .Where(s => s.StepOrder > currentStep.StepOrder)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault();
        }
        else if (decision == WorkflowDecision.Rejected.ToString())
        {
            // If there's an escalation step, go there
            nextStep = currentStep.EscalateToStepId.HasValue
                ? instance.Definition.Steps.FirstOrDefault(s => s.Id == currentStep.EscalateToStepId.Value)
                : null;
        }

        if (nextStep == null)
        {
            // No next step - workflow completes or fails
            if (decision == WorkflowDecision.Approved.ToString() || decision == WorkflowDecision.Received.ToString())
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

        // Record history entry with evaluation snapshot
        _context.WorkflowHistory.Add(new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = stepId,
            ActorUserId = 0,
            Decision = decision,
            EvaluationSnapshot = evaluationSnapshot,
            Timestamp = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return instance;
    }

    public async Task<WorkflowInstance> CancelInstanceAsync(int instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await _context.WorkflowInstances
            .FirstOrDefaultAsync(i => i.Id == instanceId, cancellationToken);

        if (instance == null)
            throw new InvalidOperationException($"Workflow instance with ID {instanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress.ToString())
            throw new InvalidOperationException($"Workflow instance is not in progress. Current status: {instance.Status}.");

        instance.Status = WorkflowInstanceStatus.Cancelled.ToString();
        instance.CompletedAt = DateTimeOffset.UtcNow;
        instance.LastModified = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return instance;
    }

    public async Task<WorkflowInstance?> GetInstanceAsync(int instanceId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(i => i.History)
            .FirstOrDefaultAsync(i => i.Id == instanceId, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowInstance>> GetInstancesAsync(string? entityName, int? entityId, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.History)
            .AsQueryable();

        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(i => i.EntityName == entityName);

        if (entityId.HasValue)
            query = query.Where(i => i.EntityId == entityId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        return await query
            .OrderByDescending(i => i.Created)
            .ToListAsync(cancellationToken);
    }
}