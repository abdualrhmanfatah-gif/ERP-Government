# Implementation Plan: Asset Attributes

## Overview

Complete the flexible attributes feature for the ERP Government asset module. Backend domain and endpoints partially exist (058 tasks T016-T018, T036-T038). Frontend is entirely missing. This plan adds: data type expansion, definitions management UI, group bindings page, asset form dynamic section, and asset detail attributes tab.

## Technical Context

### Tech Stack
- **Backend**: ASP.NET 9, C#, Clean Architecture (Domain → Application → Infrastructure → Web)
- **ORM**: Entity Framework Core 9 with SQL Server
- **CQRS**: MediatR with vertical slices
- **Validation**: FluentValidation (server), Zod (frontend)
- **Frontend**: React 18, TypeScript, React Query, Tailwind CSS, nswag-generated clients
- **Testing**: xUnit, FluentAssertions, Playwright (acceptance)

### Known Patterns
- **Endpoints**: Minimal API with `IEndpointGroup` pattern, permission-based authorization
- **Commands**: MediatR `IRequest<Result<T>>` with FluentValidation validators
- **Queries**: MediatR with `IApplicationDbContext` EF Core queries
- **Frontend**: Feature folders with `hooks/`, `pages/`, `components/`, `shared/` structure
- **API Client**: nswag-generated `web-api-client.ts` from OpenAPI spec

### Dependencies
```
059-asset-attributes depends on:
  └── 058-asset-module-spec (domain entities, endpoints, seeds exist)
      └── 054-asset-groups-crud (group CRUD pages exist)
      └── 055-asset-register-crud (asset form pages exist)
```

### Unknowns
None — all design decisions resolved during specify phase.

## Constitution Check

### Applicable Principles

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | New code follows Domain → Application → Infrastructure → Web. Endpoints delegate to use cases. No business logic in Web. |
| II. Bounded Contexts | ✅ PASS | Attributes are within Assets module. No cross-module writes. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All validation enforced server-side (FluentValidation + AttributeValueValidator). FR-021 (type change block) enforced in UpdateAssetAttributeDefinitionCommand. |
| IV. Financial Integrity | ⚠️ N/A | Attributes are non-financial. No journal entries, no posting. |
| V. Budget Control | ⚠️ N/A | Not a spending document. |
| VI. Data Integrity | ✅ PASS | EF migrations for schema changes. FK Restrict on all foreign keys. Optimistic concurrency via RowVersion. No hard deletes. |
| VII. Authorization | ✅ PASS | All endpoints declare permissions: `AssetGroups.View/Create/Update`. Asset endpoints use `Assets.View/Create/Update`. |
| VIII. Approval Workflows | ⚠️ N/A | No approval cycle for attribute definitions or values. |
| IX. API Contract Integrity | ✅ PASS | OpenAPI spec generated from endpoints. web-api-client regenerated after changes. |
| X. UI and Design System | ✅ PASS | Arabic-first RTL. Design tokens used. Shared UI components (Input, Select, Switch, Badge, Table). |
| XI. Testing | ✅ PASS | Unit tests for all handlers/validators. Functional tests with real DB. Acceptance tests for critical journeys. TDD for new features. |
| XII. Controlled Architectural Change | ⚠️ N/A | No changes to layers, module boundaries, or integration patterns. This is a feature addition within existing architecture. |
| XIII. Error Handling | ✅ PASS | Problem-details contract. Stable error codes. Arabic messages. Field-level errors for validation failures. |

### Registered Exceptions

| # | Exception | Applicable? | Notes |
|---|-----------|-------------|-------|
| 1 | Open endpoint authorization policies | Yes — existing debt | Use-case-layer authorization is effective control |
| 5 | Stubbed functional scenario tests | Yes — existing debt | Existing stubs not weakened; new tests are real |
| 6 | Spec 045 TDD exception | No | This is spec 059, not 045 |

### Gate Evaluation

**Result**: ✅ PASS — No constitution violations. No new registered exceptions needed.

## Dependencies

```
059-asset-attributes depends on:
  └── 058-asset-module-spec (domain entities, endpoints, seeds exist)
      └── 054-asset-groups-crud (group CRUD pages exist)
      └── 055-asset-register-crud (asset form pages exist)
```

## Phase 0: Research

All unknowns resolved during specify phase. Key decisions documented in `research.md`:
- R1: Data type expansion (4 → 5 types)
- R2: Separate page for group bindings
- R3: Full CRUD screen for definitions
- R4: Dynamic section in asset form
- R5: Atomic binding save
- R6: Deactivated definition display
- R7: Validation architecture (server-side only)

## Phase 1: Backend — Data Type Expansion (T059-T065)

**Goal**: Expand AssetAttributeDataType from 4 to 5 types; add IntegerValue column; rename NumericValue to DecimalValue.

### Deliverables
- Updated enum `AssetAttributeDataType` (Text=1, Integer=2, Decimal=3, Date=4, Boolean=5)
- `AssetAttributeValue` entity: add `IntegerValue? int`, rename `NumericValue` → `DecimalValue`
- EF configuration updated
- `AttributeValueValidator` updated for both Integer and Decimal
- EF migration
- Seed data updated

### Tests
- Unit: validator rejects wrong type for Integer and Decimal
- Functional: create/read attribute values with all 5 types

### Constitution Compliance
- **VI. Data Integrity**: Migration versioned, FK Restrict, RowVersion, no hard deletes
- **III. Server-Side Validation**: Validator enforces type correctness server-side

---

## Phase 2: Backend — Definitions API Enhancement (T066-T072)

**Goal**: Add search, filter, pagination to definitions query. Add GET by ID endpoint. Add linked value count.

