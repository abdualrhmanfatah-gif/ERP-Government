using ERP_Government.Application.Accounting.Commands.RecurringEntries.CreateRecurringEntry;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class RecurringEntryCreateTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _sequenceMock = null!;
    private CreateRecurringEntryCommandHandler _handler = null!;
    private CreateRecurringEntryCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Seed journal
        _dbContext.Journals.Add(new Domain.Accounting.Entities.Journal
        {
            Id = 1,
            Name = "General",
            IsActive = true
        });
        _dbContext.SaveChanges();

        _sequenceMock = new Mock<IDocumentSequenceService>();
        _handler = new CreateRecurringEntryCommandHandler(_dbContext, _sequenceMock.Object);
        _validator = new CreateRecurringEntryCommandValidator();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    // ===================== T003: Create schedule happy path =====================

    [Test]
    public async Task Handle_ValidRequest_EntryNumberStartsWithREC()
    {
        _sequenceMock.Setup(s => s.GenerateNextNumberAsync("RecurringEntry", It.IsAny<CancellationToken>()))
            .ReturnsAsync("REC-000001");

        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Monthly Rent",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            Amount = 5000m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var entry = await _dbContext.RecurringEntries.FirstAsync();
        entry.EntryNumber.ShouldStartWith("REC-");
        _sequenceMock.Verify(s => s.GenerateNextNumberAsync("RecurringEntry", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequest_StatusSetToActive()
    {
        _sequenceMock.Setup(s => s.GenerateNextNumberAsync("RecurringEntry", It.IsAny<CancellationToken>()))
            .ReturnsAsync("REC-000001");

        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Monthly Rent",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            Amount = 5000m
        };

        await _handler.Handle(command, CancellationToken.None);

        var entry = await _dbContext.RecurringEntries.FirstAsync();
        entry.Status.ShouldBe(RecurringEntryStatus.Active);
    }

    [Test]
    public async Task Handle_ValidRequest_NextExecutionDateEqualsStartDate()
    {
        _sequenceMock.Setup(s => s.GenerateNextNumberAsync("RecurringEntry", It.IsAny<CancellationToken>()))
            .ReturnsAsync("REC-000001");

        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Monthly Rent",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            Amount = 5000m
        };

        await _handler.Handle(command, CancellationToken.None);

        var entry = await _dbContext.RecurringEntries.FirstAsync();
        entry.NextExecutionDate.ShouldBe(new DateOnly(2026, 10, 1));
    }

    // ===================== T004: Amount required when no template =====================

    [Test]
    public async Task Validate_NoTemplateNoAmount_ReturnsError()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "No Amount Schedule",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            TemplateId = null,
            Amount = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Amount is required when no template is specified.");
    }

    [Test]
    public async Task Validate_NoTemplateWithAmount_PassesAmountCheck()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "With Amount",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            TemplateId = null,
            Amount = 5000m
        };

        var result = await _validator.ValidateAsync(command);

        result.Errors.ShouldNotContain(e => e.ErrorMessage == "Amount is required when no template is specified.");
    }

    [Test]
    public async Task Validate_WithTemplateNoAmount_PassesAmountCheck()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Template Based",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            TemplateId = 1,
            Amount = null
        };

        var result = await _validator.ValidateAsync(command);

        result.Errors.ShouldNotContain(e => e.ErrorMessage == "Amount is required when no template is specified.");
    }

    // ===================== T005: EndDate >= StartDate validation =====================

    [Test]
    public async Task Validate_EndDateBeforeStartDate_ReturnsError()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Bad Dates",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 12, 1),
            EndDate = new DateOnly(2026, 1, 1),
            Amount = 1000m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "End date must not be before start date.");
    }

    [Test]
    public async Task Validate_EndDateEqualsStartDate_Passes()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "Same Dates",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2026, 10, 1),
            Amount = 1000m
        };

        var result = await _validator.ValidateAsync(command);

        result.Errors.ShouldNotContain(e => e.ErrorMessage == "End date must not be before start date.");
    }

    [Test]
    public async Task Validate_NullEndDate_Passes()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 1,
            Name = "No End Date",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = null,
            Amount = 1000m
        };

        var result = await _validator.ValidateAsync(command);

        result.Errors.ShouldNotContain(e => e.ErrorMessage == "End date must not be before start date.");
    }

    // ===================== T006: JournalId validation =====================

    [Test]
    public async Task Handle_JournalNotFound_ReturnsFailure()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 999,
            Name = "Bad Journal",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            Amount = 1000m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Journal not found.");
    }

    [Test]
    public async Task Validate_JournalIdZero_ReturnsError()
    {
        var command = new CreateRecurringEntryCommand
        {
            JournalId = 0,
            Name = "Zero Journal",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 10, 1),
            Amount = 1000m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Journal is required.");
    }
}
