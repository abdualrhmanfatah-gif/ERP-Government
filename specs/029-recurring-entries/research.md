# Research: Recurring Entries (ACC-04)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

## R1: DocumentSequenceService Integration

**Decision**: Use `IDocumentSequenceService.GenerateNextNumberAsync("RecurringEntry")` in `CreateRecurringEntryCommand`.

**Rationale**: FR-008 requires unique entry numbers via the document sequence service. The existing timestamp pattern (`RE-{DateTime.UtcNow:yyyyMMddHHmmss}`) is fragile and not guaranteed unique under concurrent creation.

**Evidence**:
- `IDocumentSequenceService` interface: `src/Application/FinancialSettings/Common/Services/IDocumentSequenceService.cs:3`
- Implementation: `src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs:6`
- PrefixMap already has `"JournalEntry" = "JRN"` at line 18 — add `"RecurringEntry" = "REC"` prefix
- Format: `{PREFIX}-{D6}` (line 70)
- DI registration: `src/Application/DependencyInjection.cs:36`
- Pattern exemplar: `src/Application/Budgeting/Commands/Appropriations/ApproveAppropriationCommand.cs` uses sequence service

**Alternatives considered**:
- Timestamp-based: rejected — not unique under concurrency, not sequential
- Database MAX+1: rejected — race condition, conflicts with established pattern

**Action**: Add `"RecurringEntry" = "REC"` to PrefixMap in DocumentSequenceService. Replace timestamp in CreateRecurringEntryCommand with sequence call.

## R2: DocumentStatusLogger for Lifecycle Transitions

**Decision**: Inject `IDocumentStatusLogger` in Pause, Resume, and Cancel handlers to record status transitions.

**Rationale**: FR-007 requires persisting pause reason and cancel reason as audit trail. The existing `DocumentStatusLog` entity and `IDocumentStatusLogger` service are the established pattern for this.

**Evidence**:
- Interface: `src/Application/Parties/Common/IDocumentStatusLogger.cs:3`
- Implementation: `src/Application/Security/Common/DocumentStatusLogger.cs:7`
- Entity: `src/Domain/Security/Entities/DocumentStatusLog.cs:5` — fields: EntityName, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt, Reason
- Usage pattern (46 call sites): e.g., `src/Application/Budgeting/Commands/Appropriations/ApproveAppropriationCommand.cs:48`
- LogAsync signature: `LogAsync(entityName, documentId, fromStatus, toStatus, changedById, reason, ct)`

**Key detail**: `changedById` requires `IUser.UserId`. Current handlers don't inject `IUser`. Must add `IUser` to handler constructor.

**Action**: 
1. Add `IUser` and `IDocumentStatusLogger` to Pause/Resume/Cancel handler constructors
2. Call `LogAsync("RecurringEntry", entity.Id, oldStatus.ToString(), newStatus.ToString(), userId, reason, ct)` after status change
3. Pause/Cancel reasons stored via the `reason` parameter

**Alternatives considered**:
- Inline approval columns: rejected — Constitution Principle VIII mandates append-only audit trails
- Separate audit entity: rejected — `DocumentStatusLog` is the established pattern

## R3: Fiscal Period Closure Check on Creation

**Decision**: Check that the fiscal period for `StartDate` is open when creating a schedule.

**Rationale**: FR-010 requires server-side rejection of schedule execution in closed fiscal periods. While the processor already checks this at execution time, rejecting at creation time prevents obviously invalid schedules.

**Evidence**:
- FiscalPeriod entity: `src/Domain/FinancialSettings/Entities/FiscalPeriod.cs:5` — `IsLockedForPosting` bool
- Pattern from JournalEntry creation: `src/Application/Accounting/Commands/JournalEntries/CreateJournalEntry/CreateJournalEntryCommand.cs:54`
  ```csharp
  if (period.IsLockedForPosting)
      return Result<int>.Failure(["Fiscal period is locked for posting."]);
  ```
- RecurringEntryProcessor already does this check at execution time (line 156-160)

