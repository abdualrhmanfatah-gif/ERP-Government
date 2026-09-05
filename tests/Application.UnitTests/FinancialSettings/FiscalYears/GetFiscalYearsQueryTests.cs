using AutoMapper;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Queries.FiscalYears;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.FiscalYears;

[TestFixture]
public class GetFiscalYearsQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetFiscalYearsQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFiscalYearsQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnAllFiscalYears()
    {
        var fiscalYears = new List<FiscalYear>
        {
            new() { Id = 1, YearNumber = 2026, IsActive = true },
            new() { Id = 2, YearNumber = 2027, IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<FiscalYearDto>>(It.IsAny<List<FiscalYear>>()))
            .Returns(new List<FiscalYearDto>
            {
                new() { Id = 1, YearNumber = 2026 },
                new() { Id = 2, YearNumber = 2027 }
            });

        var query = new GetFiscalYearsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_FilterByIsActive_ShouldReturnMatching()
    {
        var fiscalYears = new List<FiscalYear>
        {
            new() { Id = 1, YearNumber = 2026, IsActive = true }
        }.AsQueryable();

        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<FiscalYearDto>>(It.IsAny<List<FiscalYear>>()))
            .Returns(new List<FiscalYearDto> { new() { Id = 1, YearNumber = 2026 } });

        var query = new GetFiscalYearsQuery { IsActive = true };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }
}
