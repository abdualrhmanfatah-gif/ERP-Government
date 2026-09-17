# Research: Asset Management Module Rebuild

**Feature**: 058-asset-module-spec | **Date**: 2026-09-15

Sources: verified codebase inspection (this session), [spec.md](./spec.md) decisions (C1, C3, C5, C7, C9, Q1–Q3, FR-094a, FR-111, FR-116), Constitution v1.4.0.

## R1: GL Posting Integration Path (Principle II)

**Decision**: Asset postings travel through the existing transactional outbox — use cases emit domain events (persisted atomically by `DispatchDomainEventsInterceptor` into `OutboxMessage`), and `OutboxProcessorService` publishes them via MediatR to idempotent Accounting-side consumers (`Application/Accounting/Integration/Assets/`) that create journal entries through the Accounting use cases.

**Rationale**: Verified infrastructure: interceptor persists events on SaveChanges (same transaction); processor has lease/heartbeat, bounded backoff retries (1s→10m, MaxRetries → terminal `Failed`), stalled-message recovery, processed-message cleanup, and manual re-drive capability. Existing asset domain events (`DepreciationPosted`, `AssetDisposed`, `AssetRevalued`, `AssetImpaired`, `AssetAcquired`) already sit in `Domain/Events/Assets/` with zero consumers — this feature wires them. Constitution II mandates this path for cross-module postings.

**Alternatives considered**: (a) Direct `JournalEntries.Add` inside assets use cases (current Payments/Revenue pattern) — rejected: registered exception #2 classifies this as a defect to remediate; violates II. (b) Synchronous same-transaction GL write without events — rejected: bypasses the event pipeline, same II violation with extra coupling.

## R2: Posting Flow Semantics (async consumer vs. synchronous posting)

**Decision**: Validate-then-emit. All FR-094a blocking gates (group account missing, template incomplete, substitution role undefined, entry unbalanced) and Constitution IV posting gates (period open/unlocked, account postable) are validated synchronously in the use case BEFORE the event is emitted — the user gets an immediate structured rejection. The document then transitions `Draft → Approved → Posting → Posted`; the consumer creates the entry, writes back `JournalEntryId`, and flips `IsPosted`. Consumer failure surfaces as a visible `PostingFailed` state with bounded retries and manual re-drive; the document never silently half-applies (FR-120: atomic event persist + idempotent consumer = one consistent result).

**Rationale**: Reconciles the spec's synchronous gate requirements with II's asynchronous mandate. FR-100 reversals build from the ORIGINAL entry's accounts (the consumer reads the original entry lines), never from current group/template config.

**Alternatives considered**: (a) Fully synchronous posting — rejected (II). (b) Emit-without-validation (validate only in the consumer) — rejected: users would learn of missing accounts asynchronously; FR-094a requires blocking at posting time.

## R3: Line-Level Source Tagging for Aggregated Depreciation (Q3: B, FR-094)

**Decision**: Add a nullable `DepreciationScheduleId` FK column to `JournalEntryLine`, mirroring the existing `PaymentOrderId` pattern (typed source reference on the line). The aggregated depreciation entry's per-asset lines each carry the schedule they serve; reports resolve accounts per record from tagged lines.

**Rationale**: `JournalEntryLine` already has a typed payment source reference — extending the same pattern is additive (IX-safe), queryable, and FK-enforced (Restrict). The spec forbids resolving accounts from an arbitrary first line.

**Alternatives considered**: (a) Generic `SourceType`/`SourceId` string pair — rejected: no referential integrity, unqueryable. (b) `JournalEntry.Ref` string — rejected: header-level, cannot attribute per-line shares.

## R4: Template Substitution Mechanism (FR-094a, C5)

**Decision**: Add a nullable `LineRole` (int enum) to `JournalEntryTemplateLine` with values including `Fixed` and `GroupDepreciationAccount` (the substitution slot). The depreciation template is located by a trusted configuration reference — a stable well-known template key stored in system configuration — never by `JournalId=6` or line order or account name. Seeding (idempotent) designates the debit line's role and fixes the accumulated-depreciation credit line. At posting, the consumer substitutes the group's `DepreciationAccountId` into the role-marked line and copies the template's fixed accounts into the entry lines; the actual accounts are then immutable in the entry.

