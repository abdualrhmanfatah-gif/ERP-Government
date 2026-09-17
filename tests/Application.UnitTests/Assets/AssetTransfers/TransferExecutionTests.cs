using ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

[TestFixture]
public class TransferExecutionTests
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

    private async Task<(int TransactionId, byte[] RowVersion, byte[] AssetRowVersion)> CreateDraft()
    {
        var handler = new CreateAssetTransferCommandHandler(_context, TransferTestData.Sequence().Object);
        var created = await handler.Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: 2, Notes: null), CancellationToken.None);

        var transaction = _context.AssetTransactions.Single(t => t.Id == created.Value);
        var asset = _context.Assets.Single();
        transaction.RowVersion = [1, 2, 3];
        asset.RowVersion = [4, 5, 6];
        await _context.SaveChangesAsync();

        return (created.Value, [1, 2, 3], [4, 5, 6]);
    }

    [Test]
    public async Task Execute_WhenCardSourceChanged_ReturnsConflictAndKeepsCard()
    {
        var (transactionId, rowVersion, assetRowVersion) = await CreateDraft();

        var asset = _context.Assets.Single();
        asset.LocationId = 4;
        asset.RowVersion = [7, 8, 9];
        await _context.SaveChangesAsync();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, [7, 8, 9]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.TransferSourceChanged);
        result.Category.ShouldBe(ErrorCategory.Conflict);

        _context.AssetTransactions.Single().Status.ShouldBe(AssetTransactionStatus.Draft);
        _context.Assets.Single().EmployeeId.ShouldBe(1);
    }

    [Test]
    public async Task Execute_WhenHeaderTokenMismatch_ReturnsConcurrencyConflict()
    {
        var (transactionId, _, assetRowVersion) = await CreateDraft();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, [9, 9, 9], assetRowVersion), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Request.ConcurrencyConflict);
        result.Category.ShouldBe(ErrorCategory.Conflict);
    }

    [Test]
    public async Task Execute_WhenAssetTokenMismatch_ReturnsConcurrencyConflict()
    {
        var (transactionId, rowVersion, _) = await CreateDraft();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, [9, 9, 9]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Request.ConcurrencyConflict);
    }

    [Test]
    public async Task Execute_WhenAssetNotActive_ReturnsValidationFailure()
    {
        var (transactionId, rowVersion, assetRowVersion) = await CreateDraft();

        var asset = _context.Assets.Single();
        asset.Status = "Disposed";
        await _context.SaveChangesAsync();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(transactionId, rowVersion, assetRowVersion), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Execute_WhenTransactionIsNotTransfer_ReturnsNotFound()
    {
        _context.AssetTransactions.Add(new AssetTransaction
        {
            Id = 60,
            TransactionNumber = "DSP-000060",
            AssetId = 1,
            TransactionType = AssetTransactionType.Disposal,
            TransactionDate = new DateOnly(2026, 9, 1),
            Status = AssetTransactionStatus.Draft,
            CurrencyId = 1,
            RowVersion = [1]
        });
        await _context.SaveChangesAsync();

        var handler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new ExecuteAssetTransferCommand(60, [1], [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.TransferNotFound);
    }

    [Test]
    public async Task Update_WhenTokenMismatch_ReturnsConcurrencyConflict()
    {
        var (transactionId, _, _) = await CreateDraft();

        var handler = new UpdateAssetTransferCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetTransferCommand(
            Id: transactionId, TransactionDate: new DateOnly(2026, 9, 2),
            ToLocationId: 4, ToEmployeeId: 2, Notes: null, RowVersion: [9, 9, 9]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Request.ConcurrencyConflict);
    }

    [Test]
    public async Task Cancel_WhenTokenMismatch_ReturnsConcurrencyConflict()
    {
        var (transactionId, _, _) = await CreateDraft();

        var handler = new CancelAssetTransferCommandHandler(_context);
        var result = await handler.Handle(
            new CancelAssetTransferCommand(transactionId, [9, 9, 9]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Request.ConcurrencyConflict);
        _context.AssetTransactions.Single().Status.ShouldBe(AssetTransactionStatus.Draft);
    }
}
