# Implementation Plan: Asset Transfers

## Overview

Complete the asset-transfers feature of the asset module (spec 060). The write slice exists in a partial, unsafe form: `CreateAssetTransferCommand` ignores destination validation, `ExecuteAssetTransferCommand` ignores its `RowVersion`, and no read/edit/cancel paths exist while the frontend already calls `GET/PUT /api/AssetTransfers`. This plan completes the vertical slice: queries, validated create/update/cancel, safe idempotent execute, endpoints, tests, and the frontend pages.

## Technical Context

### Tech Stack
- **Backend**: .NET 10 / C# 13, Clean Architecture (Domain → Application → Infrastructure → Web), MediatR, FluentValidation
- **ORM**: EF Core + SQL Server; no schema changes needed (entities exist from DEP-030)
- **Frontend**: React 19 + TypeScript, TanStack Query, React Hook Form + Zod, Tailwind, Arabic RTL
- **Testing**: NUnit + Shouldly + Moq, EF InMemory provider in unit tests

### Dependencies
```
060-asset-transfers depends on:
  └── 058-asset-module-spec (13-table model, DEP-030, permissions, sequences — all landed)
      └── 054/055 asset groups + register CRUD
      └── plan/feature-context-asset-transfers.md (decisions Q1–Q4)
```

### Unknowns
None — Q1–Q4 resolved 2026-09-16; no schema changes.

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Endpoints → MediatR handlers; no logic in Web |
| II. Bounded Contexts | PASS | No cross-module writes |
| III. Server-Side Rules | PASS | All validation in handlers/validators |
| IV. Financial Integrity | PASS | No posting for transfers; `IsPosted=false` enforced |
| VI. Data Integrity | PASS | Optimistic concurrency on header + card; no hard deletes (cancel = status) |
| VII. Authorization | PASS | Existing `AssetTransfers.View/Create/Execute` on every endpoint |
| XI. Testing | PASS | New unit tests; existing snapshot tests updated to the new contract |
| XIII. Error Handling | PASS | Stable codes, 409 conflicts, unified ProblemDetails |

**Gate**: PASS — no new exceptions.

## Data Model

No migration. `AssetTransactionStatus` gains `Cancelled = 7` (additive int enum member; column has no check constraint).

## API Surface

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | `/api/AssetTransfers` | `AssetTransfers.View` | Paginated list (search, status) |
| GET | `/api/AssetTransfers/{id}` | `AssetTransfers.View` | Detail + names + tokens |
| POST | `/api/AssetTransfers` | `AssetTransfers.Create` | Create draft |
| PUT | `/api/AssetTransfers/{id}` | `AssetTransfers.Create` | Update draft |
| POST | `/api/AssetTransfers/{id}/execute` | `AssetTransfers.Execute` | Execute (atomic + idempotent) |
| POST | `/api/AssetTransfers/{id}/cancel` | `AssetTransfers.Create` | Cancel draft |

## Files

### Backend — create
- `src/Application/Assets/AssetTransactions/Transfers/Common/AssetTransferResponses.cs` — list item + detail DTOs and mapping
- `src/Application/Assets/AssetTransactions/Transfers/Queries/GetAssetTransfersQuery.cs`
- `src/Application/Assets/AssetTransactions/Transfers/Queries/GetAssetTransferByIdQuery.cs`
- `src/Application/Assets/AssetTransactions/Transfers/Commands/UpdateAssetTransferCommand.cs`
- `src/Application/Assets/AssetTransactions/Transfers/Commands/CancelAssetTransferCommand.cs`
- `src/Application/Assets/AssetTransactions/Transfers/Common/AssetTransferRules.cs` — shared destination validation + from-snapshot + department resolution helpers

### Backend — modify
- `src/Domain/Assets/Enums/AssetTransactionStatus.cs` — add `Cancelled = 7`
- `src/Application/Common/Errors/ErrorCodes.cs` — add `Assets.TransferNotFound`, `Assets.InvalidTransferDestination`, `Assets.TransferSourceChanged`
- `src/Application/Assets/AssetTransactions/Transfers/Commands/CreateAssetTransferCommand.cs` — validated create, snapshots, no misleading From inputs
- `src/Application/Assets/AssetTransactions/Transfers/Commands/ExecuteAssetTransferCommand.cs` — concurrency (header + card), source re-validation, idempotency, occurrence time
- `src/Web/Endpoints/Assets/AssetTransfers.cs` — full surface

### Tests
- `tests/Application.UnitTests/Assets/AssetTransfers/TransferLifecycleTests.cs` (create/update/cancel/execute happy paths)
- `tests/Application.UnitTests/Assets/AssetTransfers/TransferValidationTests.cs` (destination rules, active asset, draft-only)
- `tests/Application.UnitTests/Assets/AssetTransfers/TransferExecutionTests.cs` (source change, concurrency, idempotency, occurrence/department snapshots)
- `tests/Application.UnitTests/Assets/AssetTransfers/TransferQueryTests.cs` (filters, pagination, detail mapping)
- `tests/Application.UnitTests/Assets/AssetTransfers/TransferDepartmentSnapshotTests.cs` (update to new contract)

### Frontend
- modify `src/Web/ClientApp/src/features/assets/asset-transactions/transfers/shared/types.ts`, `hooks/useTransfers.ts`
- create `shared/schemas.ts`, `components/TransferForm.tsx`, `pages/TransferCreatePage.tsx`, `pages/TransferEditPage.tsx`
- modify `pages/TransfersListPage.tsx`, `pages/TransferDetailPage.tsx`
- modify `src/Web/ClientApp/src/app/routes.tsx`, `src/Web/ClientApp/src/layouts/navigation.ts` (terminology «نقل الأصول»)

## Execution Order (TDD)

1. RED: write the transfer test files against the new contract → run `dotnet test tests/Application.UnitTests --filter AssetTransfers` → observe failures
2. GREEN: implement errors → rules → queries → commands → endpoint → re-run
3. Frontend: types → hooks → form/pages → routes/labels → `npm run lint` + `npm run build`
4. Gate: `dotnet build src/Web/Web.csproj` + focused `dotnet test` + frontend checks

## Complexity Tracking

| Item | Why needed |
|------|-----------|
| Manual RowVersion comparison in execute/update | EF InMemory cannot raise `DbUpdateConcurrencyException`; manual compare gives deterministic tests and real-SQL safety combined with EF token |
