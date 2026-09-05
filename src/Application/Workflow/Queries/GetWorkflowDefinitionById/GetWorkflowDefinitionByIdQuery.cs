using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowDefinitionById;

public class GetWorkflowDefinitionByIdQuery : IRequest<WorkflowDefinitionDto?>
{
    public int Id { get; init; }
}

public class GetWorkflowDefinitionByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWorkflowDefinitionByIdQuery, WorkflowDefinitionDto?>
{
    public async Task<WorkflowDefinitionDto?> Handle(
        GetWorkflowDefinitionByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.WorkflowDefinitions
            .Include(d => d.Steps)
            .Where(d => d.Id == request.Id)
            .Select(d => new WorkflowDefinitionDto
            {
                Id = d.Id,
                EntityName = d.EntityName,
                Version = d.Version,
                Status = d.Status,
                Description = d.Description,
                IsActive = d.IsActive,
                Created = d.Created,
                CreatedBy = d.CreatedBy,
                LastModified = d.LastModified,
                LastModifiedBy = d.LastModifiedBy,
                Steps = d.Steps.Select(s => new WorkflowStepDto
                {
                    Id = s.Id,
                    DefinitionId = s.DefinitionId,
                    StepOrder = s.StepOrder,
                    StepType = s.StepType,
                    Name = s.Name,
                    Description = s.Description,
                    AssignedRoleId = s.AssignedRoleId,
                    UseApprovalRules = s.UseApprovalRules,
                    ConditionExpression = s.ConditionExpression,
                    TimeoutHours = s.TimeoutHours,
                    EscalateToStepId = s.EscalateToStepId,
                    IsActive = s.IsActive
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}