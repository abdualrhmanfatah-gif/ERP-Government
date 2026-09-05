using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.FiscalYears;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.FiscalYears;

[TestFixture]
public class CloseFiscalYearCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CloseFiscalYearCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CloseFiscalYearCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldCloseFiscalYear()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = FiscalYearStatus.Open,
            IsClosed = false
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CloseFiscalYearCommand { Id = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Status.ShouldBe(FiscalYearStatus.HardClosed);
        entity.IsClosed.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_NotOpen_ShouldReturnFailure()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = FiscalYearStatus.Draft
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);

        var command = new CloseFiscalYearCommand { Id = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("open"));
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((FiscalYear?)null);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);

        var command = new CloseFiscalYearCommand { Id = 999 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
