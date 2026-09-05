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
public class CreateCurrencyCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CreateCurrencyCommandHandler _handler = null!;
    private CreateCurrencyCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateCurrencyCommandHandler(_contextMock.Object);
        _validator = new CreateCurrencyCommandValidator();
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnSuccess()
    {
        var currencies = new List<Currency>().AsQueryable();
        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCurrencyCommand
        {
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.Currencies.Add(It.IsAny<Currency>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DuplicateCode_ShouldReturnFailure()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "EUR", Name = "يورو", IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.Currencies)
            .Returns(currencies.BuildMockForAsync().Object);

        var command = new CreateCurrencyCommand
        {
            Code = "EUR",
            Name = "يورو جديد",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task Handle_IsBaseTrue_ShouldUnsetExistingBase()
    {
        var existingBase = new Currency { Id = 1, Code = "YER", IsBase = true, IsActive = true };
        var currencies = new List<Currency> { existingBase }.AsQueryable();

        var mockSet = currencies.BuildMockForAsync();
        _contextMock.Setup(x => x.Currencies)
            .Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCurrencyCommand
        {
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = true
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        existingBase.IsBase.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_IsBaseFalse_ShouldNotUnsetExistingBase()
    {
        var existingBase = new Currency { Id = 1, Code = "YER", IsBase = true, IsActive = true };
        var currencies = new List<Currency> { existingBase }.AsQueryable();

        var mockSet = currencies.BuildMockForAsync();
        _contextMock.Setup(x => x.Currencies)
            .Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateCurrencyCommand
        {
            Code = "EUR",
            Name = "يورو",
            Symbol = "€",
            DecimalPlaces = 2,
            RoundingPrecision = 0.01m,
            IsBase = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        existingBase.IsBase.ShouldBeTrue(); // unchanged
    }

    [Test]
    public async Task Validator_EmptyCode_ShouldHaveError()
    {
        var command = new CreateCurrencyCommand { Code = "", Name = "test", Symbol = "X" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Code");
    }

    [Test]
    public async Task Validator_InvalidIso4217_ShouldHaveError()
    {
        var command = new CreateCurrencyCommand { Code = "INVALID", Name = "test", Symbol = "X" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Code");
    }

    [Test]
    public async Task Validator_EmptyName_ShouldHaveError()
    {
        var command = new CreateCurrencyCommand { Code = "EUR", Name = "", Symbol = "€" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validator_DecimalPlacesOutOfRange_ShouldHaveError()
    {
        var command = new CreateCurrencyCommand { Code = "EUR", Name = "test", Symbol = "€", DecimalPlaces = 7 };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DecimalPlaces");
    }
}
