using ERP_Government.Application.Accounting.Common.Services;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class ExchangeRateResolverTests
{
    private ApplicationDbContext _context = null!;
    private ExchangeRateResolver _resolver = null!;

    private DbContextOptions<ApplicationDbContext> NewDb() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [SetUp]
    public void Setup()
    {
        _context = new ApplicationDbContext(NewDb());
        _resolver = new ExchangeRateResolver(_context);
    }

    [TearDown]
    public void Teardown() => _context.Dispose();

    [Test]
    public async Task GetEffectiveRateAsync_SelectsNearestOnOrBeforeDate()
    {
        _context.ExchangeRates.AddRange(
            new ExchangeRate { Id = 1, BaseCurrencyId = 1, CurrencyId = 3, RateDate = new DateOnly(2026, 1, 1), RateType = ExchangeRateType.Official, Rate = 250.00m, IsActive = true },
            new ExchangeRate { Id = 2, BaseCurrencyId = 1, CurrencyId = 3, RateDate = new DateOnly(2026, 2, 1), RateType = ExchangeRateType.Official, Rate = 260.00m, IsActive = true });
        await _context.SaveChangesAsync();

        var result = await _resolver.GetEffectiveRateAsync(1, 3, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Rate.ShouldBe(250.00m); // nearest on-or-before Jan 15 is Jan 1 (not Feb 1)
        result.ResolvedRateDate.ShouldBe(new DateOnly(2026, 1, 1));
        result.IsDefaultBase.ShouldBeFalse();
    }

    [Test]
    public async Task GetEffectiveRateAsync_NoPriorRate_ReturnsNull()
    {
        _context.ExchangeRates.Add(new ExchangeRate { Id = 1, BaseCurrencyId = 1, CurrencyId = 3, RateDate = new DateOnly(2026, 2, 1), RateType = ExchangeRateType.Official, Rate = 260.00m, IsActive = true });
        await _context.SaveChangesAsync();

        // Date before any rate exists → no applicable prior rate → null
        var result = await _resolver.GetEffectiveRateAsync(1, 3, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Test]
    public async Task GetEffectiveRateAsync_NoRatesAtAll_ReturnsNull()
    {
        var result = await _resolver.GetEffectiveRateAsync(1, 3, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Test]
    public async Task GetEffectiveRateAsync_BaseCurrency_ReturnsDefaultBaseRateOne()
    {
        var result = await _resolver.GetEffectiveRateAsync(1, 1, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Rate.ShouldBe(1m);
        result.IsDefaultBase.ShouldBeTrue();
    }
}
