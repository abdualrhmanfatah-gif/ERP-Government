using ERP_Government.Domain.BackgroundJobs.Common;
using ERP_Government.Domain.BackgroundJobs.Entities;
using ERP_Government.Domain.BackgroundJobs.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.BackgroundJobs;

[TestFixture]
public class BackgroundJobRetryPolicyTests
{
    [Test]
    public void GetBackoffDelay_FirstRetry_ReturnsFirstDelay()
    {
        var policy = new BackgroundJobRetryPolicy();
        var delay = policy.GetBackoffDelay(1);
        delay.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Test]
    public void GetBackoffDelay_SecondRetry_ReturnsSecondDelay()
    {
        var policy = new BackgroundJobRetryPolicy();
        var delay = policy.GetBackoffDelay(2);
        delay.ShouldBe(TimeSpan.FromSeconds(5));
    }

    [Test]
    public void GetBackoffDelay_ThirdRetry_ReturnsThirdDelay()
    {
        var policy = new BackgroundJobRetryPolicy();
        var delay = policy.GetBackoffDelay(3);
        delay.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void GetBackoffDelay_ExceedsSchedule_ReturnsLastDelay()
    {
        var policy = new BackgroundJobRetryPolicy();
        var delay = policy.GetBackoffDelay(100);
        delay.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Test]
    public void GetBackoffDelay_ZeroRetry_ReturnsZero()
    {
        var policy = new BackgroundJobRetryPolicy();
        var delay = policy.GetBackoffDelay(0);
        delay.ShouldBe(TimeSpan.Zero);
    }

    [Test]
    public void DefaultMaxRetries_IsFive()
    {
        var policy = new BackgroundJobRetryPolicy();
        policy.MaxRetries.ShouldBe(5);
    }

    [Test]
    public void DefaultBackoffSchedule_HasFiveElements()
    {
        var policy = new BackgroundJobRetryPolicy();
        policy.BackoffSchedule.Length.ShouldBe(5);
    }
}
