# Implementation Plan: Asset Management Module (وحدة إدارة الأصول — إعادة البناء)

**Branch**: `058-asset-module-spec` | **Date**: 2026-09-15 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/058-asset-module-spec/spec.md`

## Summary

Rebuild the Assets module per the 13-table design: drop the 9 legacy assets tables and all their data (C1 decision — registered exception to Constitution VI via DEP-030), recreate the model empty, and deliver unified transactions (transfer/disposal/revaluation/impairment), depreciation with reversal, physical counts with tri-state results, and flexible attributes. GL postings travel through the verified transactional-outbox pipeline (`DispatchDomainEventsInterceptor` + `OutboxProcessorService`) with idempotent Accounting-side consumers; the depreciation entry resolves its legs via a role-designated substitution rule on the existing `JournalEntryTemplate` mechanism (FR-094a). Backend follows MediatR CQRS vertical slices; frontend re-points the 054/055 screens and adds new feature folders with React Query, Zod, and nswag-generated clients.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`, SDK 10.0.201, warnings-as-errors, nullable enabled), TypeScript 5.x / React 18

**Primary Dependencies**: MediatR, FluentValidation, AutoMapper, Entity Framework Core 10, React Query, Zod, react-hook-form, nswag generated API clients, shared design tokens

**Storage**: SQL Server via EF Core — versioned migrations only; 9 legacy assets tables dropped + 13 new tables created in one reviewable migration (see DEP-030)

**Testing**: xUnit + FluentAssertions — `Application.UnitTests`, `Domain.UnitTests`, `Application.FunctionalTests` (real DB via `TestAppHost`), `Infrastructure.IntegrationTests`, `Web.AcceptanceTests`. Frontend: no test runner configured in the repo (R10) — lint + build gates apply

**Target Platform**: Web application (SPA + REST API), Arabic-first RTL

**Project Type**: Layered web application (Domain → Application → Infrastructure → Web, plus AppHost/ServiceDefaults and ClientApp SPA)

**Performance Goals**: Register/list loads < 2s for 10k assets; a full depreciation run for 10k eligible assets completes without timeout; count line generation for the entity-wide scope is one set-based operation; traceability resolution (card → transaction → entry → line) within one interaction (SC-006)

**Constraints**: Constitution II — GL postings only via transactional outbox + idempotent consumers; Restrict on every FK; RowVersion optimistic concurrency on every mutable record; explicit decimal precision; fail-closed authorization (VII); insert-only audit (VIII); Arabic-first RTL + design tokens (X); unified error contract (XIII)

