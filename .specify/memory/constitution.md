<!--
Sync Impact Report
- Version change: 1.3.0 -> 1.3.1 (PATCH: registered feature-scoped TDD exception)
- Modified principles: none
- Added sections: Registered Exception #6 — Spec 045 TDD exception
- Removed sections: none
- Feature registry: no change
- Decision record: DEP-027
- Follow-up TODOs: capture Spec 045 manual quickstart evidence and run existing five-project regression gate before merge
- Previous version: 1.3.0 (last amended 2026-09-06)
-->

# ERP-Government Constitution

## Core Principles

### I. Layered Architectural Integrity (NON-NEGOTIABLE)

- The solution is layered Domain -> Application -> Infrastructure -> Web, plus orchestration
  (AppHost/ServiceDefaults) and shared-contract projects. Dependencies MUST point inward only:
  Web depends on Application and Infrastructure; Application depends on Domain and shared
  contracts only; Domain depends on no other project.
- Application MUST access persistence, identity, current user, and request context only through
  abstractions owned by Application or Domain. Application MUST NOT reference Infrastructure
  or Web.
- Web is a delivery adapter: endpoints MUST contain no business rules and MUST delegate every
  operation to a use case. Business logic MUST NOT live in endpoints, background jobs, or
  frontend code.

*Rationale: the repository's verified reference graph and application-owned interfaces
(IApplicationDbContext, IUser, IIdentityService, IRequestContext) establish this direction;
it is architecture, not preference.*

### II. Bounded Contexts and Event-Carried Integration

- Each business module owns its entities, use cases, and DTOs. Module internals MUST NOT be
  shared across modules.
- Cross-module reads MAY use the shared persistence abstraction owned by Application.
- Cross-module side effects — especially postings into the general ledger — MUST travel through
  domain events persisted atomically with the originating write (transactional outbox) and be
  processed asynchronously with bounded retry and durable pending-event records. A module MUST
  NOT write another module's tables directly.
- Event consumers MUST be idempotent and re-driveable; manual re-drive of failed events MUST
  be possible without data repair.

*Rationale: Budgeting integrates with Accounting exclusively via the event pipeline with zero
direct references; the outbox and accounting-event retry machinery are established.*

### III. Server-Side Business-Rule Integrity

- Every business rule MUST be enforced server-side, in use cases or domain services. Frontend
  validation, database defaults, and documentation are not enforcement.
- Expected business-rule failures MUST return explicit result failures; exceptions are reserved
  for infrastructure-level conditions mapped to the error contract.
- Document state transitions MUST be guarded: a document may only advance along its declared
  lifecycle (for example, only an approved entry may post; only a submitted budget may be
  approved).
- Time-windowed rules (effective roles, delegations, fiscal periods, exchange-rate validity)
  MUST be evaluated at transaction time from current data, not from cached assumptions.

### IV. Financial Integrity (NON-NEGOTIABLE)

- Every journal entry MUST balance: total debits equal total credits in the base currency at
  the moment of posting.
- Exactly one base currency is active at any time. Foreign-currency lines MUST carry their
  exchange rate and converted base amounts; all control totals MUST be computed in base
  currency.
- Posted entries and their lines are the financial system of record. Derived balances are
  materializations that MUST be rebuildable from posted data, and a rebuild MUST reconcile
  against the ledger.
- Posting MUST be rejected unless all gates pass: entry approved, fiscal year open, fiscal
  period open and unlocked, account postable, entry number unique.
- Corrections to posted records MUST be reversing entries carrying a reason. Posted entries
  MUST NOT be mutated or deleted.
- System-generated entries MUST be flagged as such and traceable to the event that produced
  them.

### V. Budget Control Before Expenditure

- Spending documents MUST follow the chain Budget -> Appropriation -> Encumbrance ->
  Payment; budget availability MUST be checked before a payment is approved.
- A failed budget check MUST block approval. Overrides MUST be explicit, permissioned, and
  recorded.
