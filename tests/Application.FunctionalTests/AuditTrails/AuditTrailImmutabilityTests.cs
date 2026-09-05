using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.AuditTrails;

[TestFixture]
public class AuditTrailImmutabilityTests : TestBase
{
    [Test]
    public async Task AuditTrail_Update_ShouldBeBlockedByInterceptor()
    {
        // Arrange — create an audit trail record via interceptor
        await TestApp.RunAsAdministratorAsync();

        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed a Currency to trigger interceptor audit
            ctx.Currencies.Add(new Domain.FinancialSettings.Entities.Currency
            {
                Code = "TST", Name = "Test", Symbol = "T",
                DecimalPlaces = 2, RoundingPrecision = 0.01m, IsActive = true
            });
            await ctx.SaveChangesAsync();
        }

        // Act — attempt to modify the audit trail
        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var auditTrail = await ctx.AuditTrails
                .FirstOrDefaultAsync(a => a.DocumentType == "Currency");

            auditTrail.ShouldNotBeNull();

            auditTrail.ChangeSummary = "TAMPERED";

            // Assert — should throw
            var ex = Should.Throw<InvalidOperationException>(() => ctx.SaveChanges());
            ex.Message.ShouldContain("IImmutableEntity");
        }
    }

    [Test]
    public async Task AuditTrail_Delete_ShouldBeBlockedByInterceptor()
    {
        // Arrange
        await TestApp.RunAsAdministratorAsync();

        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ctx.Currencies.Add(new Domain.FinancialSettings.Entities.Currency
            {
                Code = "TST2", Name = "Test2", Symbol = "T2",
                DecimalPlaces = 2, RoundingPrecision = 0.01m, IsActive = true
            });
            await ctx.SaveChangesAsync();
        }

        // Act
        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var auditTrail = await ctx.AuditTrails
                .FirstOrDefaultAsync(a => a.DocumentType == "Currency");

            auditTrail.ShouldNotBeNull();
            ctx.AuditTrails.Remove(auditTrail);

            var ex = Should.Throw<InvalidOperationException>(() => ctx.SaveChanges());
            ex.Message.ShouldContain("IImmutableEntity");
        }
    }

    [Test]
    public async Task SecurityAuditLog_Update_ShouldBeBlockedByInterceptor()
    {
        // Arrange — seed a SecurityAuditLog directly
        await TestApp.RunAsAdministratorAsync();

        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            ctx.SecurityAuditLogs.Add(new SecurityAuditLog
            {
                EventCategory = "Test",
                Action = "Login",
                UserId = 1,
                Timestamp = DateTimeOffset.UtcNow,
                Success = true
            });
            await ctx.SaveChangesAsync();
        }

        // Act
        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var log = await ctx.SecurityAuditLogs.FirstAsync();
            log.EntityName = "TAMPERED";

            var ex = Should.Throw<InvalidOperationException>(() => ctx.SaveChanges());
            ex.Message.ShouldContain("IImmutableEntity");
        }
    }
}
