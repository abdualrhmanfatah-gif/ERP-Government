using ERP_Government.Application.Assets.AssetTransactions.Transfers.Queries;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

[TestFixture]
public class TransferQueryTests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _context = TransferTestData.CreateContext();
        TransferTestData.SeedBasics(_context);

        _context.AssetTransactions.AddRange(
            NewTransfer(1, "TRF-000001", new DateOnly(2026, 9, 1), AssetTransactionStatus.Executed),
            NewTransfer(2, "TRF-000002", new DateOnly(2026, 9, 5), AssetTransactionStatus.Draft),
            NewTransfer(3, "TRF-000003", new DateOnly(2026, 8, 1), AssetTransactionStatus.Cancelled));

        _context.AssetTransactions.Add(new AssetTransaction
        {
            Id = 9,
            TransactionNumber = "DSP-000009",
            AssetId = 1,
            TransactionType = AssetTransactionType.Disposal,
            TransactionDate = new DateOnly(2026, 9, 6),
            Status = AssetTransactionStatus.Draft,
            CurrencyId = 1
        });

        _context.AssetTransferDetails.AddRange(
            NewDetail(1, new DateOnly(2026, 9, 1)),
            NewDetail(2, new DateOnly(2026, 9, 5)),
            NewDetail(3, new DateOnly(2026, 8, 1)));

        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private static AssetTransaction NewTransfer(int id, string number, DateOnly date, AssetTransactionStatus status) =>
        new()
        {
            Id = id,
            TransactionNumber = number,
            AssetId = 1,
            TransactionType = AssetTransactionType.Transfer,
            TransactionDate = date,
            Status = status,
            CurrencyId = 1,
            RowVersion = [1, 2, 3]
        };

    private static AssetTransferDetail NewDetail(int transactionId, DateOnly date) =>
        new()
        {
            AssetTransactionId = transactionId,
            OccurredAt = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            FromLocationId = 3,
            ToLocationId = 4,
            FromEmployeeId = 1,
            ToEmployeeId = 2,
            FromDepartmentId = 5,
            ToDepartmentId = 6
        };

    [Test]
    public async Task List_ReturnsOnlyTransfers_NewestFirst()
    {
        var handler = new GetAssetTransfersQueryHandler(_context);

        var result = await handler.Handle(new GetAssetTransfersQuery(), CancellationToken.None);

        result.TotalCount.ShouldBe(3);
        result.Items.Count.ShouldBe(3);
        result.Items.Select(i => i.DocumentNumber)
            .ShouldBe(new[] { "TRF-000002", "TRF-000001", "TRF-000003" });
        result.Items[0].AssetCode.ShouldBe("AST-1");
        result.Items[0].FromLocationName.ShouldBe("المبنى أ");
        result.Items[0].ToEmployeeName.ShouldBe("سارة");
    }

    [Test]
    public async Task List_FiltersByStatus()
    {
        var handler = new GetAssetTransfersQueryHandler(_context);

        var result = await handler.Handle(
            new GetAssetTransfersQuery(Status: "Draft"), CancellationToken.None);

        result.TotalCount.ShouldBe(1);
        result.Items.Single().DocumentNumber.ShouldBe("TRF-000002");
    }

    [Test]
    public async Task List_SearchMatchesNumberAndAssetFields()
    {
        var handler = new GetAssetTransfersQueryHandler(_context);

        var byNumber = await handler.Handle(
            new GetAssetTransfersQuery(Search: "TRF-000003"), CancellationToken.None);
        var byAssetCode = await handler.Handle(
            new GetAssetTransfersQuery(Search: "AST-1"), CancellationToken.None);
        var byAssetName = await handler.Handle(
            new GetAssetTransfersQuery(Search: "حاسوب"), CancellationToken.None);

        byNumber.TotalCount.ShouldBe(1);
        byAssetCode.TotalCount.ShouldBe(3);
        byAssetName.TotalCount.ShouldBe(3);
    }

    [Test]
    public async Task List_Paginates()
    {
        var handler = new GetAssetTransfersQueryHandler(_context);

        var result = await handler.Handle(
            new GetAssetTransfersQuery(Page: 2, PageSize: 2), CancellationToken.None);

        result.TotalCount.ShouldBe(3);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(2);
        result.Items.Count.ShouldBe(1);
        result.Items.Single().DocumentNumber.ShouldBe("TRF-000003");
    }

    [Test]
    public async Task Detail_ReturnsNamesDepartmentsAndTokens()
    {
        var handler = new GetAssetTransferByIdQueryHandler(_context);

        var result = await handler.Handle(new GetAssetTransferByIdQuery(1), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var detail = result.Value!;
        detail.DocumentNumber.ShouldBe("TRF-000001");
        detail.Status.ShouldBe("Executed");
        detail.AssetCode.ShouldBe("AST-1");
        detail.AssetName.ShouldBe("حاسوب محمول");
        detail.FromLocationName.ShouldBe("المبنى أ");
        detail.ToLocationName.ShouldBe("المبنى ب");
        detail.FromEmployeeName.ShouldBe("أحمد");
        detail.ToEmployeeName.ShouldBe("سارة");
        detail.FromDepartmentName.ShouldBe("إدارة تقنية المعلومات");
        detail.ToDepartmentName.ShouldBe("إدارة المشتريات");
        detail.RowVersion.ShouldBe([1, 2, 3]);
        detail.AssetRowVersion.ShouldNotBeNull();
    }

    [Test]
    public async Task Detail_UnknownOrNonTransfer_ReturnsNotFound()
    {
        var handler = new GetAssetTransferByIdQueryHandler(_context);

        var missing = await handler.Handle(new GetAssetTransferByIdQuery(999), CancellationToken.None);
        var wrongType = await handler.Handle(new GetAssetTransferByIdQuery(9), CancellationToken.None);

        missing.Succeeded.ShouldBeFalse();
        missing.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
        wrongType.Succeeded.ShouldBeFalse();
        wrongType.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
    }
}
