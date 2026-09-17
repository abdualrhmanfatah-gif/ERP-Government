using ERP_Government.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.IntegrationTests;

public class OutboxRecoveryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OutboxRecoveryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ClaimMessage_ShouldSetStatusToProcessing_AndSetLeaseExpiry()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var message = new OutboxMessage
        {
            TypeName = "TestEvent",
            Payload = "{}",
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        var sut = new OutboxProcessorService(
            _factory.Services,
            new Mock<ILogger<OutboxProcessorService>>().Object,
            Options.Create(new OutboxOptions { PollIntervalMs = 1000, BatchSize = 10, MaxRetries = 3 }));

        var claimed = await sut.ClaimMessageAsync(message, context, CancellationToken.None);

        Assert.True(claimed);
        Assert.Equal(OutboxMessageStatus.Processing, message.Status);
        Assert.NotNull(message.LeaseExpiry);
        Assert.True(message.LeaseExpiry > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RecoverStalledMessages_ShouldResetExpiredLeasesToPending()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var stalledMessage = new OutboxMessage
        {
            TypeName = "TestEvent",
            Payload = "{}",
            Status = OutboxMessageStatus.Processing,
            LeaseExpiry = DateTimeOffset.UtcNow.AddMinutes(-5),
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.OutboxMessages.Add(stalledMessage);
        await context.SaveChangesAsync();

        var sut = new OutboxProcessorService(
            _factory.Services,
            new Mock<ILogger<OutboxProcessorService>>().Object,
            Options.Create(new OutboxOptions { PollIntervalMs = 1000, BatchSize = 10, MaxRetries = 3 }));

        await sut.RecoverStalledMessagesAsync(CancellationToken.None);

        var updatedMessage = await context.OutboxMessages.FindAsync(stalledMessage.Id);
        Assert.Equal(OutboxMessageStatus.Pending, updatedMessage!.Status);
        Assert.Null(updatedMessage.LeaseExpiry);
    }

    [Fact]
    public async Task ConcurrentClaims_ShouldNotAllowDoubleProcessing()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var message = new OutboxMessage
        {
            TypeName = "TestEvent",
            Payload = "{}",
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        var sut = new OutboxProcessorService(
            _factory.Services,
            new Mock<ILogger<OutboxProcessorService>>().Object,
            Options.Create(new OutboxOptions { PollIntervalMs = 1000, BatchSize = 10, MaxRetries = 3 }));

        var claimed1 = await sut.ClaimMessageAsync(message, context, CancellationToken.None);
        var claimed2 = await sut.ClaimMessageAsync(message, context, CancellationToken.None);

        Assert.True(claimed1);
        Assert.False(claimed2);
    }
}

public interface IApplicationDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public class OutboxOptions
{
    public int PollIntervalMs { get; set; }
    public int BatchSize { get; set; }
    public int MaxRetries { get; set; }
    public int RetentionDays { get; set; } = 7;
}