**Decision**: **SKIP creation-time check**. The processor already handles this at execution time, and the spec says "schedule execution" is rejected — not creation. Creating a schedule that starts in a future open period is valid. The processor's existing check is sufficient.

**Rationale for skip**: A schedule created today with a future start date should not be blocked if today's period is closed. The processor checks the period at execution time, which is the correct enforcement point.

## R4: Amount Validation (FR-017)

**Decision**: Add validation rule: if `TemplateId` is null, `Amount` must be present (not null).

**Rationale**: FR-017 requires rejecting creation if neither schedule amount nor template amount is provided. When `TemplateId` is provided, the template's amounts may supply the value. When `TemplateId` is absent, the schedule must carry its own amount.

**Evidence**:
- Current validator: `CreateRecurringEntryCommandValidator` at `src/Application/Accounting/Commands/RecurringEntries/CreateRecurringEntry/CreateRecurringEntryCommand.cs:73`
- Processor amount calculation: `src/Infrastructure/Services/RecurringEntryProcessor.cs:186` — `AmountCalculator.CalculateAmounts(templateLines, entry.Amount)` — if `entry.Amount` is null and template lines have amounts, it works; if both null, balance check fails at line 191

**Action**: Add conditional validation rule in `CreateRecurringEntryCommandValidator`:
```csharp
When(x => !x.TemplateId.HasValue, () =>
{
    RuleFor(x => x.Amount)
        .NotNull().WithMessage("Amount is required when no template is specified.");
});
```

**Alternatives considered**:
- Check template amount at creation time: rejected — template amounts are line-level, not header-level; the processor resolves them at execution time

## R5: EndDate >= StartDate Validation (FR-009)

**Decision**: Add validation rule in `CreateRecurringEntryCommandValidator`.

**Action**: Add rule:
```csharp
When(x => x.EndDate.HasValue, () =>
{
    RuleFor(x => x.EndDate)
        .GreaterThanOrEqualTo(x => x.StartDate)
        .WithMessage("End date must not be before start date.");
});
```

**Evidence**: FR-009, Spec acceptance scenario 3.

## R6: Cancelled Enum Value

**Decision**: Add `Cancelled = 3` to `RecurringEntryStatus`.

**Rationale**: OQ1 clarified this — add `Cancelled` as 4th value. The existing `CancelRecurringEntryCommand` currently sets `Status = Completed` which is incorrect per spec.

**Evidence**:
- Current enum: `src/Domain/Accounting/Enums/RecurringEntryStatus.cs` — Active=0, Paused=1, Completed=2
- Current cancel handler: `src/Application/Accounting/Commands/RecurringEntries/CancelRecurringEntry/CancelRecurringEntryCommand.cs:32` — uses `Completed`

**Impact on processor**: `RecurringEntryProcessor` queries `Status == Active` — no impact since Cancelled != Active.
**Impact on queries**: `GetRecurringEntriesListQuery` filters by Status — no impact since Cancelled is a valid filter value.

**Action**: 
1. Add `Cancelled = 3` to enum
2. Update `CancelRecurringEntryCommand` to use `Cancelled` instead of `Completed`

## R7: Frontend Approach

**Decision**: Follow existing feature folder pattern from `src/Web/ClientApp/src/features/budgeting/`.

**Rationale**: Consistent with codebase conventions. The frontend for this feature should be in `src/Web/ClientApp/src/features/accounting/recurring-entries/`.

**Pattern from budgeting**:
- `shared/types.ts` — TypeScript types matching backend DTOs
- `shared/client.ts` — API fetch wrappers
- `hooks/useRecurringEntries.ts` — React Query hooks for CRUD + lifecycle
- `pages/RecurringEntriesListPage.tsx` — list view with status badges
- `pages/RecurringEntryDetailPage.tsx` — detail view with lifecycle actions
- `pages/RecurringEntryCreatePage.tsx` — creation form

**NSwag**: `npm run generate-api` regenerates typed clients from OpenAPI spec. Frontend can use generated clients directly or manual fetch wrappers.

**Action**: Covered in tasks.md (Phase 2), not detailed in plan.
