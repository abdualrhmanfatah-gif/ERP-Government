using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

using ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequests;

[TestFixture]
public class GetDisbursementRequestsTests
{
    private ApplicationDbContext _dbContext = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private async Task SeedDataAsync()
    {
        _dbContext.DisbursementRequests.AddRange(
            new DisbursementRequest
            {
                Id = 1,
                RequestNumber = "DR-000001",
                RequestedById = 1,
                RequestedByName = "User A",
                BeneficiaryName = "Vendor A",
                RequestedAmount = 8000m,
                CurrencyId = 1,
                Purpose = "Procurement",
                FinancialYearId = 1,
                RequestDate = new DateOnly(2025, 1, 15),
                Status = DisbursementRequestStatus.Approved,
                RowVersion = [1, 2, 3]
            },
            new DisbursementRequest
            {
                Id = 2,
                RequestNumber = "DR-000002",
                RequestedById = 2,
                RequestedByName = "User B",
                BeneficiaryName = "Vendor B",
                RequestedAmount = 4500m,
                CurrencyId = 1,
                Purpose = "Services",
                FinancialYearId = 1,
                RequestDate = new DateOnly(2025, 2, 20),
                Status = DisbursementRequestStatus.Draft,
                RowVersion = [1, 2, 3]
            },
            new DisbursementRequest
            {
                Id = 3,
                RequestNumber = "DR-000003",
                RequestedById = 1,
                RequestedByName = "User A",
                BeneficiaryName = "Vendor C",
                RequestedAmount = 8000m,
                CurrencyId = 2,
                Purpose = "Travel",
                FinancialYearId = 2,
                RequestDate = new DateOnly(2025, 3, 10),
                Status = DisbursementRequestStatus.Cancelled,
                RowVersion = [1, 2, 3]
            });

        await _dbContext.SaveChangesAsync();
    }

    // ─── T047: Filter by status ───────────────────────────────────

    [Test]
    public async Task Handle_FilterByStatus_ShouldReturnCorrectSubset()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(
                Status: DisbursementRequestStatus.Approved),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(1);
        result[0].RequestNumber.ShouldBe("DR-000001");
        result[0].Status.ShouldBe(DisbursementRequestStatus.Approved);
    }

    [Test]
    public async Task Handle_FilterByRequestedById_ShouldReturnMatchingRequests()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(RequestedById: 1),
            CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(r => r.RequestedById == 1);
    }

    [Test]
    public async Task Handle_NoFilters_ShouldReturnAllOrderedByDateDesc()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(),
            CancellationToken.None);

        result.Count.ShouldBe(3);
        result[0].RequestDate.ShouldBe(new DateOnly(2025, 3, 10));
        result[1].RequestDate.ShouldBe(new DateOnly(2025, 2, 20));
        result[2].RequestDate.ShouldBe(new DateOnly(2025, 1, 15));
    }

    // ─── T048: DTO shape validation ───────────────────────────────

    [Test]
    public async Task Handle_ShouldReturnCorrectDtoFields()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(),
            CancellationToken.None);

        var dto = result.First(r => r.Id == 1);
        dto.RequestNumber.ShouldBe("DR-000001");
        dto.RequestedByName.ShouldBe("User A");
        dto.BeneficiaryName.ShouldBe("Vendor A");
        dto.RequestedAmount.ShouldBe(8000m);
        dto.CurrencyId.ShouldBe(1);
        dto.Purpose.ShouldBe("Procurement");
        dto.FinancialYearId.ShouldBe(1);
    }

    // ─── T049: Compute totals correctly ───────────────────────────

    [Test]
    public async Task Handle_ShouldReturnRequestedAmountFromEntity()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(
                Status: DisbursementRequestStatus.Approved),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].RequestedAmount.ShouldBe(8000m);
    }
}
