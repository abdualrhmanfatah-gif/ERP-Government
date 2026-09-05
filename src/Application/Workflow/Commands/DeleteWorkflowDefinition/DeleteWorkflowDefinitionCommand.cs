using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Commands.DeleteWorkflowDefinition;

public class DeleteWorkflowDefinitionCommand : IRequest<Unit>
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
    IUser user) : IRequestHandler<DeleteWorkflowDefinitionCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var definition = await context.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (definition == null)
            throw new InvalidOperationException($"Workflow definition with ID {request.Id} not found.");

        // Check for active instances
        var hasActiveInstances = await context.WorkflowInstances
            .AnyAsync(i => i.DefinitionId == request.Id
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Completed.ToString()
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Cancelled.ToString()
                && i.Status != Domain.Workflow.Enums.WorkflowInstanceStatus.Failed.ToString(),
                cancellationToken);

        if (hasActiveInstances)
            throw new InvalidOperationException("Cannot delete workflow definition with active instances.");

        // Soft delete - set IsActive = false
        definition.IsActive = false;
        definition.LastModified = DateTimeOffset.UtcNow;
        definition.LastModifiedBy = "system"; // TODO: Get from current user

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var userId = user.Id ?? 0;
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

        return Unit.Value;
    }
}