using ERP_Government.Application.Accounting.Commands.PostingRules.DeletePostingRule;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Commands.PostingRules;

/// <summary>
/// T021-T022: DeletePostingRuleCommand tests.
/// Tests delete guard (pending events) and successful deletion.
/// </summary>
[TestFixture]
public class DeletePostingRuleTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task Handle_WithPendingEvents_ReturnsFailure()
    {
        // Arrange — T021: DeletePostingRuleCommand rejects deletion with pending events
        using var context = new ApplicationDbContext(_options);

        var postingRule = new PostingRule
        {
            Name = "Test Rule",
            EventType = "PurchaseOrderApproved",
            JournalId = 2,
            Priority = 10,
            IsActive = true
        };
        context.PostingRules.Add(postingRule);

        context.AccountingEvents.Add(new AccountingEvent
        {
            EventType = EventType.PurchaseOrderApproved,
            SourceDocumentType = "PurchaseOrder",
            SourceDocumentId = 42,
            Status = EventStatus.Pending,
            RetryCount = 0
        });

        await context.SaveChangesAsync();

        var command = new DeletePostingRuleCommand { Id = postingRule.Id };

        // Act
        var handler = new DeletePostingRuleCommandHandler(context);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("pending AccountingEvents"));

        // Verify posting rule still exists
        var ruleExists = await context.PostingRules.AnyAsync(r => r.Id == postingRule.Id);
        ruleExists.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_WithNoPendingEvents_DeletesSuccessfully()
    {
        // Arrange — T022: DeletePostingRuleCommand allows deletion with no pending events
        using var context = new ApplicationDbContext(_options);

        var postingRule = new PostingRule
        {
            Name = "Test Rule",
            EventType = "PurchaseOrderApproved",
            JournalId = 2,
            Priority = 10,
            IsActive = true
        };
        context.PostingRules.Add(postingRule);
        await context.SaveChangesAsync();

        var command = new DeletePostingRuleCommand { Id = postingRule.Id };

        // Act
        var handler = new DeletePostingRuleCommandHandler(context);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();

        // Verify posting rule is deleted
        var ruleExists = await context.PostingRules.AnyAsync(r => r.Id == postingRule.Id);
        ruleExists.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_WithProcessingEvents_ReturnsFailure()
    {
        // Arrange — Processing events also block deletion
        using var context = new ApplicationDbContext(_options);

        var postingRule = new PostingRule
        {
            Name = "Test Rule",
            EventType = "PaymentOrderExecuted",
            JournalId = 4,
            Priority = 1,
            IsActive = true
        };
        context.PostingRules.Add(postingRule);

        context.AccountingEvents.Add(new AccountingEvent
        {
            EventType = EventType.PaymentOrderExecuted,
            SourceDocumentType = "PaymentOrder",
            SourceDocumentId = 10,
            Status = EventStatus.Posted,
            RetryCount = 0
        });

        await context.SaveChangesAsync();

        var command = new DeletePostingRuleCommand { Id = postingRule.Id };

        // Act
        var handler = new DeletePostingRuleCommandHandler(context);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("pending AccountingEvents"));
    }

    [Test]
    public async Task Handle_NonExistentRule_ReturnsFailure()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var command = new DeletePostingRuleCommand { Id = 999 };

        // Act
        var handler = new DeletePostingRuleCommandHandler(context);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