### Deliverables
- `GetAssetAttributeDefinitionsQuery` updated: search, filter, pagination, totalCount
- New `GetAssetAttributeDefinitionByIdQuery` with linkedValueCount
- New endpoint: `GET /api/AssetAttributes/definitions/{id}`
- Update `CreateAssetAttributeDefinitionCommand` to accept Description, Unit, SortOrder
- Update `UpdateAssetAttributeDefinitionCommand` with FR-021 validation (block DataType change when values exist)

### Tests
- Unit: search by code/name, filter by type/status, pagination
- Unit: FR-021 blocks DataType change when values exist
- Functional: full CRUD lifecycle with filters

### Constitution Compliance
- **III. Server-Side Validation**: FR-021 enforced in command handler
- **VII. Authorization**: All endpoints declare permissions
- **XIII. Error Handling**: 409 for DataType conflict, 404 for not found

---

## Phase 3: Backend — Group Detail + Asset Detail Responses (T073-T078)

**Goal**: Include attribute bindings in group detail response. Include attribute values + derived department in asset detail response.

### Deliverables
- `GetAssetGroupDetailQuery` response: add `attributeBindings` list
- `GetAssetByIdQuery` response: add `attributeValues` list + `currentDepartment`
- Asset create/update commands: validate attribute values against group bindings

### Tests
- Unit: group detail includes bindings
- Unit: asset detail includes values + department
- Functional: asset create with attribute values persists correctly

### Constitution Compliance
- **IX. API Contract Integrity**: Response shapes documented in OpenAPI
- **III. Server-Side Validation**: Values validated against bindings at save time

---

## Phase 4: Frontend — Attribute Definitions Management (T079-T088)

**Goal**: Full CRUD screens for attribute definitions.

### Deliverables
- `src/Web/ClientApp/src/features/assets/asset-attributes/` feature folder
  - `pages/AssetAttributesListPage.tsx`
  - `pages/AssetAttributeCreatePage.tsx`
  - `pages/AssetAttributeEditPage.tsx`
  - `components/AssetAttributeForm.tsx`
  - `hooks/useAssetAttributes.ts`
  - `shared/types.ts`
  - `shared/schemas.ts`
- Navigation: add "تعريفات الخصائص" to assets section in navigation.ts
- Route registration in routes.tsx

### Tests
- Acceptance: list, create, edit, toggle active, search, filter

### Constitution Compliance
- **X. UI and Design System**: Arabic-first, RTL, design tokens, shared components
- **XI. Testing**: Acceptance tests for critical journeys

---

## Phase 5: Frontend — Group Attributes Page (T089-T095)

**Goal**: Dedicated page for managing group attribute bindings.

### Deliverables
- `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupAttributesPage.tsx`
- Update `useAssetGroups.ts` hook for bindings API
- Route: `/assets/groups/:id/attributes`
- Navigation: link from group detail page

### Tests
- Acceptance: add binding, remove binding, set required, set sort order, save

### Constitution Compliance
- **X. UI and Design System**: Shared components, Arabic-first
- **XI. Testing**: Acceptance tests

---

## Phase 6: Frontend — Asset Form Dynamic Section (T096-T104)

**Goal**: Dynamic attributes section in asset create/edit form.

### Deliverables
- `src/Web/ClientApp/src/features/assets/assets/components/AssetAttributesSection.tsx`
- Update `AssetForm.tsx` to include dynamic section
- Update `useAssets.ts` hook for attribute values
- Zod schema: dynamic field definitions per group bindings
- `data-attribute` selectors for acceptance tests

### Tests
- Acceptance: select group → attributes appear, fill values, save, verify persistence

### Constitution Compliance
- **III. Server-Side Validation**: Client validation is UX only; server enforces
- **X. UI and Design System**: Dynamic section uses shared components
- **XI. Testing**: Acceptance tests for primary flow

---

## Phase 7: Frontend — Asset Detail Attributes Tab (T105-T109)

**Goal**: Display attribute values in asset detail.

### Deliverables
- `src/Web/ClientApp/src/features/assets/assets/components/AssetAttributesTab.tsx`
- Update `AssetDetailPage.tsx` to include tab
- Value formatting per data type
- Deactivated definition indicator

### Tests
- Acceptance: view asset with attributes, verify values display

### Constitution Compliance
- **X. UI and Design System**: Formatting per data type, Arabic labels
- **XI. Testing**: Acceptance test

---

## Phase 8: Tests + Acceptance (T110-T118)

**Goal**: Complete test coverage and acceptance test execution.

### Deliverables
- Unit tests: all new/modified handlers and validators
- Functional tests: full lifecycle with real DB
- Acceptance tests: update `AssetLifecycle.feature` step definitions to remove placeholders
- Update `AssetGroupPage.cs` and `AssetRegisterPage.cs` with real selectors

### Tests
- Run `dotnet test`
- Run acceptance tests (verify all steps pass)

### Constitution Compliance
- **XI. Testing**: All use cases have unit tests. Functional tests against real DB. Acceptance tests for critical journeys. TDD for new features.

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| web-api-client regeneration breaks other features | Low | High | Run full build + test after regeneration |
| EF migration conflicts with 058 migration | Medium | Medium | Check migration snapshot; rebase if needed |
| Zod schema drift from server validation | Medium | Low | Auto-generate from contracts if possible |
| Acceptance test selectors brittle | Medium | Low | Use data-testid consistently |

## Checkpoints

| Checkpoint | Criteria | Gate |
|------------|----------|------|
| After Phase 1 | 5 types work end-to-end | Unit + functional tests pass |
| After Phase 4 | Definitions CRUD works in UI | Manual smoke test |
| After Phase 6 | Asset form shows attributes | Acceptance test scenario 3 passes |
| After Phase 8 | All tests green | Full test suite passes |
