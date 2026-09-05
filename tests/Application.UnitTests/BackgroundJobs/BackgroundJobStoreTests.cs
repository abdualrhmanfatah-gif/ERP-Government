using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Entities;
using ERP_Government.Domain.BackgroundJobs.Enums;
using ERP_Government.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.BackgroundJobs;

[TestFixture]
public class BackgroundJobStoreTests
{
    private IApplicationDbContext _context = null!;
    private BackgroundJobStore _store = null!;
    private Mock<ILogger<BackgroundJobStore>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ERP_Government.Infrastructure.Data.ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ERP_Government.Infrastructure.Data.ApplicationDbContext(options);
        _context = context;

        // Seed a job definition
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

        _logger = new Mock<ILogger<BackgroundJobStore>>();
        _store = new BackgroundJobStore(context, _logger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        (_context as IDisposable)?.Dispose();
    }

    [Test]
    public async Task CreateInstanceAsync_CreatesNewInstance()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        instance.ShouldNotBeNull();
        instance.Id.ShouldBeGreaterThan(0);
        instance.Status.ShouldBe(BackgroundJobStatus.Pending);
        instance.TriggeredBy.ShouldBe("Scheduler");
    }

    [Test]
    public async Task CreateInstanceAsync_WithIdempotencyKey_CreatesInstanceWithKey()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: "test-key-123",
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        instance.IdempotencyKey.ShouldBe("test-key-123");
    }

    [Test]
    public async Task CreateInstanceAsync_DuplicateKey_ReturnsExistingInstance()
    {
        var instance1 = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: "duplicate-key",
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        var instance2 = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: "duplicate-key",
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        instance2.Id.ShouldBe(instance1.Id);
    }

    [Test]
    public async Task UpdateStatusAsync_ValidTransition_UpdatesStatus()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await _store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);

        instance.Status.ShouldBe(BackgroundJobStatus.Running);
        instance.StartedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task UpdateStatusAsync_InvalidTransition_ThrowsInvalidOperationException()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        // Pending -> Completed is invalid
        await Should.ThrowAsync<InvalidOperationException>(
            () => _store.UpdateStatusAsync(instance, BackgroundJobStatus.Completed, cancellationToken: CancellationToken.None));
    }

    [Test]
    public async Task UpdateStatusAsync_RunningToCompleted_SetsCompletedAt()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await _store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);
        await _store.UpdateStatusAsync(instance, BackgroundJobStatus.Completed, cancellationToken: CancellationToken.None);

        instance.CompletedAt.ShouldNotBeNull();
        instance.Status.ShouldBe(BackgroundJobStatus.Completed);
    }

    [Test]
    public async Task ScheduleRetryAsync_IncrementsRetryCountAndReschedules()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await _store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);
        await _store.ScheduleRetryAsync(instance, TimeSpan.FromSeconds(5), CancellationToken.None);

        instance.RetryCount.ShouldBe(1);
        instance.Status.ShouldBe(BackgroundJobStatus.Pending);
        instance.ScheduledAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task CreateExecutionLogAsync_CreatesLogEntry()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        var log = await _store.CreateExecutionLogAsync(instance.Id, 1, CancellationToken.None);

        log.ShouldNotBeNull();
        log.JobInstanceId.ShouldBe(instance.Id);
        log.AttemptNumber.ShouldBe(1);
        log.Status.ShouldBe(BackgroundJobExecutionStatus.Running);
    }

    [Test]
    public async Task GetOrphanedRunningJobsAsync_FindsOldRunningJobs()
    {
        var instance = await _store.CreateInstanceAsync(
            jobDefinitionId: 1,
            scheduledAt: DateTimeOffset.UtcNow,
            idempotencyKey: null,
            triggeredBy: "Scheduler",
            cancellationToken: CancellationToken.None);

        await _store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: CancellationToken.None);

        // Manually backdate StartedAt
        instance.StartedAt = DateTimeOffset.UtcNow.AddHours(-2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var orphans = await _store.GetOrphanedRunningJobsAsync(TimeSpan.FromMinutes(30), CancellationToken.None);

        orphans.Count.ShouldBe(1);
        orphans[0].Id.ShouldBe(instance.Id);
    }
}
