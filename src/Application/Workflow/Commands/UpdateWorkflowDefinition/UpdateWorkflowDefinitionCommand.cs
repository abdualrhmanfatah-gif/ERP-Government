using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Commands.UpdateWorkflowDefinition;

public class UpdateWorkflowDefinitionCommand : IRequest<Unit>
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
    IUser user) : IRequestHandler<UpdateWorkflowDefinitionCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var definition = await context.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (definition == null)
            throw new InvalidOperationException($"Workflow definition with ID {request.Id} not found.");

        var oldDescription = definition.Description;
        var oldStatus = definition.Status;

        if (request.Description != null)
            definition.Description = request.Description;

        if (request.Status.HasValue)
            definition.Status = request.Status.Value.ToString();

        definition.LastModified = DateTimeOffset.UtcNow;
        definition.LastModifiedBy = "system"; // TODO: Get from current user

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var userId = user.Id ?? 0;
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

        return Unit.Value;
    }
}