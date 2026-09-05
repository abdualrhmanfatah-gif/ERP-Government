# Feature Specification: Budgeting Backend Completion — Encumbrances, Sequences, Transfers, Availability API, Drift Migration, Monthly Plan

**Feature Branch**: `015-budgeting-backend-completion`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Finish the budgeting rebuild (spec 013 ~58% done, 51 open tasks — this spec absorbs the remainder and supersedes 013). Fix critical runtime bugs: APR/ENC numbers never assigned (unique-index collision on second row), model↔DB drift columns. Deliver full Encumbrance stack, sequence wiring + race fix, Transfer Appropriation pairs, availability API, schema-drift migration, BudgetItemMonthlyPlans, budget head wiring, cleanup + docs + tests."

## Context

Spec 013 delivered BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation stacks (commands/queries/endpoints) but is ~58% complete with 51 open tasks. This spec absorbs the remainder and closes 013 as **superseded**.

As-built, verified:

- Entities exist: BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance (int PKs, audit base + concurrency token, enums stored as int, decimal(23,2)).
- Commands/queries/endpoints exist for BudgetTypes, Funds, BudgetClassifications, Budgets, BudgetItems, Appropriations (minimal-API endpoint groups, `/api/{ClassName}`, PATCH lifecycle routes, PermissionCodes).
- **Encumbrances**: entity + config + DbSet + permission codes ONLY — zero commands/queries/DTOs/endpoints. Biggest gap.
- **BudgetAvailabilityService** computes whole-BudgetItem availability: net active Appropriations (Original / + Supplement / + Adjustment / − Reduction; Transfer nets 0) − open Encumbrances (Active / PartiallyReleased / PartiallyLiquidated, non-reversal). Consumed only by activation; no API exposure.
- **DocumentSequenceService** formats `{prefix}-{D6}`; Budget/Appropriation/Encumbrance prefixes (BGT/APR/ENC) seeded but NEVER called by budgeting commands; contains a read-back race (increment then re-read). **Critical runtime bug**: APR/ENC numbers are never assigned, so the unique index collides on the second row.
- **Schema drift**: single initial migration predates final entity shape — DB missing BudgetItems.OriginalAmount + Remarks, Budgets.IsActive; column-length drift on Funds (FundNumber/LegalAuthority/Description), Budgets/Encumbrances (Description); a stale FK (Encumbrances.VendorId → Suppliers) exists in the model snapshot but not in config.

## Clarifications

### Session 2026-09-05

- Q: From which encumbrance statuses is Reversal allowed? → A: Active and Suspended only (live commitments; Closed/Cancelled/Reversed reject reversal; partially-released/liquidated states unreachable in this spec).
- Q: Must the transfer source item hold sufficient positive appropriation to cover the negative source row at create time? → A: Yes — hard validation at create; otherwise the pair could push the source item's computed availability negative.
- Q: Regenerate the NSwag client or delete the stale EncumbrancesClient? → A: Regenerate via NSwag after endpoints land; wrappers consume the generated client.

Governing principles (Constitution): III (server-side rules, lifecycle guards), IV (financial integrity, reversal-only corrections), V (budget control before expenditure, explicit recorded overrides), VI (migrations, concurrency tokens, no cascades), VII/VIII (permissions + approval history on every decision), IX (OpenAPI contract), XI (TDD non-negotiable).

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Unique Document Numbers on Every Budgeting Document (Priority: P1)

A budget officer creates a Budget, then an Appropriation, then an Encumbrance. Each document receives a system-generated, immutable number (BGT-000001, APR-000001, ENC-000001). The user never types a document number. Creating a second document of the same type always succeeds with the next number — never a unique-index collision.

**Why this priority**: Active runtime bug — the second Appropriation/Encumbrance row collides on the unique index and fails. Nothing downstream (approvals, postings, encumbrances) is trustworthy until numbers are unique and immutable.

**Independent Test**: Create two documents of each kind in parallel; verify both succeed, numbers are distinct, format is `{prefix}-{6 digits}`, and the number cannot be changed by any later edit.

**Acceptance Scenarios**:

