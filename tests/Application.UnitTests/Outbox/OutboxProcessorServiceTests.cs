using ERP_Government.Infrastructure.Services;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Outbox;

[TestFixture]
public class OutboxProcessorServiceTests
{
    [Test]
    public void GetBackoffDelay_Retry1_Returns1Second()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(1);

        delay.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Test]
    public void GetBackoffDelay_Retry2_Returns5Seconds()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(2);

        delay.ShouldBe(TimeSpan.FromSeconds(5));
    }

    [Test]
    public void GetBackoffDelay_Retry3_Returns30Seconds()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(3);

        delay.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void GetBackoffDelay_Retry4_Returns2Minutes()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(4);

        delay.ShouldBe(TimeSpan.FromMinutes(2));
    }

    [Test]
    public void GetBackoffDelay_Retry5_Returns10Minutes()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(5);

        delay.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Test]
    public void GetBackoffDelay_Retry6_Returns10MinutesCapped()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(6);

        delay.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Test]
    public void GetBackoffDelay_Retry10_Returns10MinutesCapped()
    {
        var delay = OutboxProcessorService.GetBackoffDelay(10);

        delay.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Test]
    public void GetBackoffDelay_AlwaysReturnsNonNegative()
    {
        for (var i = 1; i <= 20; i++)
        {
            var delay = OutboxProcessorService.GetBackoffDelay(i);
            delay.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
        }
    }

    [Test]
    public void OutboxOptions_DefaultValues_AreCorrect()
    {
        var options = new OutboxOptions();

        options.PollIntervalMs.ShouldBe(5000);
        options.BatchSize.ShouldBe(50);
        options.MaxRetries.ShouldBe(5);
        options.RetentionDays.ShouldBe(7);
    }
}
