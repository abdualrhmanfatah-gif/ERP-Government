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
public class CreateFiscalYearCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CreateFiscalYearCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateFiscalYearCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldCreateFiscalYear()
    {
        var fiscalYears = new List<FiscalYear>().AsQueryable();
        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateFiscalYearCommand
        {
            Name = "السنة المالية 2027",
            YearNumber = 2027,
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.FiscalYears.Add(It.IsAny<FiscalYear>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DuplicateYearNumber_ShouldReturnFailure()
    {
        var fiscalYears = new List<FiscalYear>
        {
            new() { Id = 1, YearNumber = 2027, IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);

        var command = new CreateFiscalYearCommand
        {
            Name = "السنة المالية 2027",
            YearNumber = 2027,
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task Handle_OverlappingDates_ShouldReturnFailure()
    {
        var fiscalYears = new List<FiscalYear>
        {
            new()
            {
                Id = 1,
                YearNumber = 2026,
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                IsActive = true
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);

        var command = new CreateFiscalYearCommand
        {
            Name = "FY Overlap",
            YearNumber = 2027,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("overlap"));
    }
}
