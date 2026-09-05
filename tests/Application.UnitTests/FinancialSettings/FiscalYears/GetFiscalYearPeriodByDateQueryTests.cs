using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Queries.FiscalYears;
using ERP_Government.Application.UnitTests.FinancialSettings;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings.FiscalYears;

[TestFixture]
public class GetFiscalYearPeriodByDateQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private GetFiscalYearPeriodByDateQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetFiscalYearPeriodByDateQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ExistingFiscalYear_ShouldReturnResult()
    {
        var fiscalYears = new List<FiscalYear>
        {
            new()
            {
                Id = 1,
                Name = "FY 2026",
                YearNumber = 2026,
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                IsActive = true
            }
        }.AsQueryable();

        var periods = new List<FiscalPeriod>
        {
            new()
            {
                Id = 1,
                FiscalYearId = 1,
                PeriodNumber = 6,
                Name = "يونيو",
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30)
            }
        }.AsQueryable();

        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.FiscalPeriods)
            .Returns(periods.BuildMockForAsync().Object);

        var query = new GetFiscalYearPeriodByDateQuery
        {
            Date = new DateTime(2026, 6, 15)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.FiscalYearId.ShouldBe(1);
        result.FiscalYearName.ShouldBe("FY 2026");
    }

    [Test]
    public async Task Handle_NoMatchingFiscalYear_ShouldReturnNull()
    {
        var fiscalYears = new List<FiscalYear>().AsQueryable();
        _contextMock.Setup(x => x.FiscalYears)
            .Returns(fiscalYears.BuildMockForAsync().Object);

        var query = new GetFiscalYearPeriodByDateQuery
        {
            Date = new DateTime(2025, 6, 15)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeNull();
    }
}
