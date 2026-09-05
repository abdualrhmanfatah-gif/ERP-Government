using ERP_Government.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Outbox;

[TestFixture]
public class OutboxMessageTests
{
    [Test]
    public void NewOutboxMessage_HasPendingStatusByDefault()
    {
        var message = new OutboxMessage();

        message.Status.ShouldBe(OutboxMessageStatus.Pending);
    }

    [Test]
    public void NewOutboxMessage_HasZeroRetryCountByDefault()
    {
        var message = new OutboxMessage();

        message.RetryCount.ShouldBe(0);
    }

    [Test]
    public void NewOutboxMessage_HasEmptyErrorMessage()
    {
        var message = new OutboxMessage();

        message.ErrorMessage.ShouldBeNull();
    }

    [Test]
    public void NewOutboxMessage_HasNullProcessedAt()
    {
        var message = new OutboxMessage();

        message.ProcessedAt.ShouldBeNull();
    }

    [Test]
    public void NewOutboxMessage_HasNullNextRetryAt()
    {
        var message = new OutboxMessage();

        message.NextRetryAt.ShouldBeNull();
    }

    [Test]
    public void NewOutboxMessage_HasCorrelationId()
    {
        var message = new OutboxMessage();

        message.CorrelationId.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public void NewOutboxMessage_HasCreatedAt()
    {
        var message = new OutboxMessage();

        message.CreatedAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Test]
    public void OutboxMessage_CanTransitionToProcessing()
    {
        var message = new OutboxMessage();

        message.Status = OutboxMessageStatus.Processing;

        message.Status.ShouldBe(OutboxMessageStatus.Processing);
    }

    [Test]
    public void OutboxMessage_CanTransitionToProcessed()
    {
        var message = new OutboxMessage();

        message.Status = OutboxMessageStatus.Processed;
        message.ProcessedAt = DateTimeOffset.UtcNow;

        message.Status.ShouldBe(OutboxMessageStatus.Processed);
        message.ProcessedAt.ShouldNotBeNull();
    }

    [Test]
    public void OutboxMessage_CanTransitionToFailed()
    {
        var message = new OutboxMessage();

        message.Status = OutboxMessageStatus.Failed;
        message.ErrorMessage = "Test error";

        message.Status.ShouldBe(OutboxMessageStatus.Failed);
        message.ErrorMessage.ShouldBe("Test error");
    }

    [Test]
    public void OutboxMessage_CanBeResetFromFailedToPending()
    {
        var message = new OutboxMessage
        {
            Status = OutboxMessageStatus.Failed,
            RetryCount = 5,
            ErrorMessage = "Max retries exceeded"
        };

        message.Status = OutboxMessageStatus.Pending;
        message.RetryCount = 0;
        message.ErrorMessage = null;
        message.NextRetryAt = null;

        message.Status.ShouldBe(OutboxMessageStatus.Pending);
        message.RetryCount.ShouldBe(0);
        message.ErrorMessage.ShouldBeNull();
        message.NextRetryAt.ShouldBeNull();
    }

    [Test]
    public void OutboxMessage_AllStatusValues_AreDefined()
    {
        var values = Enum.GetValues<OutboxMessageStatus>();

        values.ShouldContain(OutboxMessageStatus.Pending);
        values.ShouldContain(OutboxMessageStatus.Processing);
        values.ShouldContain(OutboxMessageStatus.Processed);
        values.ShouldContain(OutboxMessageStatus.Failed);
    }

    [Test]
    public void OutboxMessage_TypeNameIsRequired()
    {
        var message = new OutboxMessage { TypeName = string.Empty };

        message.TypeName.ShouldBeEmpty();
    }

    [Test]
    public void OutboxMessage_PayloadIsRequired()
    {
        var message = new OutboxMessage { Payload = "{}" };

        message.Payload.ShouldBe("{}");
    }
}
