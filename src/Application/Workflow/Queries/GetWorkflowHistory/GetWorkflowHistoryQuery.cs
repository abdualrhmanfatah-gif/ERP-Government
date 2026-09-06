using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowHistory;

public class GetWorkflowHistoryQuery : IRequest<List<WorkflowHistoryDto>>
{
    public int InstanceId { get; init; }
}

public class GetWorkflowHistoryQueryHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<GetWorkflowHistoryQuery, List<WorkflowHistoryDto>>
{
    public async Task<List<WorkflowHistoryDto>> Handle(
        GetWorkflowHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            throw new InvalidOperationException("User identity is required for this operation.");

        var results = await context.WorkflowHistory
            .Where(h => h.WorkflowInstanceId == request.InstanceId)
            .OrderBy(h => h.Timestamp)
            .Select(h => new WorkflowHistoryDto
            {
                Id = h.Id,
                WorkflowInstanceId = h.WorkflowInstanceId,
                StepId = h.StepId,
                ActorUserId = h.ActorUserId,
                Decision = h.Decision,
                Reason = h.Reason,
                EvaluationSnapshot = h.EvaluationSnapshot,
                Timestamp = h.Timestamp
            })
            .ToListAsync(cancellationToken);

        // Audit log for history access
        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Workflow",
            Action = "ViewHistory",
            UserId = userId,
            EntityName = "WorkflowInstance",
            EntityId = request.InstanceId,
            Success = true,
            NewValues = JsonSerializer.Serialize(new { RecordCount = results.Count }),
            Timestamp = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return results;
    }
}