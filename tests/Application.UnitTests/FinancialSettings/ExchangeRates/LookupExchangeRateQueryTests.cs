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
public class LookupExchangeRateQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private LookupExchangeRateQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new LookupExchangeRateQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ExistingRate_ShouldReturnMostRecent()
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
                IsActive = true
            },
            new()
            {
                Id = 2,
                BaseCurrencyId = 1,
                CurrencyId = 3,
                RateDate = new DateOnly(2026, 6, 1),
                RateType = ExchangeRateType.Official,
                Rate = 260.00m,
                IsActive = true
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);

        var query = new LookupExchangeRateQuery
        {
            BaseCurrencyId = 1,
            CurrencyId = 3,
            RateType = ExchangeRateType.Official,
            Date = new DateOnly(2026, 6, 15)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Rate.ShouldBe(260.00m);
        result.RateDate.ShouldBe(new DateOnly(2026, 6, 1));
    }

    [Test]
    public async Task Handle_NoMatchingRate_ShouldReturnNull()
    {
        var rates = new List<Domain.FinancialSettings.Entities.ExchangeRate>().AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(rates.BuildMockForAsync().Object);

        var query = new LookupExchangeRateQuery
        {
            BaseCurrencyId = 1,
            CurrencyId = 3,
            RateType = ExchangeRateType.Official,
            Date = new DateOnly(2025, 12, 31)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeNull();
    }
}
