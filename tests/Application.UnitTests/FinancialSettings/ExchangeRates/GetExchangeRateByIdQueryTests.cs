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
public class GetExchangeRateByIdQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetExchangeRateByIdQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetExchangeRateByIdQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ExistingId_ShouldReturnExchangeRate()
    {
        var entity = new Domain.FinancialSettings.Entities.ExchangeRate
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
        };

        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate> { entity }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<ExchangeRateDto?>(It.IsAny<Domain.FinancialSettings.Entities.ExchangeRate>()))
            .Returns(new ExchangeRateDto { Id = 1, Rate = 250.00m });

        var query = new GetExchangeRateByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
    }

    [Test]
    public async Task Handle_NonExistentId_ShouldReturnNull()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>().AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);

        var query = new GetExchangeRateByIdQuery { Id = 999 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeNull();
    }
}
