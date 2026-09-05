# Research — Budgeting Backend Completion (015)

Findings verified against as-built code 2026-09-05. Each entry: Decision / Rationale / Alternatives.

## D1 — BudgetItems route surface for availability + monthly-plan

**Decision**: Create a NEW endpoint group `/api/BudgetItems` (src/Web/Endpoints/Budgeting/BudgetItems.cs) hosting ONLY `GET /{id}/availability` and `GET|PUT /{id}/monthly-plan`. BudgetItem CRUD stays where it is (nested under `/api/Budgets/{budgetId}/items/...`, Budgets.cs:68-94).

**Rationale**: The spec mandates `/api/BudgetItems/{id}/availability` and `/api/BudgetItems/{id}/monthly-plan`. As-built, item CRUD is nested because item operations need the owning BudgetId. Availability and monthly-plan only need the item id, so a flat sub-resource group is the minimal additive change. Additive contract only (Constitution XII/IX).

**Alternatives**: (a) nest availability/monthly-plan under `/api/Budgets/{budgetId}/items/{itemId}/...` — rejected: contradicts spec-specified routes and forces callers to know the budget id for a flat lookup; (b) move all item CRUD to /api/BudgetItems — rejected: breaking reshape of live endpoints.

## D2 — Sequence allocation: atomic single-statement with OUTPUT

**Decision**: Replace the increment-then-reread in `DocumentSequenceService.GenerateNextNumberAsync` (DocumentSequenceService.cs:41-59) with ONE round-trip: raw SQL `UPDATE DocumentSequences SET CurrentNumber = CurrentNumber + 1 OUTPUT inserted.CurrentNumber, inserted.Prefix WHERE DocumentType = @type AND IsActive = 1` executed via `Database.SqlQuery<T>`/`ExecuteSqlRaw` against the already-open connection. Remove all `"Error: ..."` string returns; throw typed exceptions (`DocumentSequenceNotFoundException`, `DocumentSequenceInactiveException`) caught by command handlers and translated to `Result<T>.Failure`. Format unchanged: `{prefix}-{D6}`.

