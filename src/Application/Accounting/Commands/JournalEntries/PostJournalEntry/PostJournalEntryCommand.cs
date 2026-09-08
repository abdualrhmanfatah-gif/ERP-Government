using System.Text.Json;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Accounting;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.PostJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesPost)]
public class PostJournalEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class PostJournalEntryCommandHandler(
    IApplicationDbContext context,
    ICurrencyConversionService conversionService,
    IUser user) : IRequestHandler<PostJournalEntryCommand, Result>
{
    public async Task<Result> Handle(
        PostJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry not found."]);

        // Optimistic concurrency check (FR-021): reject if row version mismatch
        if (entity.RowVersion.Length > 0 && !entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Conflict: journal entry was modified by another user. Reload and retry."]);

        // Validate lifecycle: only Approved entries can be posted (guards re-post immutability, AR-002)
        if (entity.EntryStatus != EntryStatus.Approved)
            return Result.Failure(["Only approved journal entries can be posted."]);

        // Validate entry has lines
        var journalEntryLines = await context.JournalEntryLines
            .Where(x => x.JournalEntryId == entity.Id)
            .ToListAsync(cancellationToken);

        if (journalEntryLines.Count == 0)
            return Result.Failure(["Journal entry must have at least one line."]);

        // Validate fiscal period is open (FR-003, R2)
        var period = await context.FiscalPeriods
            .FindAsync(entity.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure(["Fiscal period not found."]);

        if (period.IsLockedForPosting)
            return Result.Failure(["Fiscal period is locked for posting."]);

        var fiscalYear = await context.FiscalYears
            .FindAsync(entity.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result.Failure(["Fiscal year not found."]);

        if (fiscalYear.Status != FiscalYearStatus.Open)
            return Result.Failure(["Fiscal year is not Open."]);

        if (entity.DocumentDate < period.StartDate || entity.DocumentDate > period.EndDate)
            return Result.Failure(["Document date must be within the fiscal period range."]);

        // Resolve journal entry base currency via Currency.IsBase (Q-NBC-1 → Option C):
        // exactly one active base currency required at runtime; otherwise configuration/data-integrity error.
        var baseCurrencies = await context.Currencies
            .Where(c => c.IsBase && c.IsActive)
            .ToListAsync(cancellationToken);

        if (baseCurrencies.Count != 1)
            return Result.Failure(["Configuration error: exactly one base currency must be flagged (IsBase)."]);

        var baseCurrencyId = baseCurrencies[0].Id;

        // EPHEMERAL base-currency conversion (DEC-002): computed at post, never persisted.
        // Resolve rate for the effective (posting) date; block on missing rate (DEC-001) before any write.
        var effectiveDate = entity.DocumentDate;
        decimal totalBaseDebit = 0m, totalBaseCredit = 0m;
        var conversionResults = new List<JournalEntryLineConversion>();

        foreach (var line in journalEntryLines)
        {
            var conv = await conversionService.ConvertAtPostAsync(
                line, baseCurrencyId, effectiveDate, ExchangeRateType.Official, cancellationToken);

            conversionResults.Add(conv);

            // No applicable rate (incl. no prior rate) → block post, rollback (FR-003a, DEC-001).
            if (!conv.IsBaseCurrency && conv.Rate is null)
            {
                var errorMsg = $"No exchange rate found for currency {line.CurrencyId} on {effectiveDate:yyyy-MM-dd}. Posting blocked until a rate is provided.";
                await WriteAuditLogAsync(entity, journalEntryLines, baseCurrencyId, conversionResults, errorMsg, false, userId, cancellationToken);
                return Result.Failure([errorMsg]);
            }

            totalBaseDebit += conv.BaseDebit;
            totalBaseCredit += conv.BaseCredit;
        }

        // Hard block on ephemeral base double-entry imbalance (financial integrity, Rule XXVI)
        if (totalBaseDebit != totalBaseCredit)
        {
            var imbalanceMsg = "Base double-entry imbalance: TotalBaseDebit != TotalBaseCredit.";
            await WriteAuditLogAsync(entity, journalEntryLines, baseCurrencyId, conversionResults, imbalanceMsg, false, userId, cancellationToken);
            return Result.Failure([imbalanceMsg]);
        }

        // Update journal entry status — set posting metadata (FR-005)
        entity.EntryStatus = EntryStatus.Posted;
        entity.PostingDate = entity.DocumentDate;
        entity.PostedAt = DateTimeOffset.UtcNow;

        // AccountBalances removed (DEP-026) — balances computed live from JournalEntryLines at query time

        // Raised on successful post to complete the posting pipeline (DEC-6)
        entity.AddDomainEvent(new JournalEntryPosted
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            JournalEntryId = entity.Id,
            JournalId = entity.JournalId
        });

        await context.SaveChangesAsync(cancellationToken);

        // AR-001: Write audit log for successful multi-currency posting
        await WriteAuditLogAsync(entity, journalEntryLines, baseCurrencyId, conversionResults, null, true, userId, cancellationToken);

        return Result.Success();
    }

    private async Task WriteAuditLogAsync(
        Domain.Accounting.Entities.JournalEntry journalEntry,
        List<JournalEntryLine> journalEntryLines,
        int baseCurrencyId,
        List<JournalEntryLineConversion> conversionResults,
        string? failureReason,
        bool success,
        int userId,
        CancellationToken cancellationToken)
    {

        // Resolve currency codes for audit trail — fetch all active currencies, filter in memory
        var allCurrencies = await context.Currencies
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        var codeMap = allCurrencies.ToDictionary(c => c.Id, c => c.Code);
        var baseCurrencyCode = codeMap.GetValueOrDefault(baseCurrencyId);

        var lineDetails = journalEntryLines.Select((line, i) => new
        {
            JournalEntryLineId = line.Id,
            CurrencyCode = codeMap.GetValueOrDefault(line.CurrencyId),
            ForeignDebit = line.Debit,
            ForeignCredit = line.Credit,
            ExchangeRateUsed = conversionResults[i].Rate,
            RateDateResolved = conversionResults[i].ResolvedRateDate?.ToString("yyyy-MM-dd"),
            RateType = ExchangeRateType.Official.ToString(),
            BaseDebit = conversionResults[i].BaseDebit,
            BaseCredit = conversionResults[i].BaseCredit,
            IsBaseCurrency = conversionResults[i].IsBaseCurrency
        }).ToList();

        var auditPayload = new
        {
            JournalEntryId = journalEntry.Id,
            DocumentDate = journalEntry.DocumentDate.ToString("yyyy-MM-dd"),
            Lines = lineDetails,
            TotalBaseDebit = conversionResults.Sum(c => c.BaseDebit),
            TotalBaseCredit = conversionResults.Sum(c => c.BaseCredit),
            BaseCurrencyCode = baseCurrencyCode
        };

        var log = new SecurityAuditLog
        {
            EventCategory = "JournalPosting",
            Action = success ? "Post" : "PostFailed",
            UserId = userId,
            EntityName = "JournalEntry",
            EntityId = journalEntry.Id,
            Success = success,
            FailureReason = failureReason,
            NewValues = JsonSerializer.Serialize(auditPayload),
            Timestamp = DateTimeOffset.UtcNow
        };

        context.SecurityAuditLogs.Add(log);
    }
}

public class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