**Rationale**: The current `JournalEntryTemplateLine` (TemplateId, Sequence, AccountId, Debit, Credit, CostCenterId…) has no role concept, and the seeded "قيد هلاك أصول" template (JournalId=6, Recurring, system) has no lines at all — FR-094a explicitly requires building the substitution mechanism as part of this feature. Role designation is explicit (never order/name), per the C5 decision. `IsSystemTemplate` + configuration key protect against environment drift of JournalId.

**Alternatives considered**: (a) Convention by line order — explicitly rejected by the C5 decision. (b) Account-name matching — rejected (names drift, Arabic/English variants). (c) A separate asset-specific posting-accounts table — rejected: duplicates the Accounting template mechanism and would re-introduce removed account fields by another shape.

## R5: Document Numbering (FR-044, FR-031, FR-110)

**Decision**: Use `IDocumentSequenceService` with per-document-type prefixes registered in the existing PrefixMap: `Asset` (AST, exists — register), `AssetTransfer` (TRF), `AssetDisposal` (DSP), `AssetRevaluation` (REV), `AssetImpairment` (IMP), `DepreciationSchedule` (DEP), `AssetPhysicalCount` (CNT). Server-side generation with the service's RowVersion concurrency; yearly reset policy per current `DocumentSequence` defaults. Unified `AssetTransactions` numbering is per-type (each type draws from its own sequence) — matching the current independent-sequence behavior for the legacy documents.

**Rationale**: The service is the established server-side numbering authority (Constitution binding constraint: "unique natural keys generated server-side via document sequences"); FR-044 requires preserving the current numbering policy, which is per-entity sequences.

**Alternatives considered**: One shared sequence for all transaction types — rejected: changes number semantics and capacity planning per type.

## R6: Permission Codes (FR-141)

**Decision**: Follow the existing `PermissionCodes` verb convention (`{Module}.{Verb}` with Approve/Post for financial documents): keep the existing `AssetDisposals.*`, `AssetRevaluations.*`, `AssetImpairments.*` groups (View/Create/Approve/Post); add `AssetTransfers.View/Create/Execute`, `AssetDepreciation.View/Run/Post/Reverse`, `AssetCounts.View/Create/Execute/Review`; remove the legacy `AssetMovements.*` group (its table is dropped per C1). Attribute definitions/bindings ride on `AssetGroups.*`; register stays on `Assets.View/Create/Update`.

