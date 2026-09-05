using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.ExchangeRates;

[TestFixture]
public class CreateExchangeRateCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CreateExchangeRateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateExchangeRateCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldCreateExchangeRate()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "YER", IsActive = true },
            new() { Id = 3, Code = "USD", IsActive = true }
        }.AsQueryable();

        var exchangeRates = new List<Domain.FinancialSettings.Entities.ExchangeRate>().AsQueryable();

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(exchangeRates.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateExchangeRateCommand
        {
            BaseCurrencyId = 1,
            CurrencyId = 3,
            RateDate = new DateOnly(2026, 1, 1),
            RateType = ExchangeRateType.Official,
            Rate = 250.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ExchangeRates.Add(It.IsAny<Domain.FinancialSettings.Entities.ExchangeRate>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SameCurrencyPair_ShouldReturnFailure()
    {
        var command = new CreateExchangeRateCommand
        {
            BaseCurrencyId = 1,
            CurrencyId = 1,
            RateDate = new DateOnly(2026, 1, 1),
            RateType = ExchangeRateType.Official,
            Rate = 250.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("same"));
    }

    [Test]
    public async Task Handle_DuplicateCombination_ShouldReturnFailure()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "YER", IsActive = true },
            new() { Id = 3, Code = "USD", IsActive = true }
        }.AsQueryable();

        var exchangeRates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(exchangeRates.BuildMockForAsync().Object);

        var command = new CreateExchangeRateCommand
        {
            BaseCurrencyId = 1,
            CurrencyId = 3,
            RateDate = new DateOnly(2026, 1, 1),
            RateType = ExchangeRateType.Official,
            Rate = 260.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task Handle_NonExistentCurrency_ShouldReturnFailure()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "YER", IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);

        var command = new CreateExchangeRateCommand
        {
            BaseCurrencyId = 1,
            CurrencyId = 999,
            RateDate = new DateOnly(2026, 1, 1),
            RateType = ExchangeRateType.Official,
            Rate = 250.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