**Rationale**: As-built race: two concurrent callers each `ExecuteUpdateAsync (+1)` then both re-read the final value → same number returned twice → unique-index collision (the reported APR/ENC bug's second half). EF `ExecuteUpdate` cannot return values, so a single UPDATE...OUTPUT statement is the smallest atomic primitive on SQL Server; no serializable-transaction escalation needed.

**Alternatives**: (a) serializable transaction around increment+read — rejected: broader lock scope, higher deadlock risk under the same create bursts; (b) app-level lock/semaphore — rejected: wrong for multi-instance deployments; (c) keep string errors — rejected by spec (typed failures).

## D3 — Wire numbers into budgeting creates

**Decision**: `CreateBudgetCommand` calls `GenerateNextNumberAsync("Budget")` → `BGT-######` and STOPS accepting user-supplied BudgetNumber (remove from command + validator). `CreateAppropriationCommand` calls it with "Appropriation" (APR). `CreateEncumbranceCommand` with "Encumbrance" (ENC). Numbers immutable — no update path touches them. Handlers catch the typed sequence exceptions and return Result failures.

**Rationale**: As-built, no budgeting command calls the service (CreateAppropriationCommand.cs:40-49 creates rows with no number; CreateBudgetCommand:47 takes client number) — hence the unique-index collision on the second APR/ENC row. Prefix map already contains Budget/Appropriation/Encumbrance entries (DocumentSequenceService.cs:13).

**Alternatives**: client-supplied numbers with server uniqueness check — rejected: spec requires system-generated immutable numbers.

## D4 — Transfer pair modeling on Appropriation.TargetBudgetItemId

**Decision**: A Transfer = TWO `Appropriation` rows, one command, one SaveChanges: source row — `BudgetItemId = source`, `Effect = Transfer`, `Amount` stored so its availability effect is negative; target row — `BudgetItemId = target`, `Effect = Transfer`, positive amount, `TargetBudgetItemId = source item id`. Source row mirrors `TargetBudgetItemId = target item id`. Pair identification rule (used by lifecycle commands and tests): the partner of row X is the other row with `Effect = Transfer`, same `BudgetId`, `BudgetItemId == X.TargetBudgetItemId`, `TargetBudgetItemId == X.BudgetItemId`, and `Amount == -X.Amount`. Lifecycle commands (submit/approve/activate/suspend/close/cancel) detect `Effect == Transfer` and transition BOTH rows in the same unit of work — no separate transfer lifecycle endpoints. `CreateAppropriationCommand` keeps rejecting Effect=Transfer (draft-transfer restriction per 013: post-submission movement uses Adjustment; transfers only created by the pair command).

**Rationale**: Spec fixes the shape: one nullable FK `TargetBudgetItemId` (int?, FK → BudgetItems, Restrict, indexed). With only that column available, mutual-reference matching is the deterministic pairing rule; creating both rows atomically in one command guarantees pair integrity at birth, so the matching rule only needs to hold for rows created by this command. Item-level availability unchanged: service already nets Transfer to 0 (BudgetAvailabilityService.cs:28-33).

**Alternatives**: (a) add a TransferGroupId column — rejected: spec limits schema change to TargetBudgetItemId; (b) two independent creates — rejected: not atomic, breaks joint lifecycle; (c) reuse UpdateAppropriationCommand for transfers — rejected: as-built explicitly rejects Transfer there.

## D5 — Transfer source-coverage validation (clarified Q2)

**Decision**: `CreateTransferAppropriationCommand` hard-validates at create: same Budget for both items, source ≠ target, and source item's net active appropriation (service sum) ≥ transfer amount. Failure → Result failure listing the violated rule. No availability gate at activation beyond the existing appropriation gate (transfer rows net 0 at item level).

**Rationale**: User decision (clarification 2026-09-05): otherwise the negative source row could push a source item's computed availability negative, which nothing else would catch.

**Alternatives**: availability-gate at activation like other appropriations — rejected: Transfer nets 0 in the service, so the gate could never see the source-side reduction.

## D6 — Encumbrance lifecycle FSM and statuses

**Decision**: Use the existing `EncumbranceStatus` enum (Draft, PendingApproval, Approved, Active, PartiallyReleased, PartiallyLiquidated, FullyLiquidated, Closed, Cancelled, Reversed) and ADD `Suspended = 10` (append-only; enums stored as int, no DB change). Command→transition map:

- submit: Draft → PendingApproval
- approve: PendingApproval → Approved (ApprovalHistory)
- activate: Approved → Active
- suspend: Active → Suspended (suspended encumbrances are NOT counted open by the availability service — balance freed computationally)
- close: Active|Suspended → Closed
- cancel: Draft|PendingApproval|Approved|Active|Suspended → Cancelled
- reverse: Active|Suspended → Reversed (creates negative row ReversalOfId = original; per clarification Q1)

Update/Delete: Draft only. Availability service "open" set unchanged: {Active, PartiallyReleased, PartiallyLiquidated}, non-reversal. PartiallyReleased/PartiallyLiquidated/FullyLiquidated producers are payment-side (later spec) — no commands for them here.

**Rationale**: Matches Appropriations PATCH-lifecycle convention and spec FR-5/FR-7; enum addition is additive (int storage, no migration).

**Alternatives**: separate command per derived state (release/liquidate) — deferred to payments spec (liquidations removed by spec 008/009).

## D7 — Encumbrance availability gate at create

**Decision**: `CreateEncumbranceCommand` evaluates `BudgetAvailabilityService.EvaluateControlMethod(controlMethod, requested, available)`. `controlMethod` comes from the owning Budget's BudgetType (`BudgetType.ControlMethod`, BudgetType.cs:10). `available` = whole-item availability (net active appropriations − open encumbrances) for the item behind `AppropriationId`. None → create; Warning → create + ApprovalHistory row with Decision "Created (Warning override)" style reason carrying the warning; Blocking → Result failure. Warning overrides also write a standard security-audit entry via the existing audit pipeline where already wired for authorization decisions (Constitution V/VII).

**Rationale**: Reuses the exact gate consumed by `ActivateAppropriationCommand` today; ApprovalHistory is the designated decision-record store (Constitution VIII); no Action-enum refactor in this spec.

**Alternatives**: new availability audit table — rejected: ApprovalHistory is the established record; principle VII security audit already covers authorization decisions.

## D8 — Availability API shape

**Decision**: Extend `BudgetAvailabilityService` with a summary method returning components (netAppropriated, encumbered, available, effectiveAllowOverrun) so both endpoints and the encumbrance gate share one computation. `GET /api/BudgetItems/{id}/availability` returns whole-item figures + resolved EffectiveAllowOverrun (`EvaluateAllowOverrun` item → budget → type). `GET /api/Appropriations/{id}/availability` resolves the owning item and returns the SAME whole-item figures (control point documented = BudgetItem). 404 on unknown ids.

**Rationale**: Spec FR-16/FR-17; single computation prevents endpoint/service drift; effective-overrun chain already exists (BudgetAvailabilityService.cs:62-72).

**Alternatives**: per-appropriation availability prorated to the row — rejected: control point is the item; multiple appropriations net together.

## D9 — Migration content (one migration)

**Decision**: Single migration on top of `20260904031800_InitialCreate`:
1. DROP `BudgetItems.OriginalAmount` (entity field removed — zero-stored-aggregates).
2. ADD `BudgetItems.Remarks` (entity keeps name `Remarks`; nvarchar length per config — DB is missing the column today).
3. DROP `Budgets.IsActive`; remove property from entity + all writers (ActivateBudgetCommand.cs:32 sets true today; ApproveBudget does not touch it).
4. Column lengths aligned entity↔config (configs win): Funds.FundNumber/LegalAuthority/Description, Budgets.Description, Encumbrances.Description.
5. ADD `Appropriations.TargetBudgetItemId` int? + FK → BudgetItems (Restrict) + index.
6. CREATE `BudgetItemMonthlyPlans` (Id, BudgetItemId FK Restrict req, Month int, PlannedAmount decimal(23,2), audit + RowVersion, UNIQUE (BudgetItemId, Month)).
7. DROP FK `FK_Encumbrances_Suppliers_VendorId` (present in snapshot/migration, absent from EncumbranceConfiguration) — keep the VendorId index.

**Rationale**: Verified drift: InitialCreate.cs:3321 has the Suppliers FK; snapshot (ApplicationDbContextModelSnapshot.cs:8090-8093) matches the DB, so the config is the outlier — dropping the FK realigns config↔DB. One migration per spec FR-23.

**Alternatives**: separate migrations per concern — rejected: spec mandates exactly one; (b) keep OriginalAmount as computed column — rejected: violates zero-stored-aggregates.

## D10 — Permission codes for encumbrances

**Decision**: Add `PermissionCodes.EncumbrancesUpdate` and `PermissionCodes.EncumbrancesDelete` (missing today — block at PermissionCodes.cs:128-136 has View/Create/Submit/Approve/Activate/Release/Close/Cancel/Reverse). Map: PUT → Update, DELETE → Delete, all others use existing codes; policies registered in Web DependencyInjection.cs next to the existing stubs (open placeholder per registered exception #1 — unchanged behavior).

**Rationale**: PUT/DELETE endpoints need their own named permissions (Constitution VII: every endpoint AND use case declares one).

**Alternatives**: reuse EncumbrancesView for mutations — rejected: violates per-operation permission separation used across the codebase.

## D11 — Frontend cleanup path

**Decision**: Regenerate the NSwag client (`npm run generate-api` = `nswag run /runtime:Net100`, already wired as `prestart`/`prebuild` — package.json:56-63) after endpoints land; rewrite `features/budgeting/shared/client.ts` encumbrances wrappers + `useEncumbrances.ts` hooks to call the generated `EncumbrancesClient` (drop `liquidatePartial`/`liquidateFull` and the hand-written duplicate routes); remove the `Liquidation` ('التصفية') option from DocumentSequenceForm.tsx (no such prefix in the service).

**Rationale**: Clarification Q3 = regenerate; generation is part of the normal build so no new pipeline step; hand-written wrappers may exist only if they mirror the contract exactly (Constitution IX) — consuming the generated client removes the mirror-maintenance burden.

**Alternatives**: delete stale client and keep handwritten wrappers — rejected by user decision; leave file unused — rejected: build regenerates it anyway, stale drift would recur.

## D12 — Docs

**Decision**: `docs/database-schema.md` EXISTS (accounting notes) — EXTEND it with a "Budgeting tables" first section instead of creating from scratch (spec assumption it didn't exist was wrong; plan corrects). Update feature map FM-004 (Budget Liquidations → REMOVED), registry BF-002/BF-003 notes, and mark spec 013's status line "Superseded by 015".

**Rationale**: Verified: docs/database-schema.md present with accounting sections; appending preserves its purpose ("later specs append").

**Alternatives**: separate budgeting-schema doc — rejected: spec wants one accumulating schema doc.

## D13 — ApprovalHistory shape frozen

**Decision**: Write ApprovalHistory exactly as today: `DocumentType = "Encumbrance"`, `Decision = "{from} -> {to}"`, `ApproverUserId = userId ?? 0`, `RequiredRole = ""`, optional `Reason` (carries the Warning-override message). No Action enum.

**Rationale**: As-built convention (ApproveBudgetCommand.cs:47-55); Action refactor belongs to the party/documents spec.

## D14 — Test strategy mapping (Constitution XI)

**Decision**:
- Unit (NUnit, mocked context): encumbrance validators + FSM guards, gate decisions (None/Warning/Blocking), transfer validation, budget-head mapping.
- Functional (real DB, Respawn reset): encumbrance full lifecycle + reversal restores availability; sequence concurrency (parallel creates → distinct numbers, atomic allocation); transfer pair atomicity/joint transitions/net-zero; availability endpoints figures incl. overrun resolution; authorization (401/403); appropriation edge-case gaps.
- Every task test-first: failing test observed before implementation; no weakened/skipped tests.

**Rationale**: Mirrors existing suites (Application.UnitTests/Budgeting, FunctionalTests/Budgeting + TestBase/TestApp ResetState).

**Alternatives**: browser-level acceptance tests — out of scope (no new screens; existing journeys unchanged).
