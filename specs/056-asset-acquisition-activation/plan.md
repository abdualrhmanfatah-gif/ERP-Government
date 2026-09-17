# Implementation Plan: Asset Acquisition & Activation (الاستحواذ والتفعيل)

**Branch**: `056-asset-acquisition-activation` | **Date**: 2026-09-15 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/056-asset-acquisition-activation/spec.md`

## Summary

Implement asset activation workflow that converts Draft assets to Active status by creating a balanced acquisition journal entry (Debit: AssetGroup.AccountAssetId, Credit: hardcoded acquisition account), linking the journal entry to the asset, and publishing the AssetAcquired domain event. The activation is a single-step operation with Assets.Update permission.

## Technical Context

**Language/Version**: C# 13 / .NET 9, TypeScript 5.x / React 18

**Primary Dependencies**: MediatR, FluentValidation, Entity Framework Core 9, React Query, Zod, react-hook-form

**Storage**: SQL Server via EF Core (existing migrations, `Assets` table needs `JournalEntryId` column)

**Testing**: NUnit + Shouldly + Moq (backend unit tests), FluentAssertions (functional tests), Vitest (frontend)

**Target Platform**: Web application (SPA + REST API)

**Project Type**: Web application (layered: Domain → Application → Infrastructure → Web)

**Performance Goals**: Activation completes in under 2 minutes (SC-001), journal entry creation under 5 seconds

**Constraints**: Arabic-first RTL, shared design tokens, no cascade deletes (Restrict on all FKs), optimistic concurrency via RowVersion

**Scale/Scope**: Government asset register — up to 10,000 assets, ~50 concurrent users, ~50 activations/week

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Command/Handler in Application, Endpoint in Web, Entity in Domain. |
| II. Bounded Contexts | ✅ PASS | Assets module is self-contained. Journal entry creation reuses Accounting module services. |
| III. Server-Side Business Rules | ✅ PASS | Activation validation, journal entry creation, status transitions all server-side. |
| IV. Financial Integrity | ✅ PASS | Balanced journal entry (debit = credit) is enforced. Acquisition account hardcoded to prevent drift. |
| V. Budget Control | N/A | No budget expenditure in activation. |
| VI. Data Integrity | ✅ PASS | Optimistic concurrency (RowVersion), Restrict FKs, accounting period validation. |
| VII. Authorization | ✅ PASS | Reuses Assets.Update permission. No new permissions needed. |
| VIII. Audit Immutability | ✅ PASS | Journal entry is immutable after posting. Activation logged with actor/timestamp. |
| IX. API Contract | ✅ PASS | Single POST endpoint with problem-details error responses. |
| X. UI/Design System | ✅ PASS | Arabic-first RTL, inline loading state on button. |
| XI. Testing | ⚠️ NOTE | TDD is NON-NEGOTIABLE per Principle XI. Tests must be written first. |
| XII. Controlled Change | ✅ PASS | New files within existing Assets module. JournalEntryId added to Asset entity. |
| XIII. Error Handling | ✅ PASS | Result<T> with ErrorCategory, structured error codes. |

**Gate Result**: PASS with notes. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/056-asset-acquisition-activation/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Assets/Entities/
│   └── Asset.cs                          # EDIT — add JournalEntryId field
├── Domain/Events/Assets/
│   └── AssetAcquired.cs                  # EXISTING — will be raised by handler
├── Application/Assets/Assets/
│   └── Commands/
│       └── ActivateAsset/
│           └── ActivateAssetCommand.cs   # NEW — activation command + handler
├── Web/Endpoints/Assets/
│   └── Assets.cs                         # EDIT — add POST /{id}/activate route
├── Web/ClientApp/src/features/assets/assets/
│   ├── components/
│   │   └── ActivationForm.tsx            # NEW — activation confirmation form
│   ├── hooks/
│   │   └── useAssets.ts                  # EDIT — add useActivateAsset hook
│   └── pages/
│       └── AssetDetailPage.tsx           # EDIT — add activate button for Draft assets
```

**Structure Decision**: Follows the established vertical slice pattern. Activation command is a new slice under `Application/Assets/Assets/Commands/ActivateAsset/`. Frontend adds activation form component and updates detail page.

## Complexity Tracking

> No Constitution violations requiring justification.

No entries.
