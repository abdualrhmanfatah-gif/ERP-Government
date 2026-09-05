using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings;

[TestFixture]
public class DocumentSequenceServiceTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDatabaseTransactionFactory> _transactionFactoryMock = null!;
    private Mock<IDbContextTransaction> _transactionMock = null!;
    private DocumentSequenceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _transactionFactoryMock = new Mock<IDatabaseTransactionFactory>();
        _transactionMock = new Mock<IDbContextTransaction>();

        _transactionFactoryMock
            .Setup(f => f.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _service = new DocumentSequenceService(_contextMock.Object, _transactionFactoryMock.Object);
    }

    [Test]
    public async Task GenerateNextNumber_UnknownDocumentType_ShouldThrowDocumentSequenceException()
    {
        var act = () => _service.GenerateNextNumberAsync("UnknownType", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<DocumentSequenceException>();
        ex.Message.ShouldContain("Unknown document type");
        ex.Message.ShouldContain("UnknownType");
    }

    [Test]
    public async Task GenerateNextNumber_EmptyDocumentType_ShouldThrowDocumentSequenceException()
    {
        var act = () => _service.GenerateNextNumberAsync("", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<DocumentSequenceException>();
        ex.Message.ShouldContain("Unknown document type");
    }

    [Test]
    public async Task GenerateNextNumber_NoSequenceFound_ShouldThrowDocumentSequenceException()
    {
        _contextMock.Setup(c => c.DocumentSequences)
            .Returns(new List<DocumentSequence>().AsQueryable().BuildMockForAsync().Object);

        var act = () => _service.GenerateNextNumberAsync("Budget", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<DocumentSequenceException>();
        ex.Message.ShouldContain("No sequence found");
        ex.Message.ShouldContain("Budget");
    }

    [Test]
    public async Task GenerateNextNumber_InactiveSequence_ShouldThrowDocumentSequenceException()
    {
        var sequences = new List<DocumentSequence>
        {
            new()
            {
                Id = 1,
                DocumentType = "Budget",
                IsActive = false,
                CurrentNumber = 10,
                RowVersion = [1, 2, 3]
            }
        }.AsQueryable().BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);

        var act = () => _service.GenerateNextNumberAsync("Budget", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<DocumentSequenceException>();
        ex.Message.ShouldContain("deactivated");
        ex.Message.ShouldContain("Budget");
    }

    [Test]
    public async Task GenerateNextNumber_ValidSequence_ShouldReturnFormattedNumber()
    {
        var sequence = new DocumentSequence
        {
            Id = 1,
            DocumentType = "Budget",
            IsActive = true,
            CurrentNumber = 42,
            RowVersion = [1, 2, 3]
        };

        var sequences = new List<DocumentSequence> { sequence }
            .AsQueryable()
            .BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.GenerateNextNumberAsync("Budget", CancellationToken.None);

        result.ShouldBe("BGT-000043");
        sequence.CurrentNumber.ShouldBe(43);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GenerateNextNumber_SaveChangesReturnsZero_ShouldThrowSequenceConcurrencyException()
    {
        var sequence = new DocumentSequence
        {
            Id = 1,
            DocumentType = "JournalEntry",
            IsActive = true,
            CurrentNumber = 5,
            RowVersion = [1, 2, 3]
        };

        var sequences = new List<DocumentSequence> { sequence }
            .AsQueryable()
            .BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var act = () => _service.GenerateNextNumberAsync("JournalEntry", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<SequenceConcurrencyException>();
        ex.Message.ShouldContain("Concurrency conflict");
        ex.Message.ShouldContain("JournalEntry");
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GenerateNextNumber_ValidSequence_ShouldIncrementAndFormat()
    {
        var sequence = new DocumentSequence
        {
            Id = 1,
            DocumentType = "Appropriation",
            IsActive = true,
            CurrentNumber = 999,
            RowVersion = [1, 2, 3]
        };

        var sequences = new List<DocumentSequence> { sequence }
            .AsQueryable()
            .BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.GenerateNextNumberAsync("Appropriation", CancellationToken.None);

        result.ShouldBe("APR-001000");
        sequence.CurrentNumber.ShouldBe(1000);
    }

    [Test]
    public async Task GenerateNextNumber_PaymentOrder_ShouldUseCorrectPrefix()
    {
        var sequence = new DocumentSequence
        {
            Id = 1,
            DocumentType = "PaymentOrder",
            IsActive = true,
            CurrentNumber = 1,
            RowVersion = [1, 2, 3]
        };

        var sequences = new List<DocumentSequence> { sequence }
            .AsQueryable()
            .BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.GenerateNextNumberAsync("PaymentOrder", CancellationToken.None);

        result.ShouldBe("PO-000002");
    }

    [Test]
    public async Task GenerateNextNumber_Encumbrance_ShouldUseCorrectPrefix()
    {
        var sequence = new DocumentSequence
        {
            Id = 1,
            DocumentType = "Encumbrance",
            IsActive = true,
            CurrentNumber = 50,
            RowVersion = [1, 2, 3]
        };

        var sequences = new List<DocumentSequence> { sequence }
            .AsQueryable()
            .BuildMockForAsync();

        _contextMock.Setup(c => c.DocumentSequences).Returns(sequences.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.GenerateNextNumberAsync("Encumbrance", CancellationToken.None);

        result.ShouldBe("ENC-000051");
    }

    [Test]
    public async Task GenerateNextNumber_ShouldRollbackOnException()
    {
        _contextMock.Setup(c => c.DocumentSequences)
            .Returns(new List<DocumentSequence>().AsQueryable().BuildMockForAsync().Object);

        var act = () => _service.GenerateNextNumberAsync("Budget", CancellationToken.None);

        await act.ShouldThrowAsync<DocumentSequenceException>();
        _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void PrefixMap_ContainsAllExpectedDocumentTypes()
    {
        var field = typeof(DocumentSequenceService)
            .GetField("PrefixMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var prefixMap = (Dictionary<string, string>)field!.GetValue(null)!;

        prefixMap.ShouldContainKey("Budget");
        prefixMap.ShouldContainKey("Appropriation");
        prefixMap.ShouldContainKey("Encumbrance");
        prefixMap.ShouldContainKey("PaymentOrder");
        prefixMap.ShouldContainKey("PaymentExecution");
        prefixMap.ShouldContainKey("AdvancePayment");
        prefixMap.ShouldContainKey("JournalEntry");
        prefixMap.ShouldContainKey("PurchaseRequest");
        prefixMap.ShouldContainKey("PurchaseOrder");
        prefixMap.ShouldContainKey("RevenueReceipt");
        prefixMap.ShouldContainKey("GoodsReceiptNote");
        prefixMap.ShouldContainKey("StockTake");
        prefixMap.ShouldContainKey("Asset");
        prefixMap.ShouldContainKey("AssetDisposal");
        prefixMap.ShouldContainKey("AssetRevaluation");
        prefixMap.ShouldContainKey("AssetImpairment");
        prefixMap.ShouldContainKey("RequestForQuotation");
        prefixMap.ShouldContainKey("Quotation");
    }

    [Test]
    public void PrefixMap_ValuesAreCorrect()
    {
        var field = typeof(DocumentSequenceService)
            .GetField("PrefixMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var prefixMap = (Dictionary<string, string>)field!.GetValue(null)!;

        prefixMap["Budget"].ShouldBe("BGT");
        prefixMap["Appropriation"].ShouldBe("APR");
        prefixMap["Encumbrance"].ShouldBe("ENC");
        prefixMap["PaymentOrder"].ShouldBe("PO");
        prefixMap["PaymentExecution"].ShouldBe("PE");
        prefixMap["AdvancePayment"].ShouldBe("ADV");
        prefixMap["JournalEntry"].ShouldBe("JRN");
        prefixMap["PurchaseRequest"].ShouldBe("PRQ");
        prefixMap["PurchaseOrder"].ShouldBe("PO");
        prefixMap["RevenueReceipt"].ShouldBe("REV");
    }

    [Test]
    public void PrefixMap_IsCaseInsensitive()
    {
        var field = typeof(DocumentSequenceService)
            .GetField("PrefixMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var prefixMap = (Dictionary<string, string>)field!.GetValue(null)!;

        prefixMap.ContainsKey("paymentorder").ShouldBeTrue();
        prefixMap.ContainsKey("PAYMENTORDER").ShouldBeTrue();
        prefixMap.ContainsKey("budget").ShouldBeTrue();
    }
}
