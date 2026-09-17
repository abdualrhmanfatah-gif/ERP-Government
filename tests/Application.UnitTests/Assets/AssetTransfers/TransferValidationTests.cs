using ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

[TestFixture]
public class TransferValidationTests
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

    [Test]
    public async Task Create_WithoutAnyDestination_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: null, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidTransferDestination);
        _context.AssetTransactions.ShouldBeEmpty();
    }

    [Test]
    public async Task Create_DestinationSameAsCurrent_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 3, ToEmployeeId: 1, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidTransferDestination);
        _context.AssetTransactions.ShouldBeEmpty();
    }

    [Test]
    public async Task Create_SameLocationProvidedWithNullEmployee_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 3, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidTransferDestination);
    }

    [Test]
    public async Task Create_UnknownLocation_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 999, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Inventory.LocationNotFound);
    }

    [Test]
    public async Task Create_InactiveLocation_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 8, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Inventory.LocationInactive);
    }

    [Test]
    public async Task Create_UnknownEmployee_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: null, ToEmployeeId: 999, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Organization.EmployeeNotFound);
    }

    [Test]
    public async Task Create_InactiveEmployee_Fails()
    {
        var employee = _context.Employees.Single(e => e.Id == 2);
        employee.IsActive = false;
        await _context.SaveChangesAsync();

        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: null, ToEmployeeId: 2, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Organization.EmployeeNotFound);
    }

    [Test]
    public async Task Create_UnknownAsset_Fails()
    {
        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 999, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.AssetNotFound);
    }

    [Test]
    public async Task Create_NonActiveAsset_Fails()
    {
        var asset = _context.Assets.Single();
        asset.Status = "Disposed";
        await _context.SaveChangesAsync();

        var result = await CreateHandler().Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: null, Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Update_NonDraftTransfer_Fails()
    {
        var transaction = new AssetTransaction
        {
            Id = 50,
            TransactionNumber = "TRF-000050",
            AssetId = 1,
            TransactionType = AssetTransactionType.Transfer,
            TransactionDate = new DateOnly(2026, 9, 1),
            Status = AssetTransactionStatus.Executed,
            CurrencyId = 1,
            RowVersion = [1]
        };
        _context.AssetTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var handler = new UpdateAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetTransferCommand(
            Id: 50, TransactionDate: new DateOnly(2026, 9, 2),
            ToLocationId: 4, ToEmployeeId: null, Notes: null, RowVersion: [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Cancel_NonDraftTransfer_Fails()
    {
        var transaction = new AssetTransaction
        {
            Id = 51,
            TransactionNumber = "TRF-000051",
            AssetId = 1,
            TransactionType = AssetTransactionType.Transfer,
            TransactionDate = new DateOnly(2026, 9, 1),
            Status = AssetTransactionStatus.Executed,
            CurrencyId = 1,
            RowVersion = [1]
        };
        _context.AssetTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var handler = new CancelAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new CancelAssetTransferCommand(51, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Execute_CancelledTransfer_Fails()
    {
        var transaction = new AssetTransaction
        {
            Id = 52,
            TransactionNumber = "TRF-000052",
            AssetId = 1,
            TransactionType = AssetTransactionType.Transfer,
            TransactionDate = new DateOnly(2026, 9, 1),
            Status = AssetTransactionStatus.Cancelled,
            CurrencyId = 1,
            RowVersion = [1]
        };
        _context.AssetTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(52, [1], [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Execute_UnknownTransfer_Fails()
    {
        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(999, [1], [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
    }

    [Test]
    public async Task Update_UnknownTransfer_Fails()
    {
        var handler = new UpdateAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetTransferCommand(
            Id: 999, TransactionDate: new DateOnly(2026, 9, 2),
            ToLocationId: 4, ToEmployeeId: null, Notes: null, RowVersion: [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
    }

    [Test]
    public async Task Cancel_UnknownTransfer_Fails()
    {
        var handler = new CancelAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new CancelAssetTransferCommand(999, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
    }
}
