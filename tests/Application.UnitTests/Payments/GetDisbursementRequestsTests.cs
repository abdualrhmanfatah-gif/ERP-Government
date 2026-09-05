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
        var po1 = new PaymentOrder
        {
            Id = 1,
            PaymentOrderNumber = "PO-001",
            Status = PaymentOrderStatus.Approved,
            AmountGross = 10000m,
            DeductionAmount = 2000m,
            FundId = 1,
            BeneficiaryName = "Vendor A",
            RowVersion = [1, 2, 3]
        };
        var po2 = new PaymentOrder
        {
            Id = 2,
            PaymentOrderNumber = "PO-002",
            Status = PaymentOrderStatus.Approved,
            AmountGross = 5000m,
            DeductionAmount = 500m,
            FundId = 2,
            BeneficiaryName = "Vendor B",
            RowVersion = [1, 2, 3]
        };
        var po3 = new PaymentOrder
        {
            Id = 3,
            PaymentOrderNumber = "PO-003",
            Status = PaymentOrderStatus.Draft,
            AmountGross = 8000m,
            DeductionAmount = 0m,
            FundId = 1,
            BeneficiaryName = "Vendor C",
            RowVersion = [1, 2, 3]
        };

        _dbContext.PaymentOrders.AddRange(po1, po2, po3);

        _dbContext.DisbursementRequests.AddRange(
            new DisbursementRequest
            {
                Id = 1,
                RequestNumber = "DR-000001",
                PaymentOrderId = 1,
                RequestedById = 1,
                RequestDate = new DateOnly(2025, 1, 15),
                Status = DisbursementRequestStatus.Approved,
                HasWarning = false,
                RowVersion = [1, 2, 3]
            },
            new DisbursementRequest
            {
                Id = 2,
                RequestNumber = "DR-000002",
                PaymentOrderId = 2,
                RequestedById = 2,
                RequestDate = new DateOnly(2025, 2, 20),
                Status = DisbursementRequestStatus.Draft,
                HasWarning = false,
                RowVersion = [1, 2, 3]
            },
            new DisbursementRequest
            {
                Id = 3,
                RequestNumber = "DR-000003",
                PaymentOrderId = 3,
                RequestedById = 1,
                RequestDate = new DateOnly(2025, 3, 10),
                Status = DisbursementRequestStatus.Cancelled,
                HasWarning = true,
                RowVersion = [1, 2, 3]
            });

        await _dbContext.SaveChangesAsync();
    }

    // ─── T047: Filter by status, fund, period ─────────────────────

    [Test]
    public async Task Handle_FilterByStatus_Fund_Period_ShouldReturnCorrectSubset()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(
                Status: DisbursementRequestStatus.Approved,
                FundId: 1,
                FromDate: new DateOnly(2025, 1, 1),
                ToDate: new DateOnly(2025, 12, 31)),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(1);
        result[0].RequestNumber.ShouldBe("DR-000001");
        result[0].Status.ShouldBe(DisbursementRequestStatus.Approved);
    }

    [Test]
    public async Task Handle_FilterByPaymentOrderId_ShouldReturnMatchingRequest()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(PaymentOrderId: 2),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(2);
        result[0].PaymentOrderId.ShouldBe(2);
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

    // ─── T048: Include payment details when linked ────────────────

    [Test]
    public async Task Handle_WithLinkedPaymentOrder_ShouldIncludePaymentOrderData()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(PaymentOrderId: 1),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].PaymentOrderNumber.ShouldBe("PO-001");
        result[0].PayeeName.ShouldBe("Vendor A");
    }

    // ─── T049: Compute totals correctly ───────────────────────────

    [Test]
    public async Task Handle_ShouldComputeRequestedAmountAsGrossMinusDeduction()
    {
        await SeedDataAsync();

        var handler = new GetDisbursementRequestsQueryHandler(_dbContext);

        var result = await handler.Handle(
            new GetDisbursementRequestsQuery(PaymentOrderId: 1),
            CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].RequestedAmount.ShouldBe(8000m);
    }
}
