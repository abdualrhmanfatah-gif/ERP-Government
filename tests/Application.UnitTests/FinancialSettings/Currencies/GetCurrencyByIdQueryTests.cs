using AutoMapper;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Queries.Currencies;
using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.Currencies;

[TestFixture]
public class GetCurrencyByIdQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetCurrencyByIdQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCurrencyByIdQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ExistingId_ShouldReturnCurrency()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = false,
            IsActive = true
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var dto = new CurrencyDto
        {
            Id = 1,
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = false,
            IsActive = true
        };

        _mapperMock.Setup(x => x.Map<CurrencyDto>(currency)).Returns(dto);

        var query = new GetCurrencyByIdQuery { Id = 1 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Code.ShouldBe("EUR");
        result.Name.ShouldBe("يورو");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnNull()
    {
        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Currency?)null);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var query = new GetCurrencyByIdQuery { Id = 999 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeNull();
    }
}