- The budget's configured control level (none/warning/blocking) and overrun policy MUST be
  honored; overruns MUST follow the configured approval path.
- Commitments MUST reserve budget before payment execution consumes it.
- Multi-dimensional availability checks (fund, program, project, budget item) MUST be
  supported for budget control. The full chain appropriations -> encumbrances -> payments
  applies across all dimensions.
- Fiscal year closing (lapse) MUST block new payments against lapsed items. Reopening MUST
  restore the pre-lapse state unless payments exist against lapsed items.

### VI. Data Integrity

- Schema changes MUST ship as versioned, reviewable migrations. Runtime auto-creation or
  auto-deletion of schema is prohibited outside throwaway test hosts.
- All foreign-key referential actions MUST be Restrict. Cascading deletes are prohibited.
- Every mutable business record MUST carry an optimistic-concurrency token, and every update
  MUST verify it.
- Financial and audit records MUST NOT be hard-deleted. Corrections happen by reversal;
  deactivation happens by status or active flag.
- Monetary values MUST be stored with explicit precision appropriate to their class
  (transaction amounts, quantities, exchange rates); database defaults for precision are
  prohibited.
- Reference and master-data seeding MUST be idempotent (guarded against re-insertion).

### VII. Authorization and Separation of Duties (NON-NEGOTIABLE)

- Every business endpoint AND its backing use case MUST declare a required named permission.
  Anonymous access to business functionality is prohibited. Operational endpoints MUST at
  minimum require authentication.
- Authorization MUST fail closed when the permission subsystem errors.
- Every authorization decision — grant and denial, with reason — MUST be recorded in the
  security audit log.
- Sensitive document transitions MUST pass server-side approval-rule evaluation (document
  type, amount threshold, fund scope, sequence) and MUST record approval history with an
  evaluation snapshot. Delegation MUST be time-bounded; re-delegation MUST be explicit.
- Separation-of-duties conflicts MUST be evaluated against the configured conflict matrix;
  violations MUST block, warn, or require approval exactly as configured.
- Frontend permission checks are user experience only. The server is the sole authority.

### VIII. Approval Workflows and Audit Immutability

- Approval and workflow decisions MUST be persisted as history records at the moment of
  decision: actor, decision, time, reason, and rule evaluation snapshot.
- Audit trails (entity-change and security) are INSERT-ONLY. Update and delete attempts
  MUST be rejected by an enforced constraint, not by convention.
- Audit records MUST capture actor, timestamp, action, and per-field old/new diffs, plus
  request context where available.
- Workflow steps MAY delegate approver resolution to the approval-rule engine; the outcome
  MUST still be recorded as decision history.

### IX. API and Frontend Contract Integrity

- The backend-generated OpenAPI document is the single source of truth for the HTTP contract.
  Frontend code MUST conform to it — through generated typed clients or contract-conformant
  modules — and MUST NOT assume undocumented endpoint shapes.
- Error responses MUST use the uniform problem-details contract with fixed status semantics:
  400 validation, 401 unauthenticated, 403 forbidden, 404 not found. Concurrency conflicts
  MUST be surfaced distinctly.
- Every endpoint MUST expose a stable, named operation identity usable for client generation.
- Removing, renaming, or reshaping an endpoint or payload field is a breaking change and
  requires a decision record before implementation.
- Every mutating operation MUST round-trip the optimistic-concurrency token from frontend to
  backend.

### X. UI and Design System Consistency

- All visual design values (color, spacing, typography, radii, status colors) MUST originate
  from the central design-token source and flow only through generated token artifacts.
  Hard-coded design values in components or styles are prohibited.
- The UI is Arabic-first and right-to-left. Layout MUST use logical (start/end) properties,
  not physical left/right. Every screen MUST render correctly under RTL and dark mode.
- Repeated interaction patterns MUST use the shared UI component library; per-feature
  duplicates of shared primitives are prohibited.
