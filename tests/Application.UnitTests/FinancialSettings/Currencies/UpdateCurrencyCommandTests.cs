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
public class UpdateCurrencyCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private UpdateCurrencyCommandHandler _handler = null!;
    private UpdateCurrencyCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateCurrencyCommandHandler(_contextMock.Object);
        _validator = new UpdateCurrencyCommandValidator();
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldUpdateAndReturnSuccess()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsActive = true,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateCurrencyCommand
        {
            Id = 1,
            Name = "يورو (محدث)",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        currency.Name.ShouldBe("يورو (محدث)");
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Currency?)null);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new UpdateCurrencyCommand
        {
            Id = 999,
            Name = "test",
            Symbol = "X",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Handle_InactiveCurrency_ShouldReturnFailure()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = false,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new UpdateCurrencyCommand
        {
            Id = 1,
            Name = "test",
            Symbol = "X",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("inactive"));
    }

    [Test]
    public async Task Handle_RowVersionMismatch_ShouldReturnFailure()
    {
        var currency = new Currency
        {
            Id = 1,
            Code = "EUR",
            IsActive = true,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(currency);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new UpdateCurrencyCommand
        {
            Id = 1,
            Name = "test",
            Symbol = "X",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            RowVersion = [9, 9, 9] // mismatched
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }

    [Test]
    public async Task Validator_IdZero_ShouldHaveError()
    {
        var command = new UpdateCurrencyCommand { Id = 0, Name = "test", Symbol = "X", DecimalPlaces = 2, RoundingPrecision = 0.01m };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id");
    }

    [Test]
    public async Task Validator_EmptyName_ShouldHaveError()
    {
        var command = new UpdateCurrencyCommand { Id = 1, Name = "", Symbol = "X", DecimalPlaces = 2, RoundingPrecision = 0.01m };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }
}
