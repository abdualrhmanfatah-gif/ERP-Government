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
public class UpdateExchangeRateCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private UpdateExchangeRateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateExchangeRateCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldUpdateExchangeRate()
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
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Domain.FinancialSettings.Entities.ExchangeRate>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.ExchangeRates).Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 260.00m,
            RateType = ExchangeRateType.Market,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Rate.ShouldBe(260.00m);
        entity.RateType.ShouldBe(ExchangeRateType.Market);
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<Domain.FinancialSettings.Entities.ExchangeRate>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Domain.FinancialSettings.Entities.ExchangeRate?)null);
        _contextMock.Setup(x => x.ExchangeRates).Returns(mockSet.Object);

        var command = new UpdateExchangeRateCommand
        {
            Id = 999,
            Rate = 260.00m,
            RateType = ExchangeRateType.Market,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Handle_InactiveRate_ShouldReturnFailure()
    {
        var entity = new Domain.FinancialSettings.Entities.ExchangeRate
        {
            Id = 1,
            BaseCurrencyId = 1,
            CurrencyId = 3,
            RateDate = new DateOnly(2026, 1, 1),
            RateType = ExchangeRateType.Official,
            Rate = 250.00m,
            IsActive = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Domain.FinancialSettings.Entities.ExchangeRate>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.ExchangeRates).Returns(mockSet.Object);

        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 260.00m,
            RateType = ExchangeRateType.Market,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("inactive"));
    }

    [Test]
    public async Task Handle_RowVersionMismatch_ShouldReturnFailure()
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
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Domain.FinancialSettings.Entities.ExchangeRate>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.ExchangeRates).Returns(mockSet.Object);

        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 260.00m,
            RateType = ExchangeRateType.Market,
            RowVersion = [4, 5, 6]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }
}
