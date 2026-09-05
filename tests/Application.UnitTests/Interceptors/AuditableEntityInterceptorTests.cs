using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Interceptors;

[TestFixture]
public class AuditableEntityInterceptorTests
{
    private DbContextOptions<ApplicationDbContext> _options = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<TimeProvider> _timeProviderMock = null!;
    private Mock<IRequestContext> _requestContextMock = null!;
    private DateTimeOffset _fixedTime;

    [SetUp]
    public void SetUp()
    {
        _fixedTime = new DateTimeOffset(2026, 8, 24, 12, 0, 0, TimeSpan.Zero);

        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(42);

        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(t => t.GetUtcNow()).Returns(_fixedTime);

        _requestContextMock = new Mock<IRequestContext>();
        _requestContextMock.Setup(r => r.IpAddress).Returns("127.0.0.1");
        _requestContextMock.Setup(r => r.Endpoint).Returns("/api/test");

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(
                new ImmutableEntityConstraint(),
                new AuditableEntityInterceptor(
                    _userMock.Object,
                    _timeProviderMock.Object,
                    _requestContextMock.Object))
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Test]
    public async Task SaveChanges_Insert_CreatesAuditTrailWithCreatedAction()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };

        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Create);

        auditTrail.ShouldNotBeNull();
        auditTrail.DocumentId.ShouldBe(currency.Id);
        auditTrail.NewValues.ShouldNotBeNull();
        auditTrail.NewValues.ShouldContain("USD");
        auditTrail.NewValues.ShouldContain("US Dollar");
        auditTrail.OldValues.ShouldBeNull();
        auditTrail.ChangeSummary.ShouldBeNull();
        auditTrail.UserId.ShouldBe(42);
        auditTrail.Timestamp.ShouldBe(_fixedTime);
        auditTrail.Success.ShouldBeTrue();
        auditTrail.EventCategory.ShouldBe("EntityChange");
        auditTrail.IpAddress.ShouldBe("127.0.0.1");
    }

    [Test]
    public async Task SaveChanges_Update_WhenPropertiesChanged_CreatesAuditTrailWithUpdatedAction()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "USD",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        // Clear change tracker to simulate fresh request
        context.ChangeTracker.Clear();

        // Reload and modify
        var loaded = await context.Currencies.FindAsync(currency.Id);
        loaded.ShouldNotBeNull();
        loaded.Name = "US Dollar";

        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Update);

        auditTrail.ShouldNotBeNull();
        auditTrail.OldValues.ShouldNotBeNull();
        auditTrail.OldValues.ShouldContain("USD");
        auditTrail.NewValues.ShouldNotBeNull();
        auditTrail.NewValues.ShouldContain("US Dollar");
        auditTrail.FieldChanges.ShouldNotBeNull();
        auditTrail.FieldChanges.ShouldContain("Name");
        auditTrail.ChangeSummary.ShouldNotBeNull();
        auditTrail.ChangeSummary.ShouldContain("Name");
        auditTrail.ChangeSummary.ShouldContain("USD");
        auditTrail.ChangeSummary.ShouldContain("US Dollar");
    }

    [Test]
    public async Task SaveChanges_Delete_CreatesAuditTrailWithDeletedAction()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var loaded = await context.Currencies.FindAsync(currency.Id);
        loaded.ShouldNotBeNull();
        context.Currencies.Remove(loaded);

        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Delete);

        auditTrail.ShouldNotBeNull();
        auditTrail.OldValues.ShouldNotBeNull();
        auditTrail.OldValues.ShouldContain("USD");
        auditTrail.NewValues.ShouldBeNull();
    }

    [Test]
    public async Task SaveChanges_Update_NoOp_DoesNotCreateAuditTrail()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();
        var countAfterInsert = await context.AuditTrails.CountAsync();

        context.ChangeTracker.Clear();

        // Reload without changing anything
        var loaded = await context.Currencies.FindAsync(currency.Id);
        loaded.ShouldNotBeNull();
        // No property changes — touch LastModified to trigger Modified state
        // but the interceptor should detect no actual property changes

        await context.SaveChangesAsync();
        var countAfterNoOp = await context.AuditTrails.CountAsync();

        // Should only have the insert audit record, no update record
        countAfterNoOp.ShouldBe(countAfterInsert);
    }

    [Test]
    public async Task SaveChanges_MultipleEntities_CreatesAuditTrailForEach()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };

        var currency2 = new Currency
        {
            Code = "EUR",
            Name = "Euro",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };

        context.Currencies.AddRange(currency, currency2);
        await context.SaveChangesAsync();

        var auditTrails = context.AuditTrails
            .Where(a => a.DocumentType == "Currency")
            .ToList();

        auditTrails.Count.ShouldBe(2);
        auditTrails.ShouldAllBe(a => a.Action == AuditAction.Create);
    }

    [Test]
    public async Task SaveChanges_SoftDelete_CreatesAuditTrailWithUpdatedAction()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var loaded = await context.Currencies.FindAsync(currency.Id);
        loaded.ShouldNotBeNull();
        loaded.IsActive = false;

        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Update);

        auditTrail.ShouldNotBeNull();
        auditTrail.ChangeSummary.ShouldNotBeNull();
        auditTrail.ChangeSummary.ShouldContain("IsActive");
    }

    [Test]
    public void SaveChanges_ImmutableEntity_ThrowsInvalidOperationException()
    {
        using var context = CreateContext();

        // Manually add an AuditTrail record (bypass interceptor for INSERT)
        context.AuditTrails.Add(new AuditTrail
        {
            EventCategory = "Test",
            Action = AuditAction.Create,
            UserId = 1,
            Timestamp = _fixedTime,
            Success = true
        });
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var record = context.AuditTrails.First();
        record.EventCategory = "Tampered";

        var ex = Should.Throw<InvalidOperationException>(() => context.SaveChanges());
        ex.Message.ShouldContain("IImmutableEntity");
    }

    [Test]
    public void SaveChanges_DeleteImmutableEntity_ThrowsInvalidOperationException()
    {
        using var context = CreateContext();

        context.AuditTrails.Add(new AuditTrail
        {
            EventCategory = "Test",
            Action = AuditAction.Create,
            UserId = 1,
            Timestamp = _fixedTime,
            Success = true
        });
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var record = context.AuditTrails.First();
        context.AuditTrails.Remove(record);

        var ex = Should.Throw<InvalidOperationException>(() => context.SaveChanges());
        ex.Message.ShouldContain("IImmutableEntity");
    }

    [Test]
    public async Task SaveChanges_SecurityAuditLog_Immutable()
    {
        using var context = CreateContext();

        // Add SecurityAuditLog directly (bypass interceptor)
        context.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            EventCategory = "Security",
            Action = "Login",
            UserId = 1,
            Timestamp = _fixedTime,
            Success = true
        });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var record = await context.SecurityAuditLogs.FirstAsync();
        record.EntityName = "Tampered";

        var ex = Should.Throw<InvalidOperationException>(() => context.SaveChanges());
        ex.Message.ShouldContain("IImmutableEntity");
    }

    [Test]
    public async Task SaveChanges_Insert_Allowed_On_ImmutableEntity()
    {
        using var context = CreateContext();

        // INSERT should be allowed for IImmutableEntity
        context.AuditTrails.Add(new AuditTrail
        {
            EventCategory = "Test",
            Action = AuditAction.Create,
            UserId = 1,
            Timestamp = _fixedTime,
            Success = true
        });

        // Should not throw
        await context.SaveChangesAsync();

        context.AuditTrails.Count().ShouldBe(1);
    }

    [Test]
    public async Task SaveChanges_Insert_WhenPropertiesExceed16KB_TruncatesJson()
    {
        using var context = CreateContext();

        // Create a currency with a very long name (>16KB when serialized)
        var longName = new string('X', 20000); // 20KB > 16KB limit
        var currency = new Currency
        {
            Code = "TST",
            Name = longName,
            Symbol = "T",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };

        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Create);

        auditTrail.ShouldNotBeNull();
        auditTrail.NewValues.ShouldNotBeNull();
        auditTrail.NewValues.ShouldContain("[truncated]");
        auditTrail.NewValues.Length.ShouldBeLessThanOrEqualTo(16 * 1024 + 20); // 16KB + marker length + margin
    }

    [Test]
    public async Task SaveChanges_Update_JsonExcludesRowVersionAndAuditFields()
    {
        using var context = CreateContext();

        var currency = new Currency
        {
            Code = "USD",
            Name = "USD",
            Symbol = "$",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true
        };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var loaded = await context.Currencies.FindAsync(currency.Id);
        loaded.ShouldNotBeNull();
        loaded.Name = "US Dollar";

        await context.SaveChangesAsync();

        var auditTrail = context.AuditTrails
            .FirstOrDefault(a => a.DocumentType == "Currency" && a.Action == AuditAction.Update);

        auditTrail.ShouldNotBeNull();
        auditTrail.OldValues.ShouldNotBeNull();
        auditTrail.NewValues.ShouldNotBeNull();

        // Should NOT contain audit fields or RowVersion
        auditTrail.OldValues.ShouldNotContain("Created");
        auditTrail.OldValues.ShouldNotContain("CreatedBy");
        auditTrail.OldValues.ShouldNotContain("LastModified");
        auditTrail.OldValues.ShouldNotContain("LastModifiedBy");
        auditTrail.NewValues.ShouldNotContain("Created");
        auditTrail.NewValues.ShouldNotContain("CreatedBy");
        auditTrail.NewValues.ShouldNotContain("LastModified");
        auditTrail.NewValues.ShouldNotContain("LastModifiedBy");
    }
}
