using AutoMapper;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Queries.Currencies;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.Currencies;

[TestFixture]
public class GetCurrenciesQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetCurrenciesQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCurrenciesQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_NoFilter_ShouldReturnAllOrderedByCode()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 2, Code = "USD", Name = "دولار", IsActive = true },
            new() { Id = 1, Code = "EUR", Name = "يورو", IsActive = true },
            new() { Id = 3, Code = "GBP", Name = "جنيه", IsActive = true }
        };

        var dtos = new List<CurrencyDto>
        {
            new() { Id = 1, Code = "EUR", Name = "يورو", IsActive = true },
            new() { Id = 2, Code = "USD", Name = "دولار", IsActive = true },
            new() { Id = 3, Code = "GBP", Name = "جنيه", IsActive = true }
        };

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        _mapperMock.Setup(x => x.Map<List<CurrencyDto>>(It.IsAny<List<Currency>>()))
            .Returns(dtos);

        var query = new GetCurrenciesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(3);
        result[0].Code.ShouldBe("EUR");
        result[1].Code.ShouldBe("USD");
        result[2].Code.ShouldBe("GBP");
    }

    [Test]
    public async Task Handle_FilterActive_ShouldReturnOnlyActive()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "EUR", IsActive = true },
            new() { Id = 2, Code = "USD", IsActive = false }
        };

        var dtos = new List<CurrencyDto>
        {
            new() { Id = 1, Code = "EUR", IsActive = true }
        };

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        _mapperMock.Setup(x => x.Map<List<CurrencyDto>>(It.IsAny<List<Currency>>()))
            .Returns(dtos);

        var query = new GetCurrenciesQuery { IsActive = true };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_FilterInactive_ShouldReturnOnlyInactive()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "EUR", IsActive = true },
            new() { Id = 2, Code = "USD", IsActive = false }
        };

        var dtos = new List<CurrencyDto>
        {
            new() { Id = 2, Code = "USD", IsActive = false }
        };

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.AsQueryable().BuildMockForAsync().Object);

        _mapperMock.Setup(x => x.Map<List<CurrencyDto>>(It.IsAny<List<Currency>>()))
            .Returns(dtos);

        var query = new GetCurrenciesQuery { IsActive = false };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
    {
        var currencies = new List<Currency>().AsQueryable();

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);

        _mapperMock.Setup(x => x.Map<List<CurrencyDto>>(It.IsAny<List<Currency>>()))
            .Returns(new List<CurrencyDto>());

        var query = new GetCurrenciesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