**Scale/Scope**: Up to 10,000 assets, ~50 concurrent users; 13 module tables; ~35 endpoints; 6 frontend feature areas

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture (NON-NEGOTIABLE) | ✅ PASS | Entities in Domain, use cases in Application vertical slices, endpoints as thin adapters in Web. No Application→Infrastructure/Web references. |
| II. Bounded Contexts & Event-Carried Integration | ✅ PASS | Assets module self-contained; cross-module GL postings travel as domain events persisted atomically via the existing `DispatchDomainEventsInterceptor` → `OutboxMessage` pipeline and are processed by `OutboxProcessorService` (bounded retries, lease/heartbeat, stalled recovery, manual re-drive). Consumers (Accounting side) are idempotent — verified infrastructure, no new integration pattern. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All document state transitions, posting gates, scope rules, and uniqueness checks enforced server-side in handlers/domain services; expected failures return structured `Result` failures. |
| IV. Financial Integrity (NON-NEGOTIABLE) | ✅ PASS | Entries balance at posting; posting gates honored (period open/unlocked, account postable, entry number unique via document sequences); corrections only by reversal; system entries flagged `IsSystemGenerated` + `MoveEntryType.SystemGenerated` and traceable to the originating event. |
| V. Budget Control Before Expenditure | N/A | Asset operations post depreciation/disposal/revaluation/impairment effects to the GL but do not approve or consume budget appropriations. No budget-availability gate applies. |
| VI. Data Integrity | ⚠️ REGISTERED VIOLATION | The C1 decision drops legacy financial/audit records (9 tables) — a deliberate, user-approved exception to "financial and audit records MUST NOT be hard-deleted". Registered as **DEP-030** (owner, rationale, scope, remediation) per XII before implementation; see Complexity Tracking. All other VI obligations pass: migrations only, Restrict FKs, RowVersion, explicit decimal precision, idempotent seeding. |
| VII. Authorization (NON-NEGOTIABLE) | ✅ PASS | Every endpoint + use case declares a named permission; fail-closed on permission subsystem errors; decisions logged to SecurityAuditLog. New codes follow the existing `PermissionCodes` convention (R6). |
| VIII. Approval Workflows & Audit Immutability | ✅ PASS | FR-116 re-examination audit uses the existing `AuditTrail` (`IImmutableEntity`, per-field `OldValues`/`NewValues`, actor, timestamp); insert-only enforced by constraint. |
| IX. API & Frontend Contract Integrity | ✅ PASS | OpenAPI is the contract source; frontend uses nswag-generated clients. Only additive nullable fields on existing payloads (`JournalEntryTemplateLine.LineRole`, `JournalEntryLine.DepreciationScheduleId`) — no removals/reshapes; error schemas follow the problem-details contract. |
| X. UI & Design System Consistency | ✅ PASS | Arabic-first RTL with logical properties, shared tokens and component library, money display via shared conventions, nav entries carry permission identifiers. |
| XI. Testing & Verification | ✅ PASS w/ note | TDD NON-NEGOTIABLE: every behavior change test-first; financial invariants covered by functional tests against a real DB with per-test reset. Frontend runner gap is pre-existing repo state (R10) — backend suites carry the evidence burden. |
| XII. Controlled Architectural Change | ✅ PASS w/ note | No layer/module boundary changes. Two registered items: **DEP-030** (C1 table drop — exception to VI) and the additive `LineRole`/template-reference change inside Accounting (documented in R4; additive, not contract-breaking). |
| XIII. Error Handling, Diagnostics & Recovery | ✅ PASS | `Result<T>` with stable codes/categories; problem-details at Web; outbox failures surface as visible Failed states with bounded retry and manual re-drive; expected rejection ≠ server fault. |

**Gate Result**: PASS with one registered violation (VI via C1 → DEP-030) tracked below. No unjustified violations.

**Failure-path obligations (XIII, docs/error-handling.md)**: affected paths — posting gates (FR-094a blocking → 409/400 expected rejection with stable codes), outbox consumer failures (terminal Failed state, re-drive without repeating financial effects — idempotency by natural source key), concurrency conflicts (RowVersion → 409), permission denial (403, not session expiry). Recovery ownership: use cases own validation failures; the outbox owns retry/re-drive diagnostics; the frontend normalizes via the shared error representation. Success compatibility: existing 054/055 endpoints are reshaped by the model change and handled as part of DEP-030's migration scope (breaking change recorded there).

## Project Structure

### Documentation (this feature)

