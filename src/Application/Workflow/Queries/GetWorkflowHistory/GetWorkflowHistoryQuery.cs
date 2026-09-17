using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Workflow.Queries.GetWorkflowHistory;

[Authorize(Policy = PermissionCodes.WorkflowHistoryView)]
public class GetWorkflowHistoryQuery : IRequest<Result<List<WorkflowHistoryDto>>>
{
    public int InstanceId { get; init; }
}

public class GetWorkflowHistoryQueryHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<GetWorkflowHistoryQuery, Result<List<WorkflowHistoryDto>>>
{
    public async Task<Result<List<WorkflowHistoryDto>>> Handle(
        GetWorkflowHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<List<WorkflowHistoryDto>>.Failure(ErrorCodes.Workflow.IdentityRequired, ErrorCategory.Authorization, "User identity is required for this operation.");

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

        return Result<List<WorkflowHistoryDto>>.Success(results);
    }
}