1. **Given** a fresh system, **When** a Budget is created, **Then** BudgetNumber is assigned by the system (BGT prefix) and no user-supplied number is accepted.
2. **Given** one Appropriation exists, **When** a second Appropriation is created concurrently, **Then** both persist with distinct APR numbers and no collision error.
3. **Given** an approved Appropriation, **When** any update is attempted, **Then** its APR number is unchanged.
4. **Given** two simultaneous sequence requests, **When** both complete, **Then** no number is ever returned twice (atomic increment; read-back race eliminated).
5. **Given** a sequence failure (exhausted range), **When** creation is attempted, **Then** the failure is an explicit typed failure, never a silent "Error: ..." string result.

---

### User Story 2 — Full Encumbrance Document Lifecycle (Priority: P1)

A procurement officer reserves funds against an approved Appropriation by creating an Encumbrance (number ENC, system-generated). The document follows the standard lifecycle: Draft → Submitted → Approved → Active → (Suspended / PartiallyReleased / PartiallyLiquidated states as applicable) → Closed or Cancelled. Availability is checked at creation per the budget's control method. Posted encumbrances are only ever corrected by reversal, never edited.

**Why this priority**: Encumbrances are the "reserve budget before payment" link in the Budget → Appropriation → Encumbrance → Payment chain (Constitution V). Without them, commitments cannot be tracked and payments cannot be gated.

**Independent Test**: Walk an encumbrance through every lifecycle transition; verify guards (only Draft can update/delete), approval history on every transition, computed IsReversed flag, and availability gate behavior per control method.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with available balance and control method None, **When** an Encumbrance exceeding availability is created, **Then** it is created without warning.
2. **Given** control method Warning and over-availability, **When** an Encumbrance is created, **Then** it is allowed and the override is recorded in ApprovalHistory with reason.
3. **Given** control method Blocking and over-availability, **When** an Encumbrance is created, **Then** creation is rejected with an explicit failure.
4. **Given** a Draft encumbrance, **When** updated or deleted, **Then** it succeeds; **Given** any non-Draft status, **When** updated or deleted, **Then** it is rejected.
5. **Given** a posted (Active) encumbrance, **When** a reversal is requested, **Then** a new negative-effect row referencing the original is created, the original becomes Reversed, availability is restored computationally, and the original row's financial fields are never mutated.
6. **Given** an encumbrance that has been reversed, **When** listed or fetched, **Then** its DTO shows the computed IsReversed flag (a referencing row exists; no stored flag).
7. **Given** list filters (AppropriationId, Type, Status, date range, reversal-state), **When** the list endpoint is called, **Then** only matching rows return.
8. **Given** any lifecycle transition (submit/approve/activate/suspend/close/cancel/reverse), **Then** an ApprovalHistory row is recorded (current Decision shape "from -> to"; the Action-enum refactor is deferred to the party/documents spec and MUST NOT be pre-implemented here).

---

### User Story 3 — Transfer Appropriation Pairs (Priority: P2)

A budget officer moves funds from one BudgetItem to another within a Budget. The system atomically creates a PAIR of Appropriation rows on one command: a negative-effect row on the source item and a positive-effect row on the target item (recorded via its TargetBudgetItemId). Both rows share status transitions forever, and the pair is net zero at Budget level.

**Why this priority**: Transfers are the only sanctioned way to move approved funds between items; without the pair, one side of the move is invisible.

**Independent Test**: Create a transfer; verify exactly two rows, opposite effects, shared lifecycle, and that item-level availability nets them to zero (no double counting).

**Acceptance Scenarios**:

1. **Given** a Draft transfer request between two items of the same Budget, **When** created, **Then** exactly two Appropriation rows are created atomically (all-or-nothing) — a negative row on the source item and a positive row on the target item via TargetBudgetItemId.
2. **Given** a transfer pair, **When** either row's status transitions (submit/approve/activate/...), **Then** both rows transition together.
3. **Given** a transfer pair, **When** item-level availability is computed, **Then** transfer rows contribute net zero at the item level (unchanged from current service semantics).
4. **Given** a transfer where source and target are the same item, items from different Budgets, or a source item with insufficient positive active appropriation, **When** attempted, **Then** it is rejected by validation.

---

### User Story 4 — Availability Query API (Priority: P2)

An analyst opens a BudgetItem (or any Appropriation on it) and sees current computed figures: netAppropriated, encumbered, available, plus the resolved effective overrun policy (item → budget → type chain). Control point is the BudgetItem — multiple Appropriations on one item net together.

