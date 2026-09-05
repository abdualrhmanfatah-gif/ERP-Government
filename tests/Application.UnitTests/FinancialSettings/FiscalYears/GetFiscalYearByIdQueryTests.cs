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
public class GetFiscalYearByIdQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetFiscalYearByIdQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFiscalYearByIdQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ExistingId_ShouldReturnFiscalYear()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            Name = "FY 2027",
            YearNumber = 2027,
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2027, 12, 31)
        };

        var fiscalYears = new List<FiscalYear> { entity }.AsQueryable();
        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<FiscalYearDto?>(It.IsAny<FiscalYear>()))
            .Returns(new FiscalYearDto { Id = 1, Name = "FY 2027", YearNumber = 2027 });

        var query = new GetFiscalYearByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
    }

    [Test]
    public async Task Handle_NonExistentId_ShouldReturnNull()
    {
        var fiscalYears = new List<FiscalYear>().AsQueryable();
        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);

        var query = new GetFiscalYearByIdQuery { Id = 999 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeNull();
    }
}
