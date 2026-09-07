using ERP_Government.Application.Accounting.Commands.RecurringEntries.PauseRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.ResumeRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.CancelRecurringEntry;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class RecurringEntryLifecycleTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private PauseRecurringEntryCommandHandler _pauseHandler = null!;
    private ResumeRecurringEntryCommandHandler _resumeHandler = null!;
    private CancelRecurringEntryCommandHandler _cancelHandler = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);

        _statusLoggerMock = new Mock<IDocumentStatusLogger>();

        _pauseHandler = new PauseRecurringEntryCommandHandler(_dbContext, _userMock.Object, _statusLoggerMock.Object);
        _resumeHandler = new ResumeRecurringEntryCommandHandler(_dbContext, _userMock.Object, _statusLoggerMock.Object);
        _cancelHandler = new CancelRecurringEntryCommandHandler(_dbContext, _userMock.Object, _statusLoggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private async Task<RecurringEntry> SeedEntry(RecurringEntryStatus status = RecurringEntryStatus.Active)
    {
        var entry = new RecurringEntry
        {
            Id = 1,
            EntryNumber = "REC-000001",
            JournalId = 1,
            Name = "Test Entry",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            NextExecutionDate = new DateOnly(2026, 10, 1),
            Status = status,
            IsActive = true
        };
        _dbContext.RecurringEntries.Add(entry);
        await _dbContext.SaveChangesAsync();
        return entry;
    }

    // ===================== T011: Pause Active → Paused =====================

    [Test]
    public async Task Pause_ActiveEntry_StatusBecomesPaused()
    {
        await SeedEntry(RecurringEntryStatus.Active);

        var result = await _pauseHandler.Handle(new PauseRecurringEntryCommand
        {
            Id = 1,
            Reason = "Budget review",
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var entry = await _dbContext.RecurringEntries.FindAsync(1);
        entry!.Status.ShouldBe(RecurringEntryStatus.Paused);
    }

    // ===================== T012: Pause Paused → reject =====================

    [Test]
    public async Task Pause_PausedEntry_Rejects()
    {
        await SeedEntry(RecurringEntryStatus.Paused);

        var result = await _pauseHandler.Handle(new PauseRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only active recurring entries can be paused.");
    }

    // ===================== T013: Pause Completed → reject =====================

    [Test]
    public async Task Pause_CompletedEntry_Rejects()
    {
        await SeedEntry(RecurringEntryStatus.Completed);

        var result = await _pauseHandler.Handle(new PauseRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only active recurring entries can be paused.");
    }

    // ===================== T014: Resume Paused → Active =====================

    [Test]
    public async Task Resume_PausedEntry_StatusBecomesActive()
    {
        await SeedEntry(RecurringEntryStatus.Paused);

        var result = await _resumeHandler.Handle(new ResumeRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var entry = await _dbContext.RecurringEntries.FindAsync(1);
        entry!.Status.ShouldBe(RecurringEntryStatus.Active);
    }

    // ===================== T015: Resume Active → reject =====================

    [Test]
    public async Task Resume_ActiveEntry_Rejects()
    {
        await SeedEntry(RecurringEntryStatus.Active);

        var result = await _resumeHandler.Handle(new ResumeRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Only paused recurring entries can be resumed.");
    }

    // ===================== T016: Cancel Active → Cancelled =====================

    [Test]
    public async Task Cancel_ActiveEntry_StatusBecomesCancelled()
    {
        await SeedEntry(RecurringEntryStatus.Active);

        var result = await _cancelHandler.Handle(new CancelRecurringEntryCommand
        {
            Id = 1,
            Reason = "Project ended",
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var entry = await _dbContext.RecurringEntries.FindAsync(1);
        entry!.Status.ShouldBe(RecurringEntryStatus.Cancelled);
    }

    // ===================== T017: Cancel Cancelled → reject =====================

    [Test]
    public async Task Cancel_CancelledEntry_Rejects()
    {
        await SeedEntry(RecurringEntryStatus.Cancelled);

        var result = await _cancelHandler.Handle(new CancelRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Cancelled recurring entries cannot be cancelled.");
    }

    // ===================== T018: Cancel Completed → reject =====================

    [Test]
    public async Task Cancel_CompletedEntry_Rejects()
    {
        await SeedEntry(RecurringEntryStatus.Completed);

        var result = await _cancelHandler.Handle(new CancelRecurringEntryCommand
        {
            Id = 1,
            RowVersion = []
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Completed recurring entries cannot be cancelled.");
    }

    // ===================== T019: Pause records DocumentStatusLog =====================

    [Test]
    public async Task Pause_RecordsDocumentStatusLog()
    {
        await SeedEntry(RecurringEntryStatus.Active);

        await _pauseHandler.Handle(new PauseRecurringEntryCommand
        {
            Id = 1,
            Reason = "Budget freeze",
            RowVersion = []
        }, CancellationToken.None);

        _statusLoggerMock.Verify(l => l.LogAsync(
            "RecurringEntry",
            1,
            RecurringEntryStatus.Active.ToString(),
            RecurringEntryStatus.Paused.ToString(),
            1,
            "Budget freeze",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===================== T020: Cancel records DocumentStatusLog =====================

    [Test]
    public async Task Cancel_RecordsDocumentStatusLog()
    {
        await SeedEntry(RecurringEntryStatus.Active);

        await _cancelHandler.Handle(new CancelRecurringEntryCommand
        {
            Id = 1,
            Reason = "Project cancelled",
            RowVersion = []
        }, CancellationToken.None);

        _statusLoggerMock.Verify(l => l.LogAsync(
            "RecurringEntry",
            1,
            RecurringEntryStatus.Active.ToString(),
            RecurringEntryStatus.Cancelled.ToString(),
            1,
            "Project cancelled",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