**Why this priority**: Makes the availability engine user-visible for the first time (spec 013 T094); required for encumbrance/payment decisions and reporting.

**Independent Test**: Seed known appropriations/encumbrances on an item; call both endpoints; verify figures match the service's whole-item semantics.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with appropriations and encumbrances, **When** GET item availability is called, **Then** netAppropriated / encumbered / available match: net active appropriations minus open encumbrances.
2. **Given** an Appropriation, **When** GET appropriation availability is called, **Then** the same whole-item figures for its owning BudgetItem are returned (control point is the item).
3. **Given** AllowOverrun configured differently on item vs budget vs type, **When** availability is queried, **Then** the resolved EffectiveAllowOverrun (most specific wins) is included.
4. **Given** a nonexistent id, **When** either availability endpoint is called, **Then** a not-found failure returns.

---

### User Story 5 — Budget Head Fields and Monthly Plans (Priority: P2)

A budget officer enters the budget's planned TotalAmount, AllowOverrun, effective date window, and Description when creating/updating a Budget — today these are ignored (TotalAmount always 0). Separately, they may record per-month planned amounts (1–12) on a BudgetItem as informational/statistical input for reports; monthly plans carry no status and no enforcement anywhere.

**Why this priority**: Fixes a visible data bug (head totals always 0) and delivers the monthly plan reports input (m41/45 decision); both are independent of document flows.

**Independent Test**: Create/update a Budget with head fields and verify persistence; PUT a 12-entry monthly plan and GET it back; verify uniqueness per (item, month) and non-negative amounts.

**Acceptance Scenarios**:

1. **Given** a Budget create/update with TotalAmount, AllowOverrun, EffectiveFrom, EffectiveTo, Description, **When** submitted, **Then** all fields persist (TotalAmount no longer always 0). EffectiveTo stays nullable.
2. **Given** approval or activation of a Budget, **Then** only Status changes (no stored active flag is written or read).
3. **Given** a monthly plan PUT with 12 entries, **When** repeated for the same item, **Then** it is replaced idempotently (batch 12) and GET returns the current plan.
4. **Given** a monthly plan entry with a negative amount, month outside 1–12, or a duplicate month, **When** submitted, **Then** it is rejected.
5. **Given** monthly plans exist, **Then** no workflow, availability, or posting behavior reads them — they are statistical input for reports only.

---

### User Story 6 — Schema Drift Migration and Cleanup (Priority: P3)

The database is brought in line with the model by one versioned migration, stale frontend code is aligned with the new endpoints, and docs are updated. Zero-stored-aggregates design is enforced: item-level computed amounts are never stored.

**Why this priority**: Necessary for correctness and hygiene but adds no user-facing capability on its own.

**Independent Test**: Run migration on a database built from the initial migration; verify drops/adds/length changes; run the app; verify stale frontend wrappers replaced; verify docs exist.

**Acceptance Scenarios**:

1. **Given** the initial-migration database, **When** the new migration applies, **Then**: BudgetItems.OriginalAmount is dropped (zero-stored-aggregates), Budgets.IsActive is dropped, BudgetItems.Remarks is kept (name aligned consistently across entity/config/DTO — one name only), column lengths aligned on Funds/Budgets/Encumbrances (configs win), Appropriation.TargetBudgetItemId added (nullable FK, indexed, Restrict), BudgetItemMonthlyPlans table created with UNIQUE (BudgetItemId, Month), and the stale Encumbrances→Suppliers FK is dropped (plain index kept).
2. **Given** the frontend, **When** encumbrance features are used, **Then** the hooks/shared wrappers match the new endpoints and consume the regenerated NSwag client (no stale dead client remains).
3. **Given** the sequence admin form, **Then** the 'Liquidation' option is removed.
4. **Given** docs, **Then**: FM-004 Budget Liquidations marked REMOVED; registry BF-002/BF-003 notes updated (encumbrances live, transfers, monthly plan); docs/database-schema.md created with budgeting tables as first section (later specs append).
5. **Given** spec 013, **Then** its spec.md status line marks it superseded by this spec.

---

### Edge Cases