- Monetary values MUST render through the shared money-display and formatting conventions
  (tabular numerals, locale-consistent formatting, explicit negative representation).
  Ad-hoc number formatting for money is prohibited.
- Navigation entries MUST carry their required permission identifier, matching backend
  policy naming.

### XI. Testing, Verification, and Evidence

- Business rules and invariants MUST be proven by executable tests. A test that cannot fail
  (always-pass placeholders, scenario comments without assertions) is not evidence and MUST
  be marked as tracked debt.
- Every use case MUST have unit tests covering its success path and principal failure paths
  before it is considered done.
- Financial invariants — balancing, posting gates, reversal semantics, audit immutability,
  concurrency conflicts, approval evaluation — MUST be covered by functional tests executing
  against a real database with per-test state reset.
- Critical user journeys MUST be covered by browser-level acceptance tests.
- Frontend features MUST ship with tests in the configured frontend test runner; untested
  frontend code is incomplete.
- **Test-Driven Development (NON-NEGOTIABLE for new features)**: Every behavior change is
  driven by a test that failed first. A test exists and has been observed failing, for the
  right reason, before the code that makes it pass. Test tasks in tasks.md are not optional.
  Tests are never weakened, skipped, deleted, or filtered out to reach green. Refactoring
  happens only on a green suite.

### XII. Controlled Architectural Change

- Changes to layers, module boundaries, integration patterns, contract shape, the security
  model, or these principles REQUIRE a numbered decision record — rationale, scope,
  migration, remediation — accepted before implementation.
- Deviations from this Constitution are permitted ONLY as registered exceptions in the
  decision log, each with an owner and a remediation path. An unregistered deviation is a
  defect.
- Principles take precedence over convenience in specs, plans, and tasks. Any violation
  MUST be justified in the plan's violation tracking or escalated to a decision record.
- When implementation and this Constitution conflict, the conflict MUST be resolved by fixing
  the code or by amending the Constitution — never by silent divergence.

## Binding Architectural Constraints

Established by the repository as mandatory; testable by inspection.

- **Layer graph**: Domain -> (no project references). Application -> Domain + Shared.
  Infrastructure -> Application. Web -> Application + Infrastructure. AppHost -> Web +
  Infrastructure. Test hosts are standalone. The frontend is a separate application consuming
  only the published HTTP contract.
- **Module registry**: Accounting, Budgeting, Payments, Revenue, Banking, Procurement,
  Inventory, Assets, Suppliers, Organization, FinancialSettings, Committees, Security,
  Workflow, BackgroundJobs. Adding, removing, splitting, or merging a module requires a
  decision record.
- **Integration patterns**: shared persistence abstraction for cross-module reads;
  transactional outbox plus durable accounting-event records for cross-module financial
  effects; bounded retries with terminal failure states and manual re-drive.
- **Persistence**: migrations only; Restrict on every foreign key; explicit decimal precision;
  unique natural keys (codes, entry numbers) generated server-side via document sequences.
- **Background processing**: database-persisted job state machines with idempotency keys,
  per-attempt execution logs, and orphan recovery on restart. In-memory-only scheduling of
  financial jobs is prohibited.
- **Observability**: request logging and long-running-request detection on every use case;
  health endpoints; telemetry wired for all service defaults.
- **Localization**: Arabic-first RTL user interface; bilingual data fields where the domain
  provides them; Arabic locale formatting conventions for dates, numbers, and currency.
- **Build discipline**: warnings treated as errors; nullable reference types enabled; central
  package management with transitive pinning; SDK version pinned at repo root.
- **Hand-written frontend clients**: permitted only when they mirror the published OpenAPI
  contract exactly; divergence from the contract is a defect.

### Registered Exceptions (as of v1.0.0)

Each entry is a known deviation under Principle XII, pending remediation. Derived from the
2026-09-02 codebase discovery.

