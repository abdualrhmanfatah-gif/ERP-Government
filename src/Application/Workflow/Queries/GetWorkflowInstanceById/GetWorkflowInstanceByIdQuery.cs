using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowInstanceById;

[Authorize(Policy = PermissionCodes.WorkflowInstancesView)]
public class GetWorkflowInstanceByIdQuery : IRequest<Result<WorkflowInstanceDto>>
{
    public int Id { get; init; }
}

public class GetWorkflowInstanceByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetWorkflowInstanceByIdQuery, Result<WorkflowInstanceDto>>
{
    public async Task<Result<WorkflowInstanceDto>> Handle(
        GetWorkflowInstanceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.WorkflowInstances
            .Include(i => i.History)
            .Where(i => i.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<WorkflowInstanceDto>.Failure(ErrorCodes.Workflow.InstanceNotFound, ErrorCategory.NotFound, $"Workflow instance with ID {request.Id} not found.");

        return Result<WorkflowInstanceDto>.Success(dto);
    }
}
