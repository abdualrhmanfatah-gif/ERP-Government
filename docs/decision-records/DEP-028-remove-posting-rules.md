# DEP-028: Remove PostingRules / PostingRuleLines Engine

**Date**: 2026-09-12
**Status**: Accepted
**Deciders**: Engineering Team
**Related**: DEP-026 (AccountingEvents/AccountBalances removal), DEP-027 (spec 045 TDD exception)
**Implements**: Constitution Principle XII (Controlled Architectural Change)

## Context

The rule-based posting engine (`PostingRules`, `PostingRuleLines`, `PostingPipelineHandler`, `PostingRuleMatcher`, `EventTypeMapper`, `JournalEntryGenerator`) is the last staging mechanism left from the original SPEC-002 pipeline design. It is now unwanted for direct, verifiable accounting:

1. **Only two consumers ever existed** — `PostingPipelineHandler` (matched rules for all domain events) and `PaymentOrderExecutedHandler` (debit/credit for payment orders). Native per-business posting already exists (`CreateAccrualEntry`, `RecordPayment`) and independently performs period checks, numbering via `IDocumentSequenceService`, and balanced-line construction.
2. **Doctrine drift** — seeded rules existed for many event types (PurchaseOrderApproved, CheckCleared, etc.) but only `PaymentOrderExecuted` ever had lines, with **hardcoded account IDs (14, 89)**. Every other matched event produced an empty/incorrect journal entry — the engine generated accounting noise instead of decisions grounded in the chart of accounts.
3. **Rule config is un-audited data** — mapping business events to GL accounts is a financial decision that should be explicit per business flow, not an admin-editable rules table with no enforcement path.
4. **`PaymentOrderExecuted` has no producer** — no code ever raised it; it existed only to feed the engine.

## Decision

1. Remove the engine end-to-end: Domain entities/enums/`PaymentOrderExecuted` event; Application commands/queries/DTOs/`PostingPipelineHandler`/`PostingRuleMatcher`/`EventTypeMapper`/`MoveGenerator`(JournalEntryGenerator); permission codes `Accounting.PostingRules.*`; Web endpoints/policies; Infrastructure configs/seeds (`PostingRuleSeedData`, `PostingRuleLineSeedData`); frontend `accounting-monitoring` feature; rule-only tests.
2. Posting is always native and per-business: handler (e.g. `CreateAccrualEntry`, `RecordPayment`) → domain event → outbox (`OutboxMessages`) → `JournalEntry` + `JournalEntryLine`. Balances stay computed live from lines (DEP-026).
3. Migration `RemovePostingRules` drops `PostingRuleLines` then `PostingRules`; Down recreates the schema from the prior model. Configured rule data dropped intentionally (config-only, no financial-history value); `JournalEntries`/`JournalEntryLines` history untouched. No deletion report table (unlike DEP-026), because the tables hold configuration, not business/audit records.
4. Surviving domain events (`CheckCleared`, `ReceiptVoucherCollected`, `BankReconciliationPosted`, `FiscalYearClosed`, `JournalEntryPosted`, approval/lifecycle events) remain as signals and still flow through the outbox; their posting effects are produced by their owning business handlers.
5. HISTORICAL doc references remain (specs 002/030/033/042/045/047, DEP-026) but are all superscribed as DEP-028-superseded; no live code references remain.

## Scope

- **IN**: the engine, its seeds, permissions, endpoints, frontend monitoring feature, rule-only tests.
- **OUT**: `Journal`/`JournalEntry`/`JournalEntryLine`, `JournalEntryTemplate`, outbox mechanism, native posting handlers, all other business modules.

## Consequences

- **Schema**: 2 fewer tables (`PostingRules`, `PostingRuleLines`) — migration `RemovePostingRules`.
- **Posting**: single, explicit path per business flow; no empty/accidental posting from unmatched rules.
- **Feature registry**: "Accounting Monitoring" posting-rules UI removed; balances/events monitoring already retired (DEP-026).
- **Reversibility**: migration Down recreates schema (data not restored). Outbox idempotency retained.