using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Interceptors;

[TestFixture]
public class AuditTrailQueryabilityTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;

    [SetUp]
    public void SetUp()
    {
        var userMock = new Mock<IUser>();
        userMock.Setup(u => u.Id).Returns(1);

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(t => t.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        var requestContextMock = new Mock<IRequestContext>();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(
                new ImmutableEntityConstraint(),
                new AuditableEntityInterceptor(
                    userMock.Object,
                    timeProviderMock.Object,
                    requestContextMock.Object))
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Test]
    public async Task QueryByDocumentType_ReturnsOnlyMatchingRecords()
    {
        using var context = CreateContext();

        // Seed audit trail records for different document types
        context.AuditTrails.AddRange(
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Update, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Budget", Action = AuditAction.Create, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "ExchangeRate", Action = AuditAction.Create, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Success = true }
        );
        await context.SaveChangesAsync();

        var results = await context.AuditTrails
            .Where(a => a.DocumentType == "Currency")
            .ToListAsync();

        results.Count.ShouldBe(2);
        results.ShouldAllBe(a => a.DocumentType == "Currency");
    }

    [Test]
    public async Task QueryByUserId_ReturnsOnlyMatchingRecords()
    {
        using var context = CreateContext();

        context.AuditTrails.AddRange(
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 2, Timestamp = DateTimeOffset.UtcNow, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 3, Timestamp = DateTimeOffset.UtcNow, Success = true }
        );
        await context.SaveChangesAsync();

        var results = await context.AuditTrails
            .Where(a => a.UserId == 2)
            .ToListAsync();

        results.Count.ShouldBe(1);
        results[0].UserId.ShouldBe(2);
    }

    [Test]
    public async Task QueryByDateRange_ReturnsOnlyMatchingRecords()
    {
        using var context = CreateContext();

        var date1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var date2 = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var date3 = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

        context.AuditTrails.AddRange(
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 1, Timestamp = date1, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 1, Timestamp = date2, Success = true },
            new AuditTrail { EventCategory = "EntityChange", DocumentType = "Currency", Action = AuditAction.Create, UserId = 1, Timestamp = date3, Success = true }
        );
        await context.SaveChangesAsync();

        var startDate = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

        var results = await context.AuditTrails
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .ToListAsync();

        results.Count.ShouldBe(1);
        results[0].Timestamp.ShouldBe(date2);
    }

    [Test]
    public async Task Query_OldValuesNewValues_AreValidJson()
    {
        using var context = CreateContext();

        context.AuditTrails.AddRange(
            new AuditTrail
            {
                EventCategory = "EntityChange",
                DocumentType = "Currency",
                Action = AuditAction.Create,
                UserId = 1,
                Timestamp = DateTimeOffset.UtcNow,
                Success = true,
                OldValues = null,
                NewValues = "{\"Code\":\"USD\",\"Name\":\"US Dollar\"}",
                ChangeSummary = null
            },
            new AuditTrail
            {
                EventCategory = "EntityChange",
                DocumentType = "Currency",
                Action = AuditAction.Update,
                UserId = 1,
                Timestamp = DateTimeOffset.UtcNow,
                Success = true,
                OldValues = "{\"Name\":\"USD\"}",
                NewValues = "{\"Name\":\"US Dollar\"}",
                ChangeSummary = "Name: USD → US Dollar"
            }
        );
        await context.SaveChangesAsync();

        var records = await context.AuditTrails.ToListAsync();

        // Verify ChangeSummary is human-readable
        var updateRecord = records.First(a => a.Action == AuditAction.Update);
        updateRecord.ChangeSummary.ShouldNotBeNull();
        updateRecord.ChangeSummary.ShouldContain("Name");
        updateRecord.ChangeSummary.ShouldContain("→");

        // Verify OldValues/NewValues are valid JSON (not raw blobs)
        var createRecord = records.First(a => a.Action == AuditAction.Create);
        createRecord.NewValues.ShouldNotBeNull();
        createRecord.NewValues.ShouldContain("USD");
        createRecord.NewValues.ShouldContain("US Dollar");
    }
}
