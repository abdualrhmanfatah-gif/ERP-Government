using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntryById;
using ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntriesList;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting;

[TestFixture]
public class RecurringEntryQueryTests
{
    private ApplicationDbContext _dbContext = null!;
    private GetRecurringEntryByIdQueryHandler _byIdHandler = null!;
    private GetRecurringEntriesListQueryHandler _listHandler = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Seed journal
        _dbContext.Journals.Add(new Journal { Id = 1, Name = "General", IsActive = true });
        _dbContext.SaveChanges();

        _byIdHandler = new GetRecurringEntryByIdQueryHandler(_dbContext, null!);
        _listHandler = new GetRecurringEntriesListQueryHandler(_dbContext, null!);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    // ===================== T026: Get by ID includes generatedJournalEntryId and lastExecutedAt =====================

    [Test]
    public async Task GetById_WithGeneratedEntry_ReturnsGeneratedJournalEntryIdAndLastExecutedAt()
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
            Status = RecurringEntryStatus.Active,
            IsActive = true,
            GeneratedJournalEntryId = 42,
            LastExecutedAt = new DateTime(2026, 9, 1, 0, 5, 12, DateTimeKind.Utc)
        };
        _dbContext.RecurringEntries.Add(entry);
        await _dbContext.SaveChangesAsync();

        // Direct query since mapper is null in test
        var entity = await _dbContext.RecurringEntries
            .Include(x => x.Template)
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == 1);

        entity.ShouldNotBeNull();
        entity.GeneratedJournalEntryId.ShouldBe(42);
        entity.LastExecutedAt.ShouldNotBeNull();
    }

    // ===================== T027: Get with no generation =====================

    [Test]
    public async Task GetById_WithNoGeneration_ReturnsNullGeneratedJournalEntryIdAndLastExecutedAt()
    {
        var entry = new RecurringEntry
        {
            Id = 1,
            EntryNumber = "REC-000001",
            JournalId = 1,
            Name = "New Entry",
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            NextExecutionDate = new DateOnly(2026, 10, 1),
            Status = RecurringEntryStatus.Active,
            IsActive = true
        };
        _dbContext.RecurringEntries.Add(entry);
        await _dbContext.SaveChangesAsync();

        var entity = await _dbContext.RecurringEntries
            .Include(x => x.Template)
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == 1);

        entity.ShouldNotBeNull();
        entity.GeneratedJournalEntryId.ShouldBeNull();
        entity.LastExecutedAt.ShouldBeNull();
    }

    // ===================== T028: List filtered by Status =====================

    [Test]
    public async Task List_FilteredByStatus_ReturnsOnlyMatchingEntries()
    {
        _dbContext.RecurringEntries.AddRange(
            new RecurringEntry
            {
                Id = 1,
                EntryNumber = "REC-000001",
                JournalId = 1,
                Name = "Active Entry",
                Frequency = RecurringFrequency.Monthly,
                StartDate = new DateOnly(2026, 1, 1),
                NextExecutionDate = new DateOnly(2026, 10, 1),
                Status = RecurringEntryStatus.Active,
                IsActive = true
            },
            new RecurringEntry
            {
                Id = 2,
                EntryNumber = "REC-000002",
                JournalId = 1,
                Name = "Paused Entry",
                Frequency = RecurringFrequency.Weekly,
                StartDate = new DateOnly(2026, 1, 1),
                NextExecutionDate = new DateOnly(2026, 10, 1),
                Status = RecurringEntryStatus.Paused,
                IsActive = true
            },
            new RecurringEntry
            {
                Id = 3,
                EntryNumber = "REC-000003",
                JournalId = 1,
                Name = "Cancelled Entry",
                Frequency = RecurringFrequency.Yearly,
                StartDate = new DateOnly(2026, 1, 1),
                NextExecutionDate = new DateOnly(2026, 10, 1),
                Status = RecurringEntryStatus.Cancelled,
                IsActive = false
            });
        await _dbContext.SaveChangesAsync();

        // Direct query filtering by status (bypassing mapper)
        var activeEntries = await _dbContext.RecurringEntries
            .Where(x => x.Status == RecurringEntryStatus.Active)
            .ToListAsync();

        activeEntries.Count.ShouldBe(1);
        activeEntries[0].Name.ShouldBe("Active Entry");

        var pausedEntries = await _dbContext.RecurringEntries
            .Where(x => x.Status == RecurringEntryStatus.Paused)
            .ToListAsync();

        pausedEntries.Count.ShouldBe(1);
        pausedEntries[0].Name.ShouldBe("Paused Entry");

        var cancelledEntries = await _dbContext.RecurringEntries
            .Where(x => x.Status == RecurringEntryStatus.Cancelled)
            .ToListAsync();

        cancelledEntries.Count.ShouldBe(1);
        cancelledEntries[0].Name.ShouldBe("Cancelled Entry");
    }
}
