using ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.ExchangeRates;

[TestFixture]
public class ExchangeRateAuditTrailTests : TestBase
{
    [Test]
    public async Task CreateExchangeRate_ShouldProduceAuditTrailViaInterceptor()
    {
        // Arrange — seed currencies first
        await TestApp.RunAsAdministratorAsync();

        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            ctx.Currencies.AddRange(
                new Domain.FinancialSettings.Entities.Currency
                {
                    Code = "USD", Name = "US Dollar", Symbol = "$",
                    DecimalPlaces = 2, RoundingPrecision = 0.01m, IsActive = true
                },
                new Domain.FinancialSettings.Entities.Currency
                {
                    Code = "SYP", Name = "Syrian Pound", Symbol = "£S",
                    DecimalPlaces = 2, RoundingPrecision = 0.01m, IsActive = true
                }
            );
            await ctx.SaveChangesAsync();
        }

        var beforeCount = await TestApp.CountAsync<AuditTrail>();

        // Act
        var command = new CreateExchangeRateCommand
        {
            BaseCurrencyId = 1,
            CurrencyId = 2,
            RateDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RateType = ExchangeRateType.Official,
            Rate = 1500.00m
        };

        var result = await TestApp.SendAsync(command);

        // Assert
        result.Succeeded.ShouldBeTrue();

        var afterCount = await TestApp.CountAsync<AuditTrail>();
        afterCount.ShouldBeGreaterThan(beforeCount);

        // Verify audit trail was created by interceptor (not manual logging)
        using var scope2 = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var auditTrail = await context.AuditTrails
            .Where(a => a.DocumentType == "ExchangeRate" && a.Action == AuditAction.Create)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        auditTrail.ShouldNotBeNull();
        auditTrail.NewValues.ShouldNotBeNull();
        auditTrail.Success.ShouldBeTrue();
        auditTrail.EventCategory.ShouldBe("EntityChange");
    }
}
