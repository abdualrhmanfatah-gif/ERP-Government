using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.EventHandlers;

[TestFixture]
public class PostingRuleMatcherTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private async Task SeedPostingRuleAsync(string eventType, int journalId, int priority, bool isActive = true)
    {
        using var context = new ApplicationDbContext(_options);
        context.PostingRules.Add(new PostingRule
        {
            Name = $"Rule for {eventType} P{priority}",
            EventType = eventType,
            JournalId = journalId,
            Priority = priority,
            IsActive = isActive
        });
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task MatchAsync_WithMatchingRules_ReturnsOrderedByPriority()
    {
        // Arrange
        await SeedPostingRuleAsync("PurchaseOrderApproved", 2, 20);
        await SeedPostingRuleAsync("PurchaseOrderApproved", 1, 10);
        await SeedPostingRuleAsync("PurchaseOrderApproved", 3, 30);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseOrderApproved");

        // Assert
        rules.Count.ShouldBe(3);
        rules[0].JournalId.ShouldBe(1); // Priority 10
        rules[1].JournalId.ShouldBe(2); // Priority 20
        rules[2].JournalId.ShouldBe(3); // Priority 30
    }

    [Test]
    public async Task MatchAsync_WithInactiveRules_ExcludesInactive()
    {
        // Arrange
        await SeedPostingRuleAsync("PurchaseOrderApproved", 1, 10, isActive: true);
        await SeedPostingRuleAsync("PurchaseOrderApproved", 2, 20, isActive: false);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseOrderApproved");

        // Assert
        rules.Count.ShouldBe(1);
        rules[0].JournalId.ShouldBe(1);
    }

    [Test]
    public async Task MatchAsync_WithNoMatchingRules_ReturnsEmpty()
    {
        // Arrange
        await SeedPostingRuleAsync("OtherEvent", 1, 10);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseOrderApproved");

        // Assert
        rules.ShouldBeEmpty();
    }

    [Test]
    public async Task MatchAsync_WithMultipleEventTypes_ReturnsCorrectType()
    {
        // Arrange
        await SeedPostingRuleAsync("PurchaseOrderApproved", 1, 10);
        await SeedPostingRuleAsync("PaymentOrderExecuted", 2, 10);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PaymentOrderExecuted");

        // Assert
        rules.Count.ShouldBe(1);
        rules[0].EventType.ShouldBe("PaymentOrderExecuted");
    }

    [Test]
    public async Task MatchAsync_SeedData_PurchaseOrderApproved_ReturnsRule()
    {
        // Arrange — simulate seed data with corrected EventType
        await SeedPostingRuleAsync("PurchaseOrderApproved", 2, 2);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseOrderApproved");

        // Assert
        rules.ShouldNotBeEmpty();
        rules[0].EventType.ShouldBe("PurchaseOrderApproved");
    }

    [Test]
    public async Task MatchAsync_SeedData_PurchaseReceiptPosted_ReturnsRule()
    {
        // Arrange
        await SeedPostingRuleAsync("PurchaseReceiptPosted", 2, 1);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseReceiptPosted");

        // Assert
        rules.ShouldNotBeEmpty();
    }

    [Test]
    public async Task MatchAsync_SeedData_PaymentOrderExecuted_ReturnsRule()
    {
        // Arrange
        await SeedPostingRuleAsync("PaymentOrderExecuted", 4, 1);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PaymentOrderExecuted");

        // Assert
        rules.ShouldNotBeEmpty();
    }

    [Test]
    public async Task MatchAsync_InactiveRule_IsExcluded()
    {
        // Arrange — T013: verify inactive rules are excluded
        await SeedPostingRuleAsync("PurchaseOrderApproved", 1, 10, isActive: false);

        using var context = new ApplicationDbContext(_options);
        var matcher = new PostingRuleMatcher(context);

        // Act
        var rules = await matcher.MatchAsync("PurchaseOrderApproved");

        // Assert
        rules.ShouldBeEmpty();
    }
}
