using ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

[TestFixture]
public class TransferLifecycleTests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _context = TransferTestData.CreateContext();
        TransferTestData.SeedBasics(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private CreateAssetTransferCommandHandler CreateHandler() =>
        new(_context, TransferTestData.Sequence().Object);

    private async Task<(int TransactionId, byte[] RowVersion, byte[] AssetRowVersion)> CreateDraft(
        int? toLocationId = 4,
        int? toEmployeeId = 1)
    {
        var handler = CreateHandler();
        var created = await handler.Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: toLocationId, ToEmployeeId: toEmployeeId,
            Notes: "ملاحظة"), CancellationToken.None);

        created.Succeeded.ShouldBeTrue();

        var transaction = _context.AssetTransactions.Single(t => t.Id == created.Value);
        var asset = _context.Assets.Single();
        transaction.RowVersion = [1, 2, 3];
        asset.RowVersion = [4, 5, 6];
        await _context.SaveChangesAsync();

        return (created.Value, [1, 2, 3], [4, 5, 6]);
    }

    [Test]
    public async Task Create_Draft_SnapshotsFromValuesAndDoesNotTouchCard()
    {
        var created = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: null,
            Notes: "نقل مكتب"), CancellationToken.None);

        created.Succeeded.ShouldBeTrue();

        var transaction = _context.AssetTransactions.Single();
        transaction.TransactionNumber.ShouldBe("TRF-000001");
        transaction.Status.ShouldBe(AssetTransactionStatus.Draft);
        transaction.IsPosted.ShouldBeFalse();
        transaction.JournalEntryId.ShouldBeNull();
        transaction.TransactionDate.ShouldBe(new DateOnly(2026, 9, 1));

        var detail = _context.AssetTransferDetails.Single();
        detail.FromLocationId.ShouldBe(3);
        detail.FromEmployeeId.ShouldBe(1);
        detail.ToLocationId.ShouldBe(4);
        detail.ToEmployeeId.ShouldBe(1);

        var asset = _context.Assets.Single();
        asset.LocationId.ShouldBe(3);
        asset.EmployeeId.ShouldBe(1);
        asset.LastModified.ShouldBe(default(DateTimeOffset));
    }

    [Test]
    public async Task Create_NewCustodianDestination_SnapshotsBothSides()
    {
        var created = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: null, ToEmployeeId: 2,
            Notes: null), CancellationToken.None);

        created.Succeeded.ShouldBeTrue();

        var detail = _context.AssetTransferDetails.Single();
        detail.ToLocationId.ShouldBe(3);
        detail.ToEmployeeId.ShouldBe(2);
        detail.ToDepartmentId.ShouldBe(6);
    }

    [Test]
    public async Task Execute_HappyPath_UpdatesCardSnapshotsAndOccurrence()
    {
        var (transactionId, rowVersion, assetRowVersion) = await CreateDraft(toLocationId: 4, toEmployeeId: 2);

        var before = DateTimeOffset.UtcNow;

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, assetRowVersion), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBe(transactionId);

        var transaction = _context.AssetTransactions.Single();
        transaction.Status.ShouldBe(AssetTransactionStatus.Executed);
        transaction.IsPosted.ShouldBeFalse();
        transaction.JournalEntryId.ShouldBeNull();

        var detail = _context.AssetTransferDetails.Single();
        detail.FromDepartmentId.ShouldBe(5);
        detail.ToDepartmentId.ShouldBe(6);
        detail.OccurredAt.ShouldBeGreaterThanOrEqualTo(before);

        var asset = _context.Assets.Single();
        asset.LocationId.ShouldBe(4);
        asset.EmployeeId.ShouldBe(2);
        asset.LastModified.ShouldBeGreaterThan(default(DateTimeOffset));
    }

    [Test]
    public async Task Execute_Repeat_ReturnsSameResultWithoutSecondEffect()
    {
        var (transactionId, rowVersion, assetRowVersion) = await CreateDraft(toLocationId: 4, toEmployeeId: 2);

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var first = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, assetRowVersion), CancellationToken.None);

        var assetAfterFirst = _context.Assets.Single();
        var executedAtFirst = _context.AssetTransactions.Single().LastModified;

        // Retry with stale tokens and a stale card version must still resolve to success.
        var second = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, assetRowVersion), CancellationToken.None);

        first.Succeeded.ShouldBeTrue();
        second.Succeeded.ShouldBeTrue();
        second.Value.ShouldBe(transactionId);

        var transaction = _context.AssetTransactions.Single();
        transaction.Status.ShouldBe(AssetTransactionStatus.Executed);
        transaction.LastModified.ShouldBe(executedAtFirst);

        var assetAfterSecond = _context.Assets.Single();
        assetAfterSecond.LocationId.ShouldBe(assetAfterFirst.LocationId);
        assetAfterSecond.EmployeeId.ShouldBe(assetAfterFirst.EmployeeId);

        _context.AssetTransferDetails.Count().ShouldBe(1);
    }

    [Test]
    public async Task Update_Draft_ReSnapshotsFromValuesFromCard()
    {
        var (transactionId, rowVersion, _) = await CreateDraft(toLocationId: 4, toEmployeeId: 1);

        var asset = _context.Assets.Single();
        asset.LocationId = 9;
        await _context.SaveChangesAsync();

        var handler = new UpdateAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetTransferCommand(
            Id: transactionId,
            TransactionDate: new DateOnly(2026, 9, 10),
            ToLocationId: 4,
            ToEmployeeId: 2,
            Notes: "محدث",
            RowVersion: rowVersion), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var transaction = _context.AssetTransactions.Single();
        transaction.TransactionDate.ShouldBe(new DateOnly(2026, 9, 10));
        transaction.Notes.ShouldBe("محدث");
        transaction.Status.ShouldBe(AssetTransactionStatus.Draft);

        var detail = _context.AssetTransferDetails.Single();
        detail.FromLocationId.ShouldBe(9);
        detail.FromEmployeeId.ShouldBe(1);
        detail.ToLocationId.ShouldBe(4);
        detail.ToEmployeeId.ShouldBe(2);
        detail.ToDepartmentId.ShouldBe(6);
    }

    [Test]
    public async Task Cancel_Draft_SetsCancelledAndKeepsCardUntouched()
    {
        var (transactionId, rowVersion, _) = await CreateDraft(toLocationId: 4, toEmployeeId: 2);

        var handler = new CancelAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new CancelAssetTransferCommand(transactionId, rowVersion), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var transaction = _context.AssetTransactions.Single();
        transaction.Status.ShouldBe(AssetTransactionStatus.Cancelled);

        var asset = _context.Assets.Single();
        asset.LocationId.ShouldBe(3);
        asset.EmployeeId.ShouldBe(1);
    }
}
