using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowDefinitions;

public class GetWorkflowDefinitionsQuery : IRequest<List<WorkflowDefinitionDto>>
{
    public string? EntityName { get; init; }
    public string? Status { get; init; }
}

public class GetWorkflowDefinitionsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWorkflowDefinitionsQuery, List<WorkflowDefinitionDto>>
{
    public async Task<List<WorkflowDefinitionDto>> Handle(
        GetWorkflowDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.WorkflowDefinitions
            .Include(d => d.Steps)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(d => d.EntityName == request.EntityName);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(d => d.Status == request.Status);

        return await query
            .OrderBy(d => d.EntityName)
            .ThenBy(d => d.Version)
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
            .ToListAsync(cancellationToken);
    }
}