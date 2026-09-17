using ERP_Government.Application.Accounting.Integration.Assets;
using ERP_Government.Application.Assets.AssetTransactions.Disposals.Commands;
using ERP_Government.Application.Assets.AssetTransactions.Impairments.Commands;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Events.Assets;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransactions;

[TestFixture]
public class AssetPostingWiringTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IDocumentSequenceService> _sequence = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _sequence = new Mock<IDocumentSequenceService>();
        _sequence.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("DOC-000001");

        for (var i = 1; i <= 6; i++)
            _context.Accounts.Add(new Account { Id = i, Code = $"5{i:000}", Name = $"حساب {i}", AccountGroupId = 1, IsPostable = true, IsActive = true });

        _context.FiscalPeriods.Add(new FiscalPeriod
        {
            Id = 1, FiscalYearId = 1, PeriodNumber = 1, Name = "السنة",
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), IsActive = true
        });

        _context.AssetGroups.Add(new AssetGroup
        {
            Id = 1, Code = "AG-1", Name = "مجموعة",
            AssetAccountId = 1, AccumulatedDepreciationAccountId = 2,
            DepreciationExpenseAccountId = 6, DisposalAccountId = 3
        });

        _context.Assets.AddRange(
            new Asset { Id = 1, Code = "AST-1", Name = "للبيع", AssetGroupId = 1, CurrencyId = 1, OriginalValue = 1200m, AccumulatedDepreciation = 200m, Status = "Active" },
            new Asset { Id = 2, Code = "AST-2", Name = "للتقييم", AssetGroupId = 1, CurrencyId = 1, OriginalValue = 1200m, Status = "Active" },
            new Asset { Id = 3, Code = "AST-3", Name = "لانخفاض القيمة", AssetGroupId = 1, CurrencyId = 1, OriginalValue = 1000m, CurrentValue = 1000m, Status = "Active" });

        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task CreateDisposal_DerivesNetProceedsAndGainLossFromStoredSnapshots()
    {
        var handler = new CreateAssetDisposalCommandHandler(_context, _sequence.Object);

        var result = await handler.Handle(new CreateAssetDisposalCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            SalePrice: 1500m, DisposalCost: 100m, BuyerName: "مشتري", BuyerContact: "077", Notes: null),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var detail = _context.AssetDisposalDetails.Single();
        detail.BookValueAtDisposal.ShouldBe(1000m);
        detail.AccumulatedDepreciationAtDisposal.ShouldBe(200m);
        detail.NetProceeds.ShouldBe(1400m);
        detail.GainOrLoss.ShouldBe(400m);
        detail.BuyerName.ShouldBe("مشتري");
    }

    [Test]
    public async Task PostDisposal_EmitsEventAndConsumerAppliesEntryAndCardEffect()
    {
        var create = new CreateAssetDisposalCommandHandler(_context, _sequence.Object);
        var transactionId = (await create.Handle(new CreateAssetDisposalCommand(
            1, new DateOnly(2026, 9, 1), 1500m, 100m, null, null, null), CancellationToken.None)).Value;

        var approve = new ApproveAssetDisposalCommandHandler(_context);
        await approve.Handle(new ApproveAssetDisposalCommand(transactionId), CancellationToken.None);

        var post = new PostAssetDisposalCommandHandler(_context);
        var postResult = await post.Handle(new PostAssetDisposalCommand(transactionId), CancellationToken.None);

        postResult.Succeeded.ShouldBeTrue();
        var transaction = _context.AssetTransactions.Single();
        transaction.Status.ShouldBe(AssetTransactionStatus.Posting);
        _context.Assets.Single(a => a.Id == 1).Status.ShouldBe("Active");

        var domainEvent = transaction.DomainEvents.OfType<AssetDisposed>().Single();
        domainEvent.SourceEntityId.ShouldBe(transaction.Id);
        domainEvent.NetBookValue.ShouldBe(1000m);
        domainEvent.PeriodId.ShouldBe(1);

        var consumer = new AssetDisposedHandler(_context, _sequence.Object);
        await consumer.Handle(domainEvent, CancellationToken.None);

        transaction.Status.ShouldBe(AssetTransactionStatus.Posted);
        transaction.IsPosted.ShouldBeTrue();
        transaction.JournalEntryId.ShouldNotBeNull();
        _context.Assets.Single(a => a.Id == 1).Status.ShouldBe("Disposed");
        _context.JournalEntries.Count().ShouldBe(1);
    }

    [Test]
    public async Task ReverseImpairment_CreatesLinkedTransactionAndEvent_WithoutDirectCardMutation()
    {
        var original = new AssetTransaction
        {
            Id = 20, TransactionNumber = "IMP-1", AssetId = 3,
            TransactionType = AssetTransactionType.Impairment,
            TransactionDate = new DateOnly(2026, 8, 1),
            Status = AssetTransactionStatus.Posted, CurrencyId = 1
        };
        _context.AssetTransactions.Add(original);
        _context.AssetImpairmentDetails.Add(new AssetImpairmentDetail
        {
            AssetTransactionId = 20, CarryingAmount = 1000m, RecoverableAmount = 700m,
            LossAmount = 300m, Reason = "تلف"
        });
        _context.SaveChanges();

        var handler = new ReverseAssetImpairmentCommandHandler(_context, _sequence.Object);
        var result = await handler.Handle(new ReverseAssetImpairmentCommand(20), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var reversal = _context.AssetTransactions.Single(t => t.Id != 20);
        reversal.Status.ShouldBe(AssetTransactionStatus.Posting);

        var reversalDetail = _context.AssetImpairmentDetails.Single(d => d.AssetTransactionId == reversal.Id);
        reversalDetail.ReversalOfTransactionId.ShouldBe(20);

        original.Status.ShouldBe(AssetTransactionStatus.Reversed);
        _context.Assets.Single(a => a.Id == 3).CurrentValue.ShouldBe(1000m);

        var domainEvent = reversal.DomainEvents.OfType<AssetImpairmentReversed>().Single();
        domainEvent.SourceEntityId.ShouldBe(reversal.Id);
        domainEvent.ReversalOfTransactionId.ShouldBe(20);
        domainEvent.ReversalAmount.ShouldBe(300m);
    }

    [Test]
    public async Task ReverseImpairment_SecondAttempt_IsRejected()
    {
        var original = new AssetTransaction
        {
            Id = 20, TransactionNumber = "IMP-1", AssetId = 3,
            TransactionType = AssetTransactionType.Impairment,
            TransactionDate = new DateOnly(2026, 8, 1),
            Status = AssetTransactionStatus.Posted, CurrencyId = 1
        };
        _context.AssetTransactions.Add(original);
        _context.AssetImpairmentDetails.Add(new AssetImpairmentDetail
        {
            AssetTransactionId = 20, CarryingAmount = 1000m, RecoverableAmount = 700m,
            LossAmount = 300m, Reason = "تلف"
        });
        _context.SaveChanges();

        var handler = new ReverseAssetImpairmentCommandHandler(_context, _sequence.Object);
        await handler.Handle(new ReverseAssetImpairmentCommand(20), CancellationToken.None);

        original.Status = AssetTransactionStatus.Posted;
        _context.SaveChanges();

        var second = await handler.Handle(new ReverseAssetImpairmentCommand(20), CancellationToken.None);

        second.Succeeded.ShouldBeFalse();
        second.Code.ShouldBe(ErrorCodes.Assets.ReversalAlreadyExists);
    }
}