```text
specs/058-asset-module-spec/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/api.md     # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
docs/decision-records/
└── DEP-030-drop-legacy-assets-tables.md    # NEW — Constitution XII record for the C1 reset

src/Domain/Assets/
├── Entities/                               # RECREATED — 13 entities (9 legacy files removed, 13 new)
│   ├── AssetGroup.cs                       # reduced accounts (DepreciationAccountId only, C5)
│   ├── Asset.cs                            # EmployeeId custodian, CurrencyId (C4), typed fields
│   ├── AssetTransaction.cs                 # unified header
│   ├── AssetTransferDetail.cs
│   ├── AssetDisposalDetail.cs
│   ├── AssetRevaluationDetail.cs
│   ├── AssetImpairmentDetail.cs
│   ├── DepreciationSchedule.cs
│   ├── AssetPhysicalCount.cs
│   ├── AssetPhysicalCountDetail.cs
│   ├── AssetAttributeDefinition.cs
│   ├── AssetGroupAttribute.cs
│   └── AssetAttributeValue.cs
├── Enums/                                  # TransactionType/Status, CountStatus, DepreciationStatus, AttributeDataType
├── Services/
│   └── DepreciationCalculator.cs           # pure domain service (FR-090..093)
└── (Events/Assets/* — existing domain events reused/extended)

src/Application/Assets/
├── AssetGroups/                            # re-pointed CRUD + attribute bindings (054)
├── Assets/                                 # re-pointed register CRUD (055)
├── AssetAttributes/                        # definitions + bindings + values
├── AssetTransactions/                      # transfer/disposal/revaluation/impairment create+execute+reverse
├── Depreciation/                           # run, post, reverse
├── PhysicalCounts/                         # create, generate details, record observation, complete, review
└── Common/                                 # responses, mappings, posting-gate validators

src/Application/Accounting/Integration/
└── Assets/                                 # NEW — idempotent outbox consumers creating journal entries
    ├── DepreciationPostedHandler.cs        # aggregated entry + line-level source tagging (Q3: B)
    ├── AssetDisposalPostedHandler.cs
    ├── AssetRevaluationPostedHandler.cs
    ├── AssetImpairmentPostedHandler.cs
    └── AssetImpairmentReversedHandler.cs

src/Domain/Accounting/Entities/
├── JournalEntryTemplateLine.cs             # MODIFIED — nullable LineRole (R4)
└── JournalEntryLine.cs                     # MODIFIED — nullable DepreciationScheduleId (R3)

src/Infrastructure/
├── Data/Configurations/Assets/             # 13 new configs (Restrict FKs, precision, indexes, uniques)
├── Data/Migrations/                        # drop 9 + create 13 (one migration, DEP-030)
├── Data/Seeds/                             # idempotent: template LineRole designation, sequence prefixes
└── (ApplicationDbContext: DbSets swapped to the 13 new entities)

src/Web/Endpoints/Assets/                   # thin endpoint groups per area (permissions per R6)

src/Web/ClientApp/src/features/assets/
├── asset-groups/                           # RE-POINTED (054 screens → new model)
├── assets/                                 # RE-POINTED (055 screens → new model, employee custodian)
├── asset-attributes/                       # NEW
├── asset-transactions/                     # NEW (transfer/disposal/revaluation/impairment)
├── asset-depreciation/                     # NEW
└── asset-counts/                           # NEW

tests/
├── Application.UnitTests/                  # per-use-case success + failure paths (TDD)
├── Domain.UnitTests/                       # DepreciationCalculator, state transitions, scope rules
├── Application.FunctionalTests/            # posting gates, reversal, audit immutability, concurrency (real DB)
└── Web.AcceptanceTests/                    # critical journeys incl. DEP-030 deletion-report verification
```

**Structure Decision**: Follows the established vertical-slice + feature-folder pattern (054/055 precedent). Outbox consumers live on the consuming module's side (`Application/Accounting/Integration/Assets/`) per II. The single drop-and-recreate migration plus DEP-030 record follow the DEP-022/DEP-026 precedents.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| VI — hard-delete of 9 legacy assets tables incl. financial/audit history (C1) | The user-approved C1 decision: the legacy model (4 separate documents, user-based custody, legacy account fields) is superseded by the 13-table design; carrying legacy rows would force mapping four document shapes into the unified model with unprovable field semantics (C5 removed account fields, Q1 changed custodian type, Q2 changed count semantics) | Data migration / archive tables — explicitly rejected by the C1 decision; would preserve data whose semantics the new model no longer defines, and every field-level conflict (C2–C9) would become a migration defect instead of a clean start. Registered as DEP-030 with remediation path per XII |