**Rationale**: FR-141 requires exact match to the current naming convention at plan time; the current convention is noun-groups with Approve/Post verbs (not the spec's proposed `Assets.Transfers.Create` nesting). Transfers have no GL posting, so Execute (no Post); depreciation has no approval step, so Run/Post/Reverse; counts use Execute (start/complete) and Review.

**Alternatives considered**: The spec's proposed nested `Assets.Transfers.Create` naming — rejected: diverges from every existing code in `PermissionCodes.cs`; would also orphan the already-seeded disposal/revaluation/impairment codes.

## R7: C1 Migration Mechanics (FR-129–FR-133, DEP-030)

**Decision**: One versioned EF Core migration drops the 9 legacy assets tables (`AssetGroups`, `Assets`, `AssetMovements`, `AssetDisposals`, `AssetRevaluations`, `AssetImpairments`, `DepreciationSchedules`, `AssetPhysicalCounts`, `AssetPhysicalCountDetails`) and creates the 13 new tables. Verified: no external FK blockers exist — no table outside the Assets module references asset tables (only `Domain/Events/Assets` type references, which are replaced). Programmatic references removed in the same change: `IApplicationDbContext` DbSets, entity configurations, 054/055 Application slices, `Web/Endpoints/Assets`, seed code in `ApplicationDbContextInitialiser`, and frontend generated clients (regenerated via nswag). Legacy journal entries produced by asset operations remain untouched; their source references are textual/absent (no FK) and are listed in DEP-030 for the record. DEP-030 (next number, following DEP-022/DEP-026 precedents) records owner, rationale, scope, and remediation BEFORE implementation.

**Rationale**: Atomic single migration is reviewable as a unit and mirrors the DEP-022/DEP-026 removal precedents. SC-001's deletion report is produced by the migration's verification step (Quickstart).

**Alternatives considered**: (a) Staged drop-then-create migrations — rejected: an intermediate state with neither model complicates rollback and review. (b) Renaming legacy tables to archive names — rejected: C1 explicitly forbids archive/compatibility tables.

## R8: Depreciation Calculation Engine (FR-090–FR-093)

**Decision**: A pure `DepreciationCalculator` domain service (no dependencies) computes each record from stored inputs (method, base, rate, period data) and writes historical snapshots + after-values onto `DepreciationSchedule`. Six-decimal storage per the spec; posting amounts rounded by the current rounding policy at the consumer. Duplicate prevention keys on (asset, fiscal period, record nature) in the use case — not a unique constraint (FR-094), since reversals/corrections legitimately repeat the period.

**Rationale**: No legacy calculation code exists in this codebase (verified — the Assets Application layer only has groups/register CRUD), so the rules come from the spec's preserved-behavior requirements and the design document; a pure service is unit-testable per XI and keeps III's server-side rule in Domain.

**Alternatives considered**: Computing inside the MediatR handler — rejected: mixes calculation with orchestration and blocks Domain-level unit testing.

## R9: Flexible Attributes Storage (FR-020–FR-025)

**Decision**: The 3 design tables with typed value columns on `AssetAttributeValues` (`TextValue?`, `NumericValue? decimal(23,6)`, `DateValue?`, `BooleanValue?`); exactly one value column populated per the definition's `AttributeDataType`; validation enforces type + required-ness from the group binding. Definitions carry code (unique), name, data type, active flag; bindings carry (group, definition) composite identity + required flag + optional sort order.

**Rationale**: Typed columns keep values queryable and precision-guaranteed (VI explicit precision) without a value-type discriminator on reads; the composite binding identity is the design's stated key.

**Alternatives considered**: Single `string Value` column — rejected: no type safety, no numeric filtering, precision drift.

## R10: Frontend Test Runner Gap (Constitution XI)

**Decision**: Follow the repo's current state: no frontend test runner is configured (`package.json` has lint/build/generate-api only, consistent with 054/055 and the standing XI/AGENTS.md governance conflict noted in the Constitution's sync report). This feature ships frontend changes gated by ESLint + build + nswag contract regeneration; behavioral evidence (posting gates, reversals, audit, concurrency, balancing) is carried by the backend suites (unit/functional/integration/acceptance), which are TDD-mandatory.

**Rationale**: Precedent (054/055 plans record the same gap); introducing and populating a runner for 6 feature areas would expand this feature's scope materially. The Constitution's own header defers the XI frontend question, so no unregistered deviation is created by following current repo policy.

**Alternatives considered**: Configuring Vitest for the new screens — rejected as scope expansion; belongs to a dedicated tooling decision if raised.

## R11: Count Scope & Snapshot Implementation (FR-111/111a/111b, FR-112)

**Decision**: Scope resolution is one set-based query at detail generation: department membership derives from the custodian employee's `Employees.DepartmentId` (Q1); department-unrestricted scopes include custodian-less/department-less assets, department-restricted scopes exclude unprovable membership (SQL `EXISTS` on the employee join). Generated `System*` fields are written once and never refreshed (FR-112). The count header stores the resolved scope label («جميع المواقع — جميع الإدارات» et al.) for display on screen/document/report. Permission-bounded "all": the creator's data scope is applied to the generation query; if it cannot cover the declared scope, creation is blocked (no silent partial comprehensive count).

**Rationale**: Spec decisions FR-111a/b are directly implementable as one generation-time freeze; storing the resolved label preserves the certified scope even if reference data later changes.

**Alternatives considered**: Computing scope membership at read time — rejected: violates the freeze rule (FR-111b).
