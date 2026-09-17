using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Shared.Workflow;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Commands.CreateWorkflowDefinition;

[Authorize(Policy = PermissionCodes.WorkflowDefinitionsManage)]
public class CreateWorkflowDefinitionCommand : IRequest<Result<int>>
{
    public string EntityName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<WorkflowStepDto> Steps { get; init; } = new();
}

public class CreateWorkflowDefinitionCommandValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required.")
            .MaximumLength(50).WithMessage("Entity name must not exceed 50 characters.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one step is required.");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.StepOrder)
                .GreaterThan(0).WithMessage("Step order must be greater than 0.");
            step.RuleFor(s => s.StepType)
                .NotEmpty().WithMessage("Step type is required.")
                .MaximumLength(20).WithMessage("Step type must not exceed 20 characters.");
            step.RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Step name is required.")
                .MaximumLength(100).WithMessage("Step name must not exceed 100 characters.");
        });
    }
}

public class CreateWorkflowDefinitionCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<CreateWorkflowDefinitionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

        // Get next version number for this entity
        var lastVersion = await context.WorkflowDefinitions
            .Where(d => d.EntityName == request.EntityName)
            .MaxAsync(d => (int?)d.Version, cancellationToken) ?? 0;

        var definition = new Domain.Workflow.Entities.WorkflowDefinition
        {
            EntityName = request.EntityName,
            Version = lastVersion + 1,
            Status = Domain.Workflow.Enums.WorkflowDefinitionStatus.Draft.ToString(),
            Description = request.Description,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.WorkflowDefinitions.Add(definition);
        await context.SaveChangesAsync(cancellationToken);

        // Add steps
        foreach (var stepDto in request.Steps)
        {
            var step = new Domain.Workflow.Entities.WorkflowStep
            {
                DefinitionId = definition.Id,
                StepOrder = stepDto.StepOrder,
                StepType = stepDto.StepType,
                Name = stepDto.Name,
                Description = stepDto.Description,
                AssignedRoleId = stepDto.AssignedRoleId,
                UseApprovalRules = stepDto.UseApprovalRules,
                ConditionExpression = stepDto.ConditionExpression,
                TimeoutHours = stepDto.TimeoutHours,
                EscalateToStepId = stepDto.EscalateToStepId,
                IsActive = true
            };
            context.WorkflowSteps.Add(step);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Audit log
        var auditPayload = new
        {
            WorkflowDefinitionId = definition.Id,
            EntityName = definition.EntityName,
            Version = definition.Version,
            StepCount = request.Steps.Count
        };

        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "CreateDefinition",
            UserId = userId,
            EntityName = "WorkflowDefinition",
            EntityId = definition.Id,
            Success = true,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(definition.Id);
    }
}