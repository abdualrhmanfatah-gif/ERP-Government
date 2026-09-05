using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Commands.FiscalYears;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.FinancialSettings;

[TestFixture]
public class YearlyResetTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private OpenFiscalYearCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new OpenFiscalYearCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_YearlyResetSequence_ResetsCurrentNumberToOne()
    {
        var fiscalYear = new FiscalYear { Id = 1, Status = FiscalYearStatus.Draft };
        var sequences = new List<DocumentSequence>
        {
            new() { Id = 1, DocumentType = "PaymentOrder", ResetPolicy = ResetPolicy.Yearly, FiscalYearId = 1, CurrentNumber = 50 }
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(fiscalYear);

        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
        _contextMock.Setup(x => x.DocumentSequences)
            .Returns(sequences.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new OpenFiscalYearCommand { Id = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        fiscalYear.Status.ShouldBe(FiscalYearStatus.Open);
        sequences[0].CurrentNumber.ShouldBe(1);
    }

    [Test]
    public async Task Handle_NeverResetSequence_DoesNotReset()
    {
        var fiscalYear = new FiscalYear { Id = 1, Status = FiscalYearStatus.Draft };
        var sequences = new List<DocumentSequence>
        {
            new() { Id = 1, DocumentType = "JournalEntry", ResetPolicy = ResetPolicy.Never, FiscalYearId = 1, CurrentNumber = 100 }
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(fiscalYear);

        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
        _contextMock.Setup(x => x.DocumentSequences)
            .Returns(sequences.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new OpenFiscalYearCommand { Id = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        sequences[0].CurrentNumber.ShouldBe(100); // unchanged
    }

    [Test]
    public async Task Handle_YearlyResetWithNullFiscalYearId_DoesNotReset()
    {
        var fiscalYear = new FiscalYear { Id = 1, Status = FiscalYearStatus.Draft };
        var sequences = new List<DocumentSequence>
        {
            new() { Id = 1, DocumentType = "Appropriation", ResetPolicy = ResetPolicy.Yearly, FiscalYearId = null, CurrentNumber = 200 }
        };

        var mockSet = new Mock<DbSet<FiscalYear>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(fiscalYear);

        _contextMock.Setup(x => x.FiscalYears).Returns(mockSet.Object);
        _contextMock.Setup(x => x.DocumentSequences)
            .Returns(sequences.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new OpenFiscalYearCommand { Id = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        sequences[0].CurrentNumber.ShouldBe(200); // unchanged
    }
}
