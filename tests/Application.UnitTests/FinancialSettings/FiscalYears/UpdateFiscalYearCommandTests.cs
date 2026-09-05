using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.FiscalYears;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.FiscalYears;

[TestFixture]
public class UpdateFiscalYearCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private UpdateFiscalYearCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateFiscalYearCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldUpdateFiscalYear()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "Old Name",
            YearNumber = 2027,
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31),
            Status = FiscalYearStatus.Draft,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateFiscalYearCommand
        {
            Id = 1,
            Name = "Updated Name",
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31),
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        entity.Name.ShouldBe("Updated Name");
    }

    [Test]
    public async Task Handle_NotDraft_ShouldReturnFailure()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = FiscalYearStatus.Open,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);

        var command = new UpdateFiscalYearCommand
        {
            Id = 1,
            Name = "Updated",
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31),
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("draft"));
    }

    [Test]
    public async Task Handle_RowVersionMismatch_ShouldReturnFailure()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            Status = FiscalYearStatus.Draft,
            RowVersion = [1, 2, 3]
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(entity);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);

        var command = new UpdateFiscalYearCommand
        {
            Id = 1,
            Name = "Updated",
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31),
            RowVersion = [4, 5, 6]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }

    [Test]
    public async Task Handle_NotFound_ShouldReturnFailure()
    {
        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((FiscalYear?)null);
        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);

        var command = new UpdateFiscalYearCommand
        {
            Id = 999,
            Name = "Updated",
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31),
            RowVersion = [1, 2, 3]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
