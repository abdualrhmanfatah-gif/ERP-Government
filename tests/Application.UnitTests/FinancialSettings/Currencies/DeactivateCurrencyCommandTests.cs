using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.Currencies;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.Currencies;

[TestFixture]
public class DeactivateCurrencyCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private DeactivateCurrencyCommandHandler _handler = null!;
    private DeactivateCurrencyCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeactivateCurrencyCommandHandler(_contextMock.Object);
        _validator = new DeactivateCurrencyCommandValidator();
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldDeactivateAndReturnSuccess()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = true,
            IsBase = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        // No active exchange rates
        var exchangeRates = new List<ExchangeRate>().AsQueryable();
        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(exchangeRates.BuildMockForAsync().Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeactivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        currency.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Currency?)null);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new DeactivateCurrencyCommand
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
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = false,
            IsBase = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new DeactivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already inactive"));
    }

    [Test]
    public async Task Handle_BaseCurrency_ShouldReturnFailure()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "YER",
            IsActive = true,
            IsBase = true,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new DeactivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("base currency"));
    }

    [Test]
    public async Task Handle_ActiveExchangeRates_ShouldReturnFailure()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = true,
            IsBase = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var exchangeRates = new List<ExchangeRate>
        {
            new() { Id = 1, CurrencyId = 1, IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.ExchangeRates)
            .Returns(exchangeRates.BuildMockForAsync().Object);

        var command = new DeactivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("exchange rates"));
    }

    [Test]
    public async Task Handle_RowVersionMismatch_ShouldReturnFailure()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = true,
            IsBase = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new DeactivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [9, 9, 9] // mismatched
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }

    [Test]
    public async Task Validator_IdZero_ShouldHaveError()
    {
        var command = new DeactivateCurrencyCommand { Id = 0 };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id");
    }
}
