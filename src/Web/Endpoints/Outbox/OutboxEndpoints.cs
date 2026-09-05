using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Common;
using ERP_Government.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Endpoint.Outbox;

public class OutboxEndpoints : IEndpointGroup
{
    public static string? RoutePrefix => "/api/outbox";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/status", GetOutboxStatus)
            .Produces<OutboxStatusResponse>()
            .RequireAuthorization();

        groupBuilder.MapPost("/{id:int}/reset", ResetOutboxMessage)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetOutboxStatus(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var pendingCount = await context.OutboxMessages
            .CountAsync(m => m.Status == OutboxMessageStatus.Pending, cancellationToken);

        var failedCount = await context.OutboxMessages
            .CountAsync(m => m.Status == OutboxMessageStatus.Failed, cancellationToken);

        var oldestPending = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var failedMessages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Failed)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new FailedOutboxMessageDto
            {
                Id = m.Id,
                TypeName = m.TypeName,
                AggregateId = m.AggregateId,
                RetryCount = m.RetryCount,
                ErrorMessage = m.ErrorMessage,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var response = new OutboxStatusResponse
        {
            PendingCount = pendingCount,
            FailedCount = failedCount,
            OldestPendingAge = oldestPending == default
                ? null
                : DateTimeOffset.UtcNow - oldestPending,
            FailedMessages = failedMessages
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> ResetOutboxMessage(int id, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var message = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (message is null)
            return Results.NotFound();

        if (message.Status != OutboxMessageStatus.Failed)
            return Results.BadRequest("Only Failed messages can be reset");

        message.Status = OutboxMessageStatus.Pending;
        message.RetryCount = 0;
        message.ErrorMessage = null;
        message.NextRetryAt = null;

        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}

public class OutboxStatusResponse
{
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public TimeSpan? OldestPendingAge { get; set; }
    public List<FailedOutboxMessageDto> FailedMessages { get; set; } = [];
}

public class FailedOutboxMessageDto
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? AggregateId { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
