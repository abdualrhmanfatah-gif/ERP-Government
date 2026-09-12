using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class CreateDisbursementRequestTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IUser> _userMock = null!;
    private const int UserId = 1;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _userMock = new Mock<IUser>();
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("DR-000001");
        _userMock.Setup(x => x.Id).Returns(UserId);

        SeedReferenceData();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private void SeedReferenceData()
    {
        _dbContext.Currencies.Add(new Currency { Id = 1, Code = "YER", Name = "Yemeni Rial" });
        _dbContext.FiscalYears.Add(new FiscalYear { Id = 1, YearNumber = 2026, IsClosed = false });
        _dbContext.Users.Add(new User { Id = UserId, Login = "testuser", IsActive = true });
        _dbContext.SaveChanges();
    }

    private static CreateDisbursementRequestCommand ValidCommand() => new()
    {
        BeneficiaryName = "Test Beneficiary",
        RequestedAmount = 5000m,
        CurrencyId = 1,
        Purpose = "Test purpose",
        FinancialYearId = 1
    };

    // ─── Success ──────────────────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_ValidCommand_ShouldSucceed()
    {
        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Status.ShouldBe(DisbursementRequestStatus.Draft);
        result.Value.RequestNumber.ShouldBe("DR-000001");
        result.Value.BeneficiaryName.ShouldBe("Test Beneficiary");
        result.Value.RequestedAmount.ShouldBe(5000m);
    }

    // ─── Amount <= 0 ──────────────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_ZeroAmount_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Beneficiary",
            RequestedAmount = 0m,
            CurrencyId = 1,
            Purpose = "Test purpose",
            FinancialYearId = 1
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Requested amount must be greater than zero"));
    }

    [Test]
    public async Task CreateDisbursementRequest_NegativeAmount_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Beneficiary",
            RequestedAmount = -100m,
            CurrencyId = 1,
            Purpose = "Test purpose",
            FinancialYearId = 1
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Requested amount must be greater than zero"));
    }

    // ─── Beneficiary name ─────────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_EmptyBeneficiaryName_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "",
            RequestedAmount = 5000m,
            CurrencyId = 1,
            Purpose = "Test purpose",
            FinancialYearId = 1
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Beneficiary name is required"));
    }

    // ─── Purpose ──────────────────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_EmptyPurpose_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Beneficiary",
            RequestedAmount = 5000m,
            CurrencyId = 1,
            Purpose = "",
            FinancialYearId = 1
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Purpose is required"));
    }

    // ─── Invalid references ───────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_InvalidCurrency_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Beneficiary",
            RequestedAmount = 5000m,
            CurrencyId = 999,
            Purpose = "Test purpose",
            FinancialYearId = 1
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Invalid currency"));
    }

    [Test]
    public async Task CreateDisbursementRequest_InvalidFiscalYear_ShouldReject()
    {
        var command = new CreateDisbursementRequestCommand
        {
            BeneficiaryName = "Test Beneficiary",
            RequestedAmount = 5000m,
            CurrencyId = 1,
            Purpose = "Test purpose",
            FinancialYearId = 999
        };

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Invalid fiscal year"));
    }

    // ─── Sequence number failure ──────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_SequenceNumberFails_ShouldReject()
    {
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentSequenceException("Sequence exhausted"));

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Sequence exhausted"));
    }
}
