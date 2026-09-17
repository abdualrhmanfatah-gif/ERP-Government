using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowInstances;

[Authorize(Policy = PermissionCodes.WorkflowInstancesView)]
public class GetWorkflowInstancesQuery : IRequest<List<WorkflowInstanceDto>>
{
    public string? EntityName { get; init; }
    public int? EntityId { get; init; }
    public string? Status { get; init; }
}

public class GetWorkflowInstancesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWorkflowInstancesQuery, List<WorkflowInstanceDto>>
{
    public async Task<List<WorkflowInstanceDto>> Handle(
        GetWorkflowInstancesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.WorkflowInstances
            .Include(i => i.History)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(i => i.EntityName == request.EntityName);

        if (request.EntityId.HasValue)
            query = query.Where(i => i.EntityId == request.EntityId.Value);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(i => i.Status == request.Status);

        return await query
            .OrderByDescending(i => i.Created)
            .Select(i => new WorkflowInstanceDto
            {
                Id = i.Id,
                DefinitionId = i.DefinitionId,
                EntityName = i.EntityName,
                EntityId = i.EntityId,
                CurrentStepId = i.CurrentStepId,
                Status = i.Status,
                StartedAt = i.StartedAt,
                CompletedAt = i.CompletedAt,
                Created = i.Created,
                CreatedBy = i.CreatedBy,
                LastModified = i.LastModified,
                LastModifiedBy = i.LastModifiedBy,
                History = i.History.Select(h => new WorkflowHistoryDto
                {
                    Id = h.Id,
                    WorkflowInstanceId = h.WorkflowInstanceId,
                    StepId = h.StepId,
                    ActorUserId = h.ActorUserId,
                    Decision = h.Decision,
                    Reason = h.Reason,
                    EvaluationSnapshot = h.EvaluationSnapshot,
                    Timestamp = h.Timestamp
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}