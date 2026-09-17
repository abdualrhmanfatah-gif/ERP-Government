# Research: Asset Acquisition & Activation

**Feature**: 056-asset-acquisition-activation | **Date**: 2026-09-15

## R1: Journal Entry Creation Pattern

**Decision**: Use inline journal entry creation pattern (similar to RecordPaymentCommand) rather than the RevenueJournalEntryService.

**Rationale**: The RevenueJournalEntryService is registered only for Revenue module use and creates entries in Posted status directly. For asset activation, we need more control over the entry lifecycle (Draft → Posted) and the ability to validate the accounting period before creation. The inline pattern from RecordPaymentCommand provides a clear, self-contained implementation.

**Alternatives considered**: 
- RevenueJournalEntryService — rejected because it's not generalized, creates Posted entries directly, and is not registered for asset use
- Interactive two-step (Create + Submit + Approve + Post) — rejected because activation is a single-step operation per user decision

## R2: Asset Entity JournalEntryId Field

**Decision**: Add `int? JournalEntryId` field to the Asset entity.

**Rationale**: The Asset entity currently has no link to accounting entries. All other asset operations (Disposal, Revaluation, Impairment, Movement, Depreciation) have `JournalEntryId` fields on their respective entities. For activation, the link must be on the Asset itself since activation is the operation that creates the accounting impact.

**Alternatives considered**: 
- Creating a separate AssetActivation entity — rejected because it adds unnecessary complexity; the Asset entity already has ActivationDate and AcquisitionCost fields

## R3: Domain Event Publishing

**Decision**: Raise AssetAcquired event via entity.AddDomainEvent() in the ActivateAssetCommandHandler, let the Outbox pattern handle persistence and publishing.

**Rationale**: The codebase uses the Outbox pattern via DispatchDomainEventsInterceptor. Events are serialized to OutboxMessages during SaveChanges, then processed by OutboxProcessorService. This ensures events are published even if the handler fails after save.

**Alternatives considered**: 
- Direct MediatR.Publish — rejected because it bypasses the Outbox and could lose events on failure
- No event publishing — rejected because the spec requires FR-010 (publish AssetAcquired event)

## R4: Acquisition Account Configuration

**Decision**: Hardcode the acquisition account as a constant in the ActivationConstants file.

**Rationale**: Per user decision (Q4), the acquisition account is a hardcoded reference. This avoids configuration complexity and ensures the account is always available. The account ID will be defined as a constant that can be changed in code if needed.

**Alternatives considered**: 
- SystemSettings table — rejected per user decision
- Per-AssetGroup configuration — rejected per user decision

## R5: Activation Frontend Pattern

**Decision**: Add activation as a separate form component (ActivationForm.tsx) that opens when the user clicks "تفعيل" on a Draft asset.

**Rationale**: The activation form shows a preview of the journal entry before confirmation, similar to how payment recording shows a preview. This gives the user visibility into the accounting impact before committing.

**Alternatives considered**: 
- Inline activation on detail page — rejected because it doesn't provide enough space for journal entry preview
- Modal dialog — rejected because the form needs to show asset details, journal entry preview, andDepreciationStartDate input

## R6: Journal Entry Description Format

**Decision**: Auto-generate description as "استحواذ أصل {Code} - {Name}" per user decision (Q6).

**Rationale**: Consistent, traceable descriptions that directly reference the asset. No user input required, reducing error potential.

**Alternatives considered**: 
- User-entered description — rejected because it adds friction and inconsistency
- Generic "قيد استحواذ أصول" — rejected because it lacks traceability

## R7: Loading State Pattern

**Decision**: Disable the "تفعيل" button and show "جاري التفعيل..." inline while processing per user decision (Q5).

**Rationale**: Prevents duplicate submissions while keeping the form visible. Users can see the context of what's being processed.

**Alternatives considered**: 
- Skeleton/shimmer — rejected because the form should remain visible during processing
- Centered spinner — rejected because it hides the form context

## R8: Permission Reuse

**Decision**: Reuse existing Assets.Update permission for activation (no new permission needed).

**Rationale**: Activation is semantically an update to the asset (status change + field updates). The existing permission infrastructure already supports this. No new permission codes or policies need to be created.

**Alternatives considered**: 
- New Assets.Activate permission — rejected because it adds unnecessary complexity for a single operation
- Assets.Post permission (like AssetDisposals) — rejected because activation is not a separate document type

## R9: Accounting Period Validation

**Decision**: Validate that the activation date falls within an open fiscal period before creating the journal entry.

**Rationale**: FR-015 requires validation that the accounting period is open. The codebase has existing infrastructure for this (FiscalPeriods entity with IsActive and date range). The validation should happen before journal entry creation to provide a clear error message.

**Alternatives considered**: 
- Skip validation — rejected because it violates FR-015
- Validate after journal entry creation — rejected because it would leave an orphaned entry if validation fails
