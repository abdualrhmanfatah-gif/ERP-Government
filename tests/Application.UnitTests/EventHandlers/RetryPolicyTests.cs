using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

[TestFixture]
public class RetryPolicyTests
{
    private RetryPolicy _policy = null!;

    [SetUp]
    public void SetUp()
    {
        _policy = new RetryPolicy();
    }

    [Test]
    public void CanRetry_FailedEventWithLessThanMaxRetries_ReturnsTrue()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Posted,
            RetryCount = 2
        };

        _policy.CanRetry(accountingEvent).ShouldBeTrue();
    }

    [Test]
    public void CanRetry_FailedEventAtMaxRetries_ReturnsFalse()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Posted,
            RetryCount = 3
        };

        _policy.CanRetry(accountingEvent).ShouldBeFalse();
    }

    [Test]
    public void CanRetry_NonFailedEvent_ReturnsFalse()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Reversed,
            RetryCount = 0
        };

        _policy.CanRetry(accountingEvent).ShouldBeFalse();
    }

    [Test]
    public void RequiresManualRetry_FailedEventAtMaxRetries_ReturnsTrue()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Posted,
            RetryCount = 3
        };

        _policy.RequiresManualRetry(accountingEvent).ShouldBeTrue();
    }

    [Test]
    public void RequiresManualRetry_FailedEventBelowMaxRetries_ReturnsFalse()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Posted,
            RetryCount = 1
        };

        _policy.RequiresManualRetry(accountingEvent).ShouldBeFalse();
    }

    [Test]
    public void CanManualRetry_FailedEvent_ReturnsTrue()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Posted,
            RetryCount = 5
        };

        _policy.CanManualRetry(accountingEvent).ShouldBeTrue();
    }

    [Test]
    public void CanManualRetry_NonFailedEvent_ReturnsFalse()
    {
        var accountingEvent = new AccountingEvent
        {
            Status = EventStatus.Reversed,
            RetryCount = 0
        };

        _policy.CanManualRetry(accountingEvent).ShouldBeFalse();
    }

    [Test]
    public void MaxRetries_IsThree()
    {
        RetryPolicy.MaxRetries.ShouldBe(3);
    }
}
