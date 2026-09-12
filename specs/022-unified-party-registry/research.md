# Research: Unified Party Registry & Shared Document Panels

**Feature**: 022-unified-party-registry
**Date**: 2026-09-06

## Gap Analysis

| # | Gap | Impact | Resolution |
|---|-----|--------|------------|
| 1 | No attachment gate check endpoint | Frontend cannot show gate badge before approval | Add `GET /api/Documents/{documentType}/{documentId:int}/attachment-gate-check` endpoint |
| 2 | No attachment download endpoint | Users cannot view/download uploaded files | Add `GET /api/Documents/attachments/{id:int}/download` endpoint |
| 3 | NSwag client returns `Promise<void>` for Documents endpoints | Generated client types are stale/wrong | Regenerate NSwag client after adding proper OpenAPI metadata |
| 4 | No `[Authorize]` on Documents endpoints | Auth not enforced at endpoint level (only handler level) | Add `.RequireAuthorization()` to each Documents route — out of scope for this feature (existing gap) |
| 5 | Party-to-document relationship query undefined | Related documents section (US4) needs a query | Add `GET /api/Parties/{id:int}/documents` endpoint |

## Decisions

### D1: Attachment Gate Check Endpoint

**Decision**: Add a new lightweight endpoint that calls `IAttachmentGateService.CheckMandatoryAttachmentsAsync` and returns the list of missing type codes.

**Rationale**: The gate service already exists and is used internally by approval commands. Exposing it as a GET endpoint lets the frontend show the gate badge in real time without triggering an approval attempt. The endpoint returns a simple list of missing type codes — no complex DTO needed.

**Alternatives considered**:
- Client-side gate evaluation: Rejected — would require duplicating business logic (which types are mandatory) in the frontend. The backend is the source of truth.
- Embed gate check in approval response: Rejected — the gate should be visible *before* the user clicks approve, not as a error after.

### D2: Attachment Download Endpoint

**Decision**: Add `GET /api/Documents/attachments/{id:int}/download` that streams the file via `IFileStorageService.OpenReadAsync`.

**Rationale**: The `IFileStorageService` already has `OpenReadAsync` method. Wiring it to an HTTP endpoint is straightforward. Users need to view/download attachments to verify document completeness.

**Alternatives considered**:
- Pre-signed URLs: Rejected — overkill for internal government ERP with no cloud storage.
- Direct file system links: Rejected — exposes storage paths, no auth enforcement.

### D3: Party Documents Query Endpoint

**Decision**: Add `GET /api/Parties/{id:int}/documents` that queries documents referencing the party via `DocumentType` + `DocumentId` pattern across known document type tables.

**Rationale**: The spec requires a related documents section on the party detail page. The backend needs to aggregate across multiple document type tables (ReceiptVouchers, PaymentOrders, Encumbrances) where the party is referenced.

**Alternatives considered**:
- Generic polymorphic query service: Rejected — too broad for this feature. A party-specific endpoint is simpler and can be generalized later.
- Frontend makes multiple parallel requests per document type: Rejected — N+1 problem, slow for parties with many documents.

### D4: NSwag Client Regeneration

**Decision**: After adding the new endpoints, regenerate the NSwag client. The existing `PartiesClient` is already generated and functional. The new Documents endpoints will produce correct types after regeneration.

**Rationale**: The NSwag pipeline (`npm run generate-api`) reads the OpenAPI spec from the running backend. Adding proper `[ProducesResponseType]` attributes to the new endpoints ensures correct type generation.

**Alternatives considered**:
- Hand-write all API clients: Rejected — duplicates effort, risk of drift. NSwag is the project convention.
- Skip NSwag, use manual fetch only: Rejected — the user specified "NSwag: PartiesClient + documents generic endpoints client as generated."

### D5: Shared Panel Component Architecture

**Decision**: Three components in `features/documents/components/` — `ApprovalsPanel`, `StatusLogPanel`, `AttachmentsPanel`. Each accepts a `documentType: string` and `documentId: number` prop. They call the generic Documents API endpoints. Exported via `features/documents/shared/index.ts` for import by any feature.

**Rationale**: Follows the existing pattern where `features/budgeting/components/ApprovalHistoryPanel.tsx` is a reusable panel. The new panels are more generic (work across all document types) so they live in a dedicated `documents` feature folder rather than being tied to `budgeting`.

**Alternatives considered**:
- Put panels in `src/components/ui/`: Rejected — these are feature-level shared components with domain logic (API calls), not generic UI primitives.
- Put panels in each feature that uses them: Rejected — violates DRY, defeats the "zero custom code" success criterion.

### D6: Duplicate Tax Number Check Strategy

**Decision**: Frontend calls `GET /api/Parties?search={taxNumber}` on field blur, then filters results client-side to check for exact TaxNumber match. No dedicated backend endpoint needed.

**Rationale**: The existing `GetPartiesQuery` already supports search by TaxNumber. The frontend can reuse this endpoint rather than adding a new one. The duplicate check is a soft warning (per spec clarification), so a dedicated endpoint is unnecessary overhead.

**Alternatives considered**:
- Dedicated `GET /api/Parties/check-duplicate?taxNumber=X` endpoint: Rejected — overkill for a soft warning. The existing search endpoint provides the same information.
- Real-time validation on every keystroke: Rejected — the spec says "within 2 seconds of field losing focus," so blur-based is sufficient.

## Existing Patterns to Follow

| Pattern | Exemplar | File |
|---------|----------|------|
| Manual API client | `budgetTypesClient` | `src/Web/ClientApp/src/features/budgeting/shared/client.ts` |
| React-query hook | `useBudgets` | `src/Web/ClientApp/src/features/budgeting/hooks/useBudgets.ts` |
| Page component | `BudgetsListPage` | `src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetsListPage.tsx` |
| Co-located test | `BudgetsListPage.test.tsx` | `src/Web/ClientApp/src/features/budgeting/__tests__/BudgetsListPage.test.tsx` |
| Shared panel | `ApprovalHistoryPanel` | `src/Web/ClientApp/src/features/budgeting/components/ApprovalHistoryPanel.tsx` |
| Route config | `routes.tsx` | `src/Web/ClientApp/src/app/routes.tsx` |
| Permission check | `usePermission` | `src/Web/ClientApp/src/shared/hooks/usePermission.ts` |
