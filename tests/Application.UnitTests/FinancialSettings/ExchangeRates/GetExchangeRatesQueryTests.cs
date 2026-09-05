using AutoMapper;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.ExchangeRates;

[TestFixture]
public class GetExchangeRatesQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetExchangeRatesQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetExchangeRatesQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnAllExchangeRates()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            },
            new()
            {
                Id = 2,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 6, 1),
                RateType = ExchangeRateType.Market,
                Rate = 260.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<ExchangeRateDto>>(It.IsAny<List<Domain.FinancialSettings.Entities.ExchangeRate>>()))
            .Returns(new List<ExchangeRateDto>
            {
                new() { Id = 1, Rate = 250.00m },
                new() { Id = 2, Rate = 260.00m }
            });

        var query = new GetExchangeRatesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_FilterByCurrencyId_ShouldReturnMatching()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<ExchangeRateDto>>(It.IsAny<List<Domain.FinancialSettings.Entities.ExchangeRate>>()))
            .Returns(new List<ExchangeRateDto> { new() { Id = 1, Rate = 250.00m } });

        var query = new GetExchangeRatesQuery { CurrencyId = 3 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task Handle_FilterByRateType_ShouldReturnMatching()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<ExchangeRateDto>>(It.IsAny<List<Domain.FinancialSettings.Entities.ExchangeRate>>()))
            .Returns(new List<ExchangeRateDto> { new() { Id = 1, Rate = 250.00m } });

        var query = new GetExchangeRatesQuery { RateType = ExchangeRateType.Official };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task Handle_FilterByDateRange_ShouldReturnMatching()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<ExchangeRateDto>>(It.IsAny<List<Domain.FinancialSettings.Entities.ExchangeRate>>()))
            .Returns(new List<ExchangeRateDto> { new() { Id = 1, Rate = 250.00m } });

        var query = new GetExchangeRatesQuery
        {
            FromDate = new DateOnly(2025, 12, 1),
            ToDate = new DateOnly(2026, 12, 31)
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task Handle_FilterByIsActive_ShouldReturnMatching()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>
        {
            new()
            {
                Id = 1,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 1, 1),
                RateType = ExchangeRateType.Official,
                Rate = 250.00m,
                IsActive = true,
                BaseCurrency = new Currency { Id = 1, Code = "YER" },
                Currency = new Currency { Id = 3, Code = "USD" }
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<ExchangeRateDto>>(It.IsAny<List<Domain.FinancialSettings.Entities.ExchangeRate>>()))
            .Returns(new List<ExchangeRateDto> { new() { Id = 1, Rate = 250.00m } });

        var query = new GetExchangeRatesQuery { IsActive = true };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }
}