- Concurrent creation of two encumbrances consuming the last available balance — one must fail cleanly (server-side availability check at creation; no double-reservation).
- Reverse an already-reversed encumbrance — must be rejected.
- Activate an encumbrance on a Budget whose status no longer permits it — guarded by lifecycle rules.
- Transfer where source item lacks sufficient positive active appropriation to cover the negative row — rejected at create (hard validation, FR-15).
- Sequence prefix missing or exhausted — typed failure, creation blocked (never a null or duplicate number).
- Update carrying a stale concurrency token — conflict surfaced distinctly per the error contract.
- Monthly plan for an item in a Budget not yet Active — allowed (informational), no enforcement.
- Delete attempt on posted encumbrance — rejected (Draft-only delete; posted rows only reverse).

## Requirements *(mandatory)*

### Functional Requirements

**Sequences (US1)**

- **FR-1**: System MUST assign BudgetNumber (BGT), APR, and ENC numbers via the document sequence service at creation; user-supplied numbers MUST be removed from the budget create path (command + validation).
- **FR-2**: Document numbers MUST be immutable after creation.
- **FR-3**: Sequence allocation MUST be a single atomic operation; two concurrent callers MUST NEVER receive the same number.
- **FR-4**: Sequence failures MUST surface as typed failures, not string-error results. Format MUST remain `{prefix}-{D6}`.

**Encumbrances (US2)**

- **FR-5**: System MUST provide full Encumbrance stack matching Appropriation conventions: list query (filters: AppropriationId, Type, Status, date range, reversal-state), get-by-id, create, update (Draft only), delete (Draft only), submit, approve, activate, suspend, close, cancel, reverse.
- **FR-6**: Encumbrance create MUST check availability via the availability service per budget control method: None → skip; Warning → allow + record override in ApprovalHistory reason; Blocking → reject.
- **FR-7**: Encumbrance reversal MUST be allowed only from Active or Suspended; reversal from Closed, Cancelled, Draft, or already-Reversed MUST be rejected. Reversal MUST create a new negative-effect row with ReversalOfId = original, set the original to Reversed, and never mutate the original's financial fields. Availability MUST be restored computationally (no stored aggregates).
- **FR-8**: Encumbrance DTO MUST expose a computed IsReversed flag (existence of a referencing row); no stored flag.
- **FR-9**: Every Encumbrance lifecycle transition MUST record an ApprovalHistory row (DocumentType "Encumbrance", Decision "from -> to" — current shape).
- **FR-10**: All encumbrance operations MUST require their named permissions, at both the endpoint and use-case layers.
- **FR-11**: Every encumbrance update MUST verify the optimistic-concurrency token.

**Transfers (US3)**

- **FR-12**: A Transfer MUST be created as an atomic PAIR of Appropriation rows (negative-effect source row; positive-effect target row via TargetBudgetItemId), net zero, one command.
- **FR-13**: Transfer pair rows MUST share status transitions (transitioning one transitions both).
- **FR-14**: Item-level availability MUST continue to treat Transfer rows as net zero (existing service semantics preserved).
- **FR-15**: Transfers MUST be validated: source ≠ target item; both items in the same Budget; source item holds sufficient positive active appropriation to cover the negative source row at create time (hard validation); and (per 013 decision) Transfer restricted to Draft only — post-submission fund movement uses Adjustment.

**Availability API (US4)**

