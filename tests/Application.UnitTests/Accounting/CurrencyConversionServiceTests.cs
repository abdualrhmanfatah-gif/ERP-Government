using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Accounting.Common.Services;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class CurrencyConversionServiceTests
{
    private Mock<IExchangeRateResolver> _resolverMock = null!;
    private CurrencyConversionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _resolverMock = new Mock<IExchangeRateResolver>();
        _service = new CurrencyConversionService(_resolverMock.Object);
    }

    [Test]
    public async Task ConvertAtPostAsync_ForeignLine_ComputesBaseDebitAndCredit()
    {
        _resolverMock.Setup(x => x.GetEffectiveRateAsync(1, 3, new DateOnly(2026, 1, 15), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveRate(1.5m, new DateOnly(2026, 1, 1), false));

        var line = new JournalEntryLine { CurrencyId = 3, Debit = 100m, Credit = 0m };

        var result = await _service.ConvertAtPostAsync(line, 1, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.IsBaseCurrency.ShouldBeFalse();
        result.Rate.ShouldBe(1.5m);
        result.BaseDebit.ShouldBe(150.00m);
        result.BaseCredit.ShouldBe(0m);
        result.ResolvedRateDate.ShouldBe(new DateOnly(2026, 1, 1));
    }

    [Test]
    public async Task ConvertAtPostAsync_Rounding_RoundsToTwoDecimals()
    {
        _resolverMock.Setup(x => x.GetEffectiveRateAsync(1, 3, It.IsAny<DateOnly>(), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveRate(1.333m, new DateOnly(2026, 1, 1), false));

        var line = new JournalEntryLine { CurrencyId = 3, Debit = 10m, Credit = 0m };

        var result = await _service.ConvertAtPostAsync(line, 1, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.BaseDebit.ShouldBe(13.33m); // 10 × 1.333 = 13.33
    }

    [Test]
    public async Task ConvertAtPostAsync_BaseCurrencyLine_UsesRateOneAndIsBase()
    {
        _resolverMock.Setup(x => x.GetEffectiveRateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<ExchangeRateType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveRate(1m, new DateOnly(2026, 1, 1), true));

        var line = new JournalEntryLine { CurrencyId = 1, Debit = 250.00m, Credit = 0m };

        var result = await _service.ConvertAtPostAsync(line, 1, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.IsBaseCurrency.ShouldBeTrue();
        result.Rate.ShouldBe(1m);
        result.BaseDebit.ShouldBe(250.00m);
        result.BaseCredit.ShouldBe(0m);
    }

    [Test]
    public async Task ConvertAtPostAsync_NoRate_SignalsNoRate()
    {
        _resolverMock.Setup(x => x.GetEffectiveRateAsync(1, 3, It.IsAny<DateOnly>(), ExchangeRateType.Official, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EffectiveRate?)null);

        var line = new JournalEntryLine { CurrencyId = 3, Debit = 100m, Credit = 0m };

        var result = await _service.ConvertAtPostAsync(line, 1, new DateOnly(2026, 1, 15), ExchangeRateType.Official, CancellationToken.None);

        result.IsBaseCurrency.ShouldBeFalse();
        result.Rate.ShouldBeNull();
    }
}
