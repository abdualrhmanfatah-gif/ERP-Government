using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting
{
    [TestFixture]
    public class RecurringEntryProcessorTests
    {
        private Mock<IServiceProvider> _serviceProviderMock = null!;
        private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
        private Mock<IServiceScope> _scopeMock = null!;
        private ServiceProvider _realServiceProvider = null!;
        private ApplicationDbContext _dbContext = null!;
        private RecurringEntryProcessor _processor = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Seed required reference data
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            _dbContext.FiscalYears.Add(new FiscalYear
            {
                Id = 1,
                YearNumber = 2026,
                Name = "2026",
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                Status = FiscalYearStatus.Open,
                IsActive = true
            });
            _dbContext.FiscalPeriods.Add(new FiscalPeriod
            {
                Id = 1,
                FiscalYearId = 1,
                PeriodNumber = today.Month,
                Name = "Current",
                StartDate = new DateOnly(today.Year, today.Month, 1),
                EndDate = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)),
                IsLockedForPosting = false,
                IsActive = true
            });
            _dbContext.Journals.Add(new Journal { Id = 1, Name = "General", IsActive = true });
            _dbContext.SaveChanges();

            // Wire up IServiceProvider to return the real InMemory context
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IApplicationDbContext>(_dbContext);
            serviceCollection.AddSingleton<Microsoft.Extensions.Logging.ILogger<RecurringEntryProcessor>>(
                new Moq.Logging.MockLogger<RecurringEntryProcessor>());
            _realServiceProvider = serviceCollection.BuildServiceProvider();

            // Mock the IBackgroundJob's service provider to create a scope that returns our InMemory context
            _serviceProviderMock = new Mock<IServiceProvider>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();

            // Build a scope that uses the real service provider
            var realScope = _realServiceProvider.CreateScope();
            _scopeMock.Setup(s => s.ServiceProvider).Returns(realScope.ServiceProvider);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
            _serviceProviderMock.Setup(f => f.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);

            _processor = new RecurringEntryProcessor(
                _serviceProviderMock.Object,
                _realServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RecurringEntryProcessor>>());
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _realServiceProvider?.Dispose();
        }

        private RecurringEntry CreateEntry(
            int id = 1,
            int? templateId = 1,
            int journalId = 1,
            RecurringEntryStatus status = RecurringEntryStatus.Active,
            DateOnly? nextExec = null,
            DateOnly? endDate = null,
            decimal? amount = null,
            int? costCenterId = null)
        {
            return new RecurringEntry
            {
                Id = id,
                EntryNumber = $"RE-{id:D4}",
                TemplateId = templateId,
                JournalId = journalId,
                Name = $"Recurring Entry {id}",
                Frequency = RecurringFrequency.Monthly,
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = endDate,
                NextExecutionDate = nextExec ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Status = status,
                IsActive = true,
                Amount = amount,
                CostCenterId = costCenterId
            };
        }

        private async Task SeedTemplate(int templateId = 1, int journalId = 1)
        {
            _dbContext.JournalEntryTemplates.Add(new JournalEntryTemplate
            {
                Id = templateId,
                TemplateName = $"Template {templateId}",
                JournalId = journalId,
                IsActive = true
            });
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedTemplateLines(int templateId = 1, decimal debit1 = 1000, decimal credit2 = 1000)
        {
            _dbContext.JournalEntryTemplateLines.AddRange(
                new JournalEntryTemplateLine
                {
                    TemplateId = templateId,
                    Sequence = 1,
                    AccountId = 100,
                    DebitAmount = debit1,
                    CreditAmount = 0,
                    CurrencyId = 1
                },
                new JournalEntryTemplateLine
                {
                    TemplateId = templateId,
                    Sequence = 2,
                    AccountId = 200,
                    DebitAmount = 0,
                    CreditAmount = credit2,
                    CurrencyId = 1
                });
            await _dbContext.SaveChangesAsync();
        }

        // ===================== US1: Automatic Journal Entry Generation =====================

        [Test]
        public async Task Execute_DueEntry_CreatesJournalEntryWithCorrectFields()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.ToListAsync();
            journalEntries.ShouldNotBeEmpty();
            var journalEntry = journalEntries[0];
            journalEntry.EntryNumber.ShouldStartWith("AUTO-");
            journalEntry.DocumentDate.ShouldBe(DateOnly.FromDateTime(DateTime.UtcNow));
            journalEntry.EntryStatus.ShouldBe(EntryStatus.Draft);
            journalEntry.JournalId.ShouldBe(1);
            journalEntry.IsSystemGenerated.ShouldBeTrue();
        }

        [Test]
        public async Task Execute_DueEntry_CreatesJournalEntryLinesFromTemplate()
        {
            await SeedTemplate();
            await SeedTemplateLines(debit1: 500, credit2: 500);

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.Count.ShouldBe(2);
            lines[0].AccountId.ShouldBe(100);
            lines[0].Debit.ShouldBe(500);
            lines[0].Credit.ShouldBe(0);
            lines[1].AccountId.ShouldBe(200);
            lines[1].Debit.ShouldBe(0);
            lines[1].Credit.ShouldBe(500);
        }

        [Test]
        public async Task Execute_AmountOverride_ProportionalRecalculation()
        {
            await SeedTemplate();
            await SeedTemplateLines(debit1: 1000, credit2: 1000);

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow), amount: 2000);
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.Count.ShouldBe(2);
            // Each line gets full amount (1000/1000 = 100% ratio each)
            // Line 1: Debit=2000, Credit=0 (debit line)
            // Line 2: Debit=0, Credit=2000 (credit line)
            lines[0].Debit.ShouldBe(2000);
            lines[0].Credit.ShouldBe(0);
            lines[1].Debit.ShouldBe(0);
            lines[1].Credit.ShouldBe(2000);
        }

        [Test]
        public async Task Execute_CostCenterOverride_UsesEntryValue()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow), costCenterId: 42);
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.ShouldAllBe(l => l.CostCenterId == 42);
        }

        [Test]
        public async Task Execute_DescriptionTemplate_SetsJournalEntryNarration()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            entry.DescriptionTemplate = "Monthly office rent payment";
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.ToListAsync();
            journalEntries[0].Narration.ShouldBe("Monthly office rent payment");
        }

        // ===================== US2: Execution History and Audit Trail =====================

        [Test]
        public async Task Execute_Success_CreatesExecutionLogWithCorrectStatus()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var logs = await _dbContext.RecurringEntryExecutionLogs.ToListAsync();
            logs.ShouldNotBeEmpty();
            logs[0].RecurringEntryId.ShouldBe(entry.Id);
            logs[0].Status.ShouldBe(RecurringEntryExecutionStatus.Success);
            logs[0].TriggeredBy.ShouldBe("Scheduler");
            logs[0].CompletedAt.ShouldNotBeNull();
            logs[0].GeneratedJournalEntryId.ShouldNotBeNull();
        }

        [Test]
        public async Task Execute_Success_UpdatesLastExecutedAtAndGeneratedJournalEntryId()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var updated = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            updated!.LastExecutedAt.ShouldNotBeNull();
            updated.GeneratedJournalEntryId.ShouldNotBeNull();
        }

        // ===================== US3: Idempotent Execution =====================

        [Test]
        public async Task Execute_AlreadyProcessedToday_SkipsEntry()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            // First run — should succeed
            await _processor.ExecuteAsync(CancellationToken.None);
            var journalEntriesAfterFirst = await _dbContext.JournalEntries.CountAsync();
            journalEntriesAfterFirst.ShouldBe(1);

            // Re-query entry to get updated NextExecutionDate (now in the future)
            var updatedEntry = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            updatedEntry!.NextExecutionDate = DateOnly.FromDateTime(DateTime.UtcNow); // Reset to today
            await _dbContext.SaveChangesAsync();

            // Add a successful log for today to simulate prior success
            _dbContext.RecurringEntryExecutionLogs.Add(new RecurringEntryExecutionLog
            {
                RecurringEntryId = entry.Id,
                ExecutionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = RecurringEntryExecutionStatus.Success,
                GeneratedJournalEntryId = 1,
                TriggeredBy = "Scheduler"
            });
            await _dbContext.SaveChangesAsync();

            // Second run — should skip
            await _processor.ExecuteAsync(CancellationToken.None);
            var journalEntriesAfterSecond = await _dbContext.JournalEntries.CountAsync();
            journalEntriesAfterSecond.ShouldBe(1); // No new JournalEntry created
        }

        [Test]
        public async Task Execute_FailedLogExists_RetriesEntry()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            // Add a failed log for today
            _dbContext.RecurringEntryExecutionLogs.Add(new RecurringEntryExecutionLog
            {
                RecurringEntryId = entry.Id,
                ExecutionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = RecurringEntryExecutionStatus.Failed,
                ErrorMessage = "Previous error",
                TriggeredBy = "Scheduler"
            });
            await _dbContext.SaveChangesAsync();

            // Run — should retry (failed log doesn't block)
            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(1); // New JournalEntry created
        }

        // ===================== US4: Failure Isolation =====================

        [Test]
        public async Task Execute_EntryWithMissingTemplate_SkipsWithoutCrash()
        {
            var entry = CreateEntry(templateId: 999, nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_Failure_DoesNotAdvanceNextExecutionDate()
        {
            var entry = CreateEntry(templateId: 999, nextExec: new DateOnly(2026, 8, 15));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var updated = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            updated!.NextExecutionDate.ShouldBe(new DateOnly(2026, 8, 15)); // Not advanced
        }

        // ===================== US5: Pause, Resume, Cancel =====================

        [Test]
        public async Task Execute_PausedEntry_IsSkipped()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(status: RecurringEntryStatus.Paused, nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_CompletedEntry_IsSkipped()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(status: RecurringEntryStatus.Completed, nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_EndDateReached_SetsStatusToCompleted()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(
                nextExec: new DateOnly(2026, 1, 31),
                endDate: new DateOnly(2026, 1, 31));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var updated = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            updated!.Status.ShouldBe(RecurringEntryStatus.Completed);
        }

        // ===================== US6: Missed-Run Handling =====================

        [Test]
        public async Task Execute_MissedRun_NextDateAdvancesFromOriginalNotToday()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: new DateOnly(2026, 8, 15));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var updated = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            // Monthly from Aug 15 → Sep 15 (not from today)
            updated!.NextExecutionDate.ShouldBe(new DateOnly(2026, 9, 15));
        }

        [Test]
        public async Task Execute_MissedRun_CreatesSingleJournalEntry()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: new DateOnly(2026, 8, 28));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(1);
        }

        // ===================== Phase 9: Polish =====================

        [Test]
        public async Task Execute_ImbalancedTemplate_RejectsWithError()
        {
            await SeedTemplate();
            // Imbalanced: debit=100, credit=0 on line1; debit=0, credit=80 on line2
            _dbContext.JournalEntryTemplateLines.AddRange(
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 1,
                    AccountId = 100,
                    DebitAmount = 100,
                    CreditAmount = 0,
                    CurrencyId = 1
                },
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 2,
                    AccountId = 200,
                    DebitAmount = 0,
                    CreditAmount = 80,
                    CurrencyId = 1
                });
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);

            // Execution log should show failure
            var logs = await _dbContext.RecurringEntryExecutionLogs.ToListAsync();
            logs.ShouldNotBeEmpty();
            logs.Any(l => l.Status == RecurringEntryExecutionStatus.Failed).ShouldBeTrue();

            // NextExecutionDate should NOT be advanced
            var updated = await _dbContext.RecurringEntries.FindAsync(entry.Id);
            updated!.NextExecutionDate.ShouldBe(DateOnly.FromDateTime(DateTime.UtcNow));
        }

        [Test]
        public async Task Execute_CurrencyId_FromTemplateLine()
        {
            await SeedTemplate();
            _dbContext.JournalEntryTemplateLines.AddRange(
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 1,
                    AccountId = 100,
                    DebitAmount = 500,
                    CreditAmount = 0,
                    CurrencyId = 5
                },
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 2,
                    AccountId = 200,
                    DebitAmount = 0,
                    CreditAmount = 500,
                    CurrencyId = 5
                });
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.Count.ShouldBe(2);
            lines.All(l => l.CurrencyId == 5).ShouldBeTrue();
        }

        [Test]
        public async Task Execute_CurrencyId_FallsBackToBaseCurrency()
        {
            await SeedTemplate();
            _dbContext.JournalEntryTemplateLines.AddRange(
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 1,
                    AccountId = 100,
                    DebitAmount = 500,
                    CreditAmount = 0,
                    CurrencyId = null // No currency set
                },
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 2,
                    AccountId = 200,
                    DebitAmount = 0,
                    CreditAmount = 500,
                    CurrencyId = null
                });
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.Count.ShouldBe(2);
            lines.All(l => l.CurrencyId == 1).ShouldBeTrue(); // Fallback to base currency
        }

        [Test]
        public async Task Execute_AmountOverride_UnequalRatios_ProportionalRecalculation()
        {
            await SeedTemplate();
            // Line1: Debit=60, Line2: Debit=0, Credit=60 → ratio is 60/60=100% for line1
            _dbContext.JournalEntryTemplateLines.AddRange(
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 1,
                    AccountId = 100,
                    DebitAmount = 60,
                    CreditAmount = 0,
                    CurrencyId = 1
                },
                new JournalEntryTemplateLine
                {
                    TemplateId = 1,
                    Sequence = 2,
                    AccountId = 200,
                    DebitAmount = 0,
                    CreditAmount = 60,
                    CurrencyId = 1
                });
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow), amount: 1000);
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var lines = await _dbContext.JournalEntryLines.ToListAsync();
            lines.Count.ShouldBe(2);
            // Both lines get 1000 each (equal ratio from 60:60 → debit=1000, credit=1000)
            lines[0].Debit.ShouldBe(1000);
            lines[1].Credit.ShouldBe(1000);
        }

        [Test]
        public async Task Execute_MultipleEntries_PartialFailure_IndependentlyProcessed()
        {
            await SeedTemplate();
            await SeedTemplateLines(debit1: 100, credit2: 100);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Entry 1: valid
            var entry1 = CreateEntry(id: 1, nextExec: today);
            // Entry 2: invalid (no template)
            var entry2 = CreateEntry(id: 2, templateId: 999, nextExec: today);
            // Entry 3: valid
            var entry3 = CreateEntry(id: 3, nextExec: today);

            _dbContext.RecurringEntries.AddRange(entry1, entry2, entry3);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(2); // Entry 1 and 3 succeed

            var entry1Updated = await _dbContext.RecurringEntries.FindAsync(entry1.Id);
            entry1Updated!.LastExecutedAt.ShouldNotBeNull();

            var entry2Updated = await _dbContext.RecurringEntries.FindAsync(entry2.Id);
            entry2Updated!.LastExecutedAt.ShouldBeNull(); // Not processed
            entry2Updated!.NextExecutionDate.ShouldBe(today); // Not advanced

            var entry3Updated = await _dbContext.RecurringEntries.FindAsync(entry3.Id);
            entry3Updated!.LastExecutedAt.ShouldNotBeNull();
        }

        [Test]
        public async Task Execute_TemplateIdNull_SkipsEntry()
        {
            var entry = CreateEntry(templateId: null, nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_InactiveJournal_SkipsEntry()
        {
            await SeedTemplate();
            await SeedTemplateLines();
            // Deactivate the journal
            var journal = await _dbContext.Journals.FindAsync(1);
            journal!.IsActive = false;
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_NoFiscalPeriod_SkipsEntry()
        {
            await SeedTemplate();
            await SeedTemplateLines();
            // Remove all fiscal periods
            _dbContext.FiscalPeriods.RemoveRange(_dbContext.FiscalPeriods);
            await _dbContext.SaveChangesAsync();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task Execute_ZeroTemplateLines_SkipsEntry()
        {
            await SeedTemplate();
            // No template lines seeded

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.CountAsync();
            journalEntries.ShouldBe(0);
        }

        [Test]
        public async Task DEBUG_Diagnose_ProcessorFailure()
        {
            await SeedTemplate();
            await SeedTemplateLines();

            var entry = CreateEntry(nextExec: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.RecurringEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            // Manually test what the processor does step by step
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // 1. Query due entries
            var dueEntries = await _dbContext.RecurringEntries
                .Where(r => r.Status == RecurringEntryStatus.Active
                         && r.NextExecutionDate <= today
                         && r.IsActive)
                .ToListAsync();
            dueEntries.Count.ShouldBe(1, "Should find 1 due entry");

            // 2. Check idempotency
            var hasLog = await _dbContext.RecurringEntryExecutionLogs
                .AnyAsync(l => l.RecurringEntryId == entry.Id
                            && l.ExecutionDate == today
                            && l.Status == RecurringEntryExecutionStatus.Success);
            hasLog.ShouldBeFalse("Should have no successful log");

            // 3. Check template
            var template = await _dbContext.JournalEntryTemplates
                .FirstOrDefaultAsync(t => t.Id == entry.TemplateId!.Value && t.IsActive);
            template.ShouldNotBeNull("Template should exist");

            // 4. Check template lines
            var templateLines = await _dbContext.JournalEntryTemplateLines
                .Where(l => l.TemplateId == template.Id)
                .ToListAsync();
            templateLines.Count.ShouldBe(2, "Should have 2 template lines");

            // 5. Check journal
            var journal = await _dbContext.Journals
                .FirstOrDefaultAsync(j => j.Id == entry.JournalId && j.IsActive);
            journal.ShouldNotBeNull("Journal should exist");

            // 6. Check fiscal period
            var period = await _dbContext.FiscalPeriods
                .FirstOrDefaultAsync(p => p.IsActive
                    && !p.IsLockedForPosting
                    && p.StartDate <= today
                    && p.EndDate >= today);
            period.ShouldNotBeNull("FiscalPeriod should exist");

            // 7. Check fiscal year
            var year = await _dbContext.FiscalYears
                .FirstOrDefaultAsync(f => f.Id == period.FiscalYearId && f.IsActive);
            year.ShouldNotBeNull("FiscalYear should exist");

            // 8. Try the actual processor with exception observation
            // The processor catches exceptions — let's run it directly
            await _processor.ExecuteAsync(CancellationToken.None);

            var journalEntries = await _dbContext.JournalEntries.ToListAsync();
            TestContext.Out.WriteLine($"JournalEntries count after processor: {journalEntries.Count}");
            var logs = await _dbContext.RecurringEntryExecutionLogs.ToListAsync();
            TestContext.Out.WriteLine($"Logs count: {logs.Count}");
            foreach (var log in logs)
            {
                TestContext.Out.WriteLine($"  Log: Status={log.Status}, Error={log.ErrorMessage}");
            }
        }
    }
}

namespace Moq.Logging
{
    public class MockLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
