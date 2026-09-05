using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.Currencies;
using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.Currencies;

[TestFixture]
public class ActivateCurrencyCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private ActivateCurrencyCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ActivateCurrencyCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldActivateAndReturnSuccess()
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new ActivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        currency.IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<Currency>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((Currency?)null);

        _contextMock.Setup(x => x.Currencies).Returns(mockSet.Object);

        var command = new ActivateCurrencyCommand
        {
            Id = 999,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Handle_AlreadyActive_ShouldReturnFailure()
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

        var command = new ActivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already active"));
    }

    [Test]
    public async Task Handle_RowVersionMismatch_ShouldReturnFailure()
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

        var command = new ActivateCurrencyCommand
        {
            Id = 1,
            RowVersion = [9, 9, 9] // mismatched
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }
}
