using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.ExchangeRates;

[TestFixture]
public class DeactivateExchangeRateCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private DeactivateExchangeRateCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeactivateExchangeRateCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldDeactivateExchangeRate()
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

        var command = new DeactivateExchangeRateCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<Domain.FinancialSettings.Entities.ExchangeRate>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Domain.FinancialSettings.Entities.ExchangeRate?)null);
        _contextMock.Setup(x => x.ExchangeRates).Returns(mockSet.Object);

        var command = new DeactivateExchangeRateCommand
        {
            Id = 999,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Handle_AlreadyInactive_ShouldReturnFailure()
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

        var command = new DeactivateExchangeRateCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already inactive"));
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

        var command = new DeactivateExchangeRateCommand
        {
            Id = 1,
            RowVersion = [4, 5, 6]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }
}
