using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Entities;
using ERP_Government.Domain.Workflow.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Commands.ExecuteApprovalStep;

public class ExecuteApprovalStepCommand : IRequest<Result>
{
    public int InstanceId { get; init; }
    public int StepId { get; init; }
    public WorkflowDecision Decision { get; init; }
    public string? Reason { get; init; }
}

public class ExecuteApprovalStepCommandValidator : AbstractValidator<ExecuteApprovalStepCommand>
{
    public ExecuteApprovalStepCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .GreaterThan(0).WithMessage("Instance ID must be greater than 0.");

        RuleFor(x => x.StepId)
            .GreaterThan(0).WithMessage("Step ID must be greater than 0.");

        RuleFor(x => x.Decision)
            .IsInEnum().WithMessage("Decision must be a valid workflow decision.");
    }
}

public class ExecuteApprovalStepCommandHandler(
    IApplicationDbContext context,
    IApprovalRuleEvaluationService evaluationService,
    IUser user) : IRequestHandler<ExecuteApprovalStepCommand, Result>
{
    public async Task<Result> Handle(
        ExecuteApprovalStepCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

        var instance = await context.WorkflowInstances
            .Include(i => i.Definition)
            .ThenInclude(d => d!.Steps.Where(s => s.IsActive))
            .FirstOrDefaultAsync(i => i.Id == request.InstanceId, cancellationToken);

        if (instance?.Definition == null)
            return Result.Failure(ErrorCodes.Workflow.InstanceNotFound, ErrorCategory.NotFound, $"Workflow instance with ID {request.InstanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress.ToString())
            return Result.Failure(ErrorCodes.Workflow.InstanceNotInProgress, ErrorCategory.BusinessRule, $"Workflow instance is not in progress. Current status: {instance.Status}.");

        var currentStep = instance.Definition.Steps.FirstOrDefault(s => s.Id == instance.CurrentStepId);
        if (currentStep == null || currentStep.Id != request.StepId)
            return Result.Failure(ErrorCodes.Workflow.StepNotFound, ErrorCategory.BusinessRule, $"Step {request.StepId} is not the current step of this workflow instance.");

        if (!currentStep.UseApprovalRules)
            return Result.Failure(ErrorCodes.Workflow.StepNotApproval, ErrorCategory.BusinessRule, "This step does not use approval rules. Use AdvanceWorkflowInstance instead.");

        // Evaluate approval rules
        var evaluationResults = await evaluationService.EvaluateAsync(
            instance.EntityName,
            0m, // Amount resolved from entity if available
            null,
            null,
            cancellationToken);

        if (evaluationResults.Count == 0)
            return Result.Failure(ErrorCodes.Workflow.NoApprovalRules, ErrorCategory.BusinessRule, "No approval rules defined for this entity type.");

        // Check if user has required role
        var hasRequiredRole = false;
        string? matchedRole = null;

        foreach (var rule in evaluationResults)
        {
            if (string.IsNullOrEmpty(rule.RequiredRole)) continue;

            if (await context.SecurityRoles.AnyAsync(r => r.Code == rule.RequiredRole, cancellationToken))
            {
                // In real implementation, check user roles via identity service
                hasRequiredRole = true;
                matchedRole = rule.RequiredRole;
                break;
            }
        }

        if (!hasRequiredRole)
        {
            var requiredRoles = evaluationResults.Select(r => r.RequiredRole).Where(r => !string.IsNullOrEmpty(r));
            return Result.Failure(ErrorCodes.Workflow.UserMissingRequiredRole, ErrorCategory.Authorization, $"User does not have any required approval role ({string.Join(", ", requiredRoles)}).");
        }

        // Record evaluation snapshot
        var evaluationSnapshot = JsonSerializer.Serialize(evaluationResults.Select(r => new
        {
            r.Sequence,
            r.RequiredRole,
            r.ApproverRoleId
        }));

        // Advance workflow
        WorkflowStep? nextStep = null;
        if (request.Decision == WorkflowDecision.Approved)
        {
            nextStep = instance.Definition.Steps
                .Where(s => s.StepOrder > currentStep.StepOrder)
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault();
        }
        else if (request.Decision == WorkflowDecision.Rejected)
        {
            nextStep = currentStep.EscalateToStepId.HasValue
                ? instance.Definition.Steps.FirstOrDefault(s => s.Id == currentStep.EscalateToStepId.Value)
                : null;
        }

        if (nextStep == null)
        {
            if (request.Decision == WorkflowDecision.Approved)
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
        instance.LastModifiedBy = userId.ToString();

        // Create history entry
        context.WorkflowHistory.Add(new Domain.Workflow.Entities.WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = request.StepId,
            ActorUserId = userId,
            Decision = request.Decision.ToString(),
            Reason = request.Reason,
            EvaluationSnapshot = evaluationSnapshot,
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowInstanceId = instance.Id,
            StepId = request.StepId,
            Decision = request.Decision.ToString(),
            RequiredRole = matchedRole,
            EvaluationResults = evaluationResults.Select(r => new { r.Sequence, r.RequiredRole })
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "ExecuteApprovalStep",
            UserId = userId,
            EntityName = "WorkflowInstance",
            EntityId = instance.Id,
            Success = true,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}