- **FR-16**: System MUST expose computed availability (netAppropriated, encumbered, available) for a BudgetItem and for an Appropriation (returning its owning item's whole-item figures).
- **FR-17**: Both responses MUST include the resolved EffectiveAllowOverrun (item → budget → type chain).
- **FR-18**: Availability MUST be computed at request time from current data — no stored or cached aggregates.

**Budget head + monthly plans (US5)**

- **FR-19**: Budget create/update MUST accept and persist TotalAmount (entered plan), AllowOverrun (nullable), EffectiveFrom, EffectiveTo (nullable). TotalAmount MUST NOT default to 0.
- **FR-20**: Budget approve/activate MUST set Status only — no stored active flag.
- **FR-21**: System MUST provide GET/PUT `/api/BudgetItems/{id}/monthly-plan` (batch of 12) storing: BudgetItemId (required FK), Month (1–12), PlannedAmount (decimal(23,2), ≥ 0, entered), audit fields; UNIQUE (BudgetItemId, Month).
- **FR-22**: Monthly plans MUST have no status and MUST NOT be enforced or consumed by any workflow — reports input only.

**Migration + cleanup (US6)**

- **FR-23**: All schema changes MUST ship as ONE versioned migration: drop BudgetItems.OriginalAmount; drop Budgets.IsActive; keep BudgetItems.Remarks with a single aligned name; align column lengths (Funds.FundNumber/LegalAuthority/Description, Budgets.Description, Encumbrances.Description — configs win); add Appropriation.TargetBudgetItemId (nullable FK, Restrict, indexed); create BudgetItemMonthlyPlans; drop the stale Encumbrances→Suppliers FK (keep a plain index).
- **FR-24**: All commands reading/writing Budgets.IsActive MUST be corrected to use Status only.
- **FR-25**: Frontend encumbrance wrappers/hooks MUST match the published contract and consume the regenerated NSwag client (client regenerated after endpoints land; dead EncumbrancesClient replaced, not left stale). The sequence-form 'Liquidation' option MUST be removed.
- **FR-26**: Docs MUST be updated: feature map FM-004 REMOVED; registry BF-002/BF-003 notes updated; docs/database-schema.md created (budgeting tables first section); spec 013 marked superseded.

**Testing (all stories — Constitution XI)**

- **FR-27**: Every new/changed behavior MUST be test-first (observed failing before the code that passes it): encumbrance command/query/validator/gate tests; sequence wiring (generated, immutable, no duplicates under concurrency); transfer pair (atomic, net-zero, joint status transitions); availability endpoints; authorization tests; Appropriation edge-case gap tests.

### Key Entities *(include if feature involves data)*

- **Encumbrance**: fund reservation against an Appropriation; lifecycle Draft → ... → Closed/Cancelled/Reversed; reversal via new referencing row; computed IsReversed; number ENC-###### assigned at creation.
- **Appropriation**: budget-line authorization; Effect (Original/Supplement/Adjustment/Reduction/Transfer); new nullable TargetBudgetItemId for transfer target rows; number APR-###### assigned at creation.
- **Budget**: head plan with entered TotalAmount, AllowOverrun, effective window, Description; Status-only lifecycle; number BGT-###### assigned at creation.
- **BudgetItemMonthlyPlans**: informational per-month planned amounts on a BudgetItem; unique (item, month); no status, no enforcement.
- **DocumentSequence / DocumentSequenceService**: atomic per-prefix number source; existing service, wired into budgeting creates, race fixed, typed failures.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Creating the Nth document of any budgeting kind never fails on a duplicate number — 100% of concurrent create pairs (50+ trials) yield distinct, correctly formatted numbers.
- **SC-002**: 100% of encumbrance lifecycle transitions succeed only through their guards, and every transition leaves exactly one ApprovalHistory row.
- **SC-003**: Availability endpoints return figures matching the availability service for seeded scenarios with 100% agreement (including overrun-policy resolution).
- **SC-004**: A transfer creates exactly 2 rows, all-or-nothing, and item-level availability is unchanged by an active transfer pair (net zero verified in tests).
- **SC-005**: Budget head fields entered by users persist and display exactly as entered (TotalAmount is no longer always 0).
- **SC-006**: One migration applies cleanly to the initial-migration database; model, config, and DB match (verified by migration idempotency + schema diff with zero drift).
- **SC-007**: Zero user-supplied document numbers accepted anywhere in budgeting creation paths.
- **SC-008**: All tests green with TDD evidence (each behavior observed failing first); no weakened or skipped tests.

## Assumptions

- ApprovalHistory current shape (DocumentType string, Decision "from -> to") is kept as-is; the Action enum refactor belongs to the party/documents spec and is out of scope here.
- The stale Encumbrances→Suppliers FK is dropped now; the proper Party FK lands in the party spec.
- BudgetItems.Remarks is kept under one aligned name (entity + config + DTO consistent) rather than duplicated.
- EffectiveTo on Budget stays nullable (as-built decision).
- NSwag client is regenerated as part of this spec once encumbrance endpoints land; wrappers consume the generated client (no stale-file deferral).
- Transfers are Draft-only (per 013 clarification) and validated to same-Budget items.
- No frontend budgeting screens are delivered by this spec (separate frontend specs); this spec only fixes/aligns the stale encumbrance wrappers and sequence form option.
