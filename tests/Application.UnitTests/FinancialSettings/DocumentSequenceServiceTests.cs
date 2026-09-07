using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.FinancialSettings.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings;

[TestFixture]
public class DocumentSequenceServiceTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private DocumentSequenceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _service = new DocumentSequenceService(_contextMock.Object);
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
    }

    [Test]
    public async Task GenerateNextNumber_SaveChangesThrowsConcurrency_ShouldThrowSequenceConcurrencyException()
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
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        var act = () => _service.GenerateNextNumberAsync("JournalEntry", CancellationToken.None);

        var ex = await act.ShouldThrowAsync<SequenceConcurrencyException>();
        ex.Message.ShouldContain("Concurrency conflict");
        ex.Message.ShouldContain("JournalEntry");
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
    public void PrefixMap_ContainsRecurringEntry()
    {
        var field = typeof(DocumentSequenceService)
            .GetField("PrefixMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var prefixMap = (Dictionary<string, string>)field!.GetValue(null)!;

        prefixMap.ShouldContainKey("RecurringEntry");
        prefixMap["RecurringEntry"].ShouldBe("REC");
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
        prefixMap.ShouldContainKey("JournalEntry");
        prefixMap.ShouldContainKey("RecurringEntry");
    }
}
