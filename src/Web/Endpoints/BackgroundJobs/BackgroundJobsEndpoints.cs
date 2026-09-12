using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Endpoint.BackgroundJobs;

public class BackgroundJobsEndpoints : IEndpointGroup
{
    public static string? RoutePrefix => "/api/background-jobs";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBackgroundJobs)
            .Produces<BackgroundJobsResponse>()
            .RequireAuthorization();

        groupBuilder.MapGet("/{id:int}/history", GetBackgroundJobHistory)
            .Produces<BackgroundJobHistoryResponse>()
            .RequireAuthorization();

        groupBuilder.MapPost("/{id:int}/cancel", CancelBackgroundJob)
            .Produces<CancelBackgroundJobResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetBackgroundJobs(
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var definitions = await context.BackgroundJobDefinitions
            .Where(d => d.IsActive)
            .OrderBy(d => d.DisplayName)
            .ToListAsync(cancellationToken);

        var result = new List<BackgroundJobDto>();

        foreach (var def in definitions)
        {
            var lastInstance = await context.BackgroundJobInstances
                .Where(i => i.JobDefinitionId == def.Id)
                .OrderByDescending(i => i.ScheduledAt)
                .FirstOrDefaultAsync(cancellationToken);

            var nextScheduled = await context.BackgroundJobInstances
                .Where(i => i.JobDefinitionId == def.Id
                         && i.Status == BackgroundJobStatus.Pending
                         && i.ScheduledAt > DateTimeOffset.UtcNow)
                .OrderBy(i => i.ScheduledAt)
                .Select(i => i.ScheduledAt)
                .FirstOrDefaultAsync(cancellationToken);

            result.Add(new BackgroundJobDto
            {
                Id = def.Id,
                TypeName = def.TypeName,
                DisplayName = def.DisplayName,
                ScheduleType = def.ScheduleType.ToString(),
                ScheduleValue = def.ScheduleValue,
                IsActive = def.IsActive,
                CurrentStatus = lastInstance?.Status.ToString() ?? "NeverRun",
                LastExecutedAt = lastInstance?.CompletedAt,
                NextScheduledAt = nextScheduled == default ? null : nextScheduled,
                LastRetryCount = lastInstance?.RetryCount ?? 0,
                LastError = lastInstance?.ErrorMessage
            });
        }

        return Results.Ok(new BackgroundJobsResponse { Jobs = result });
    }

    private static async Task<IResult> GetBackgroundJobHistory(
        int id,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var definition = await context.BackgroundJobDefinitions
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (definition == null)
            return Results.NotFound($"Background job definition with ID {id} not found");

        var limit = 50;
        var offset = 0;

        var instances = await context.BackgroundJobInstances
            .Where(i => i.JobDefinitionId == id)
            .OrderByDescending(i => i.ScheduledAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var instanceDtos = new List<BackgroundJobInstanceDto>();

        foreach (var inst in instances)
        {
            var logs = await context.BackgroundJobExecutionLogs
                .Where(l => l.JobInstanceId == inst.Id)
                .OrderBy(l => l.AttemptNumber)
                .Select(l => new ExecutionAttemptDto
                {
                    AttemptNumber = l.AttemptNumber,
                    StartedAt = l.StartedAt,
                    CompletedAt = l.CompletedAt,
                    Status = l.Status.ToString(),
                    ErrorMessage = l.ErrorMessage
                })
                .ToListAsync(cancellationToken);

            instanceDtos.Add(new BackgroundJobInstanceDto
            {
                InstanceId = inst.Id,
                ScheduledAt = inst.ScheduledAt,
                StartedAt = inst.StartedAt,
                CompletedAt = inst.CompletedAt,
                Status = inst.Status.ToString(),
                RetryCount = inst.RetryCount,
                TriggeredBy = inst.TriggeredBy,
                Attempts = logs
            });
        }

        return Results.Ok(new BackgroundJobHistoryResponse
        {
            JobId = definition.Id,
            DisplayName = definition.DisplayName,
            Instances = instanceDtos,
            TotalCount = await context.BackgroundJobInstances.CountAsync(i => i.JobDefinitionId == id, cancellationToken),
            Limit = limit,
            Offset = offset
        });
    }

    private static async Task<IResult> CancelBackgroundJob(
        int id,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var instance = await context.BackgroundJobInstances
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (instance == null)
            return Results.NotFound($"Background job instance with ID {id} not found");

        if (instance.Status is BackgroundJobStatus.Completed or BackgroundJobStatus.Failed or BackgroundJobStatus.Cancelled)
            return Results.Conflict($"Instance {id} is already in terminal state: {instance.Status}");

        instance.Status = BackgroundJobStatus.Cancelled;
        instance.CompletedAt = DateTimeOffset.UtcNow;
        instance.ErrorMessage = "Cancelled by operator";

        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new CancelBackgroundJobResponse
        {
            InstanceId = instance.Id,
            Status = instance.Status.ToString(),
            CancelledAt = instance.CompletedAt,
            Reason = "Cancelled by operator"
        });
    }
}
