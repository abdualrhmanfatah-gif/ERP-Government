using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateAccrualEntry;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class CreateAccrualEntryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IUser> _userMock = null!;
    private const int UserId = 5;
    private const int DisbursementRequestId = 1;
    private const int ExpenseAccountId = 100;
    private const int LiabilityAccountId = 200;
    private const int CurrencyId = 1;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(x => x.Id).Returns(UserId);
    }

    private void SetupDisbursementRequest(
        DisbursementRequestStatus status,
        decimal requestedAmount = 1000m,
        int? accrualJournalEntryId = null)
    {
        var entity = new DisbursementRequest
        {
            Id = DisbursementRequestId,
            Status = status,
            RequestedAmount = requestedAmount,
            BeneficiaryName = "Test Beneficiary",
            RequestNumber = "DR-001",
            CurrencyId = CurrencyId,
            AccrualJournalEntryId = accrualJournalEntryId,
            RowVersion = [1, 2, 3]
        };

        var requestsMock = new Mock<DbSet<DisbursementRequest>>();
        requestsMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == entity.Id ? entity : null));

        _contextMock.Setup(x => x.DisbursementRequests).Returns(requestsMock.Object);
    }

    private void SetupAccounts(bool expenseValid = true, bool liabilityValid = true)
    {
        var expenseAccount = expenseValid ? new Account
        {
            Id = ExpenseAccountId,
            Code = "352216",
            Name = "مساعدات مرضية",
            IsPostable = true,
            IsActive = true
        } : null;

        var liabilityAccount = liabilityValid ? new Account
        {
            Id = LiabilityAccountId,
            Code = "2531",
            Name = "دائنون متنوعون محليون",
            IsPostable = true,
            IsActive = true
        } : null;

        var accounts = new List<Account>();
        if (expenseAccount is not null) accounts.Add(expenseAccount);
        if (liabilityAccount is not null) accounts.Add(liabilityAccount);

        var accountsMock = new Mock<DbSet<Account>>();
        accountsMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                accounts.FirstOrDefault(a => a.Id == (int)kvs[0])));

        _contextMock.Setup(x => x.Accounts).Returns(accountsMock.Object);
    }

    private void SetupFiscalPeriod()
    {
        var period = new FiscalPeriod
        {
            Id = 1,
            IsActive = true,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            FiscalYearId = 1
        };

        var periods = new List<FiscalPeriod> { period };
        var periodsMock = periods.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.FiscalPeriods).Returns(periodsMock.Object);
    }

    private void SetupJournalEntries()
    {
        var journalEntries = new List<JournalEntry>();
        var journalEntriesMock = journalEntries.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesMock.Object);

        var journalEntryLines = new List<JournalEntryLine>();
        var journalEntryLinesMock = journalEntryLines.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.JournalEntryLines).Returns(journalEntryLinesMock.Object);
    }

    [Test]
    public async Task Should_CreateAccrualEntry_When_AllDataValid()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();
        _sequenceServiceMock.Setup(x => x.GenerateNextNumberAsync("JournalEntry", It.IsAny<CancellationToken>()))
            .ReturnsAsync("JE-001");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task Should_Fail_When_DisbursementRequestNotFound()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = 999,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }

    [Test]
    public async Task Should_Fail_When_RequestNotApproved()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Draft);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("approved"));
    }

    [Test]
    public async Task Should_Fail_When_AccrualAlreadyExists()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved, accrualJournalEntryId: 1);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task Should_Fail_When_ExpenseAccountInvalid()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts(expenseValid: false);
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("expense account"));
    }

    [Test]
    public async Task Should_Fail_When_LiabilityAccountInvalid()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts(liabilityValid: false);
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("liability account"));
    }

    [Test]
    public async Task Should_Fail_When_AmountExceedsRequested()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved, requestedAmount: 100m);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("exceed"));
    }

    [Test]
    public async Task Should_Fail_When_AccountsSame()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = ExpenseAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("different"));
    }

    [Test]
    public async Task Should_LinkAccrualToRequest_When_Success()
    {
        SetupDisbursementRequest(DisbursementRequestStatus.Approved);
        SetupAccounts();
        SetupFiscalPeriod();
        SetupJournalEntries();
        _sequenceServiceMock.Setup(x => x.GenerateNextNumberAsync("JournalEntry", It.IsAny<CancellationToken>()))
            .ReturnsAsync("JE-001");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccrualEntryCommandHandler(
            _contextMock.Object,
            _sequenceServiceMock.Object,
            _userMock.Object);

        var result = await handler.Handle(
            new CreateAccrualEntryCommand
            {
                DisbursementRequestId = DisbursementRequestId,
                ExpenseAccountId = ExpenseAccountId,
                LiabilityAccountId = LiabilityAccountId,
                Amount = 500m,
                CurrencyId = CurrencyId
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        // Verify that the disbursement request's AccrualJournalEntryId was set
        var requestsMock = _contextMock.Object.DisbursementRequests;
        var entity = await requestsMock.FindAsync(DisbursementRequestId);
        entity.ShouldNotBeNull();
        entity.AccrualJournalEntryId.ShouldNotBeNull();
    }
}
