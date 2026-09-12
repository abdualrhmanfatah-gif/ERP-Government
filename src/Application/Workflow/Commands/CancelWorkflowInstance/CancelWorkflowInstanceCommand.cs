using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Commands.CancelWorkflowInstance;

public class CancelWorkflowInstanceCommand : IRequest<Unit>
{
    public int InstanceId { get; init; }
    public string? Reason { get; init; }
}

public class CancelWorkflowInstanceCommandValidator : AbstractValidator<CancelWorkflowInstanceCommand>
{
    public CancelWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .GreaterThan(0).WithMessage("Instance ID must be greater than 0.");
    }
}

public class CancelWorkflowInstanceCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<CancelWorkflowInstanceCommand, Unit>
{
    public async Task<Unit> Handle(
        CancelWorkflowInstanceCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            throw new InvalidOperationException("User identity is required for this operation.");

        var instance = await context.WorkflowInstances
            .FirstOrDefaultAsync(i => i.Id == request.InstanceId, cancellationToken);

        if (instance == null)
            throw new InvalidOperationException($"Workflow instance with ID {request.InstanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress.ToString())
            throw new InvalidOperationException($"Workflow instance is not in progress. Current status: {instance.Status}.");

        instance.Status = WorkflowInstanceStatus.Cancelled.ToString();
        instance.CompletedAt = DateTimeOffset.UtcNow;
        instance.LastModified = DateTimeOffset.UtcNow;
        instance.LastModifiedBy = userId.ToString();

        // Create history entry
        context.WorkflowHistory.Add(new Domain.Workflow.Entities.WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            StepId = instance.CurrentStepId ?? 0,
            ActorUserId = userId,
            Decision = WorkflowDecision.Cancelled.ToString(),
            Reason = request.Reason,
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowInstanceId = instance.Id,
            Reason = request.Reason
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "CancelInstance",
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