using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Entities;
using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.BackgroundJobs;

[TestFixture]
public class BackgroundJobStoreIntegrationTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;
    private Mock<ILogger<BackgroundJobStore>> _logger = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "BackgroundJobStore_IntegrationTest")
            .Options;

        _logger = new Mock<ILogger<BackgroundJobStore>>();

        SeedData();
    }

    private void SeedData()
    {
        using var context = new ApplicationDbContext(_options);

        var definition = new BackgroundJobDefinition
        {
            TypeName = "TestJob",
            DisplayName = "Test Job",
            ScheduleType = BackgroundJobScheduleType.Interval,
            ScheduleValue = "60",
            MaxRetries = 3,
            TimeoutSeconds = 30,
            IsActive = true
        };
        context.BackgroundJobDefinitions.Add(definition);
        context.SaveChanges();
    }

    [Test]
    public async Task FullLifecycle_CreatedToCompleted()
    {
        using var context = new ApplicationDbContext(_options);
        var store = new BackgroundJobStore(context, _logger.Object);

        var instance = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        instance.Status.ShouldBe(BackgroundJobStatus.Pending);
        instance.StartedAt.ShouldBeNull();

        await store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);
        instance.Status.ShouldBe(BackgroundJobStatus.Running);
        instance.StartedAt.ShouldNotBeNull();

        await store.UpdateStatusAsync(instance, BackgroundJobStatus.Completed, cancellationToken: CancellationToken.None);
        instance.Status.ShouldBe(BackgroundJobStatus.Completed);
        instance.CompletedAt.ShouldNotBeNull();

        using var verifyContext = new ApplicationDbContext(_options);
        var persisted = await verifyContext.BackgroundJobInstances.FindAsync(instance.Id);
        persisted.ShouldNotBeNull();
        persisted!.Status.ShouldBe(BackgroundJobStatus.Completed);
    }

    [Test]
    public async Task FullLifecycle_CreatedToFailedWithRetry()
    {
        using var context = new ApplicationDbContext(_options);
        var store = new BackgroundJobStore(context, _logger.Object);

        var instance = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);
        await store.ScheduleRetryAsync(instance, TimeSpan.FromSeconds(5), CancellationToken.None);

        instance.RetryCount.ShouldBe(1);
        instance.Status.ShouldBe(BackgroundJobStatus.Pending);
        instance.ScheduledAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task IdempotencyKey_DuplicateReturnsSameInstance()
    {
        using var context = new ApplicationDbContext(_options);
        var store = new BackgroundJobStore(context, _logger.Object);

        var instance1 = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: "idem-key-001",
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        var instance2 = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: "idem-key-001",
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        instance1.Id.ShouldBe(instance2.Id);
    }

    [Test]
    public async Task ExecutionLog_CreatedWithCorrectAttemptNumber()
    {
        using var context = new ApplicationDbContext(_options);
        var store = new BackgroundJobStore(context, _logger.Object);

        var instance = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        var log1 = await store.CreateExecutionLogAsync(instance.Id, 1, CancellationToken.None);
        var log2 = await store.CreateExecutionLogAsync(instance.Id, 2, CancellationToken.None);

        log1.AttemptNumber.ShouldBe(1);
        log2.AttemptNumber.ShouldBe(2);

        using var verifyContext = new ApplicationDbContext(_options);
        var logs = await verifyContext.BackgroundJobExecutionLogs
            .Where(l => l.JobInstanceId == instance.Id)
            .OrderBy(l => l.AttemptNumber)
            .ToListAsync();

        logs.Count.ShouldBe(2);
        logs[0].AttemptNumber.ShouldBe(1);
        logs[1].AttemptNumber.ShouldBe(2);
    }

    [Test]
    public async Task OrphanedJobs_DetectedByTimeThreshold()
    {
        using var context = new ApplicationDbContext(_options);
        var store = new BackgroundJobStore(context, _logger.Object);

        var old = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow.AddHours(-3),
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        var recent = await store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await store.UpdateStatusAsync(old, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);
        await store.UpdateStatusAsync(recent, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);

        old.StartedAt = DateTimeOffset.UtcNow.AddHours(-2);
        await context.SaveChangesAsync(CancellationToken.None);

        var orphans = await store.GetOrphanedRunningJobsAsync(TimeSpan.FromMinutes(30), CancellationToken.None);

        orphans.Count.ShouldBe(1);
        orphans[0].Id.ShouldBe(old.Id);
    }
}