1. **Open endpoint authorization policies** — endpoint-layer policies are open placeholders
   pending real RBAC wiring (decision trace: DEP-020). Use-case-layer authorization is the
   effective control until remediated.
2. **Direct general-ledger write in payment completion** — payment completion creates ledger
   entries directly, bypassing the event/posting pipeline, with placeholder foreign keys.
   Remediation: route through the posting pipeline.
3. **Missing use-case authorization in Workflow, approval-admin, and pending-approvals use
   cases** — these use cases lack declared permissions. Remediation: add permissions.
4. **Frontend permission and user-profile stubs** — always-grant placeholders in the frontend
   permission layer. Remediation: bind to server permission endpoints.
5. **Stubbed functional scenario tests** — balance, concurrency, and approval-evaluation
   scenarios are always-pass placeholders. Remediation: replace with real assertions under
   Principle XI.
6. **Spec 045 TDD exception** — work scoped to `specs/045-payments-group/` does not require
   red-green-refactor cycles or new automated tests (decision trace: DEP-027). Verification uses
   scoped builds, frontend lint/build, manual quickstart evidence, and the existing five backend
   test projects as convergence/pre-merge regression gates. Existing tests may not be weakened,
   skipped, or deleted. This exception does not apply outside Spec 045.

## Compliance in the Spec-Driven Workflow

This Constitution governs every phase of the Spec-Driven Development workflow
(specify -> plan -> tasks -> implement).

- **Specify**: Specifications MUST express invariants as independently testable acceptance
  scenarios and requirements using normative language (MUST). Any requirement that touches a
  principle MUST identify which principle applies.
- **Plan**: The Constitution Check gate MUST run before research begins and be re-checked
  after design. Violations MUST be logged in the plan's violation/complexity tracking with a
  justification, or escalated to a decision record. Plans MUST reference the registered
  exceptions they depend on or remediate.
- **Tasks**: Tasks MUST be grouped by user story with independent verification. Tasks for
  financial features MUST include verification of balancing, concurrency, audit, and
  approval behavior. Replacement of any stub test MUST appear as an explicit task.
- **Implement**: Code MUST satisfy the principles its plan cites. Review MUST verify layer
  dependencies, declared permissions on new endpoints and use cases, contract regeneration,
  token usage, RTL/dark-mode correctness, and test evidence. Newly created stub tests MUST
  be marked as debt in the same change.
- **Checklists and review**: Every review gate in the workflow MUST include a Constitution
  compliance check; NON-NEGOTIABLE principles cannot be waived by a spec, plan, or task.

## Governance

- **Authority**: This Constitution is the supreme engineering authority of ERP-Government.
  It supersedes the README, templates, feature specs, plans, task lists, code comments, and
  prior conventions whenever they conflict.
- **Precedence**: Constitution > registered decision records > feature specification >
  implementation plan > task list > implementation. Code that conflicts with this
  Constitution is either a defect (fix the code) or an unregistered exception (register it
  or amend the Constitution).
- **Amendment process**: A proposal MUST state rationale, affected principles, impact on
  existing code and registered exceptions, and a migration/remediation plan. Amendments take
  effect when the version line and the Sync Impact Report are updated in this file.
- **Versioning**: MAJOR.MINOR.PATCH. MAJOR for removals or redefinitions that invalidate
  existing compliance; MINOR for new or materially expanded principles or sections; PATCH
  for clarifications and wording that do not change obligations.
- **Compliance review**: The Constitution MUST be re-validated against the codebase at every
  amendment. Registered exceptions are reviewed at every amendment; an exception without a
  live remediation path MUST be re-registered or closed by remediation.
- **Exception handling**: All architectural exceptions MUST be numbered decision records
  with owner, rationale, scope, and remediation path, kept in the repository's decision log.
  Unregistered deviation from any principle is treated as a defect in review.

**Version**: 1.3.1 | **Ratified**: 2026-09-02 | **Last Amended**: 2026-09-09
