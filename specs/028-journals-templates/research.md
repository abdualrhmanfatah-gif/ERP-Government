# Research: ACC-03 — Journals & Templates

**Date**: 2026-09-07
**Amend 2026-09-07 (US4 lines)**: Prior R1 claim "backend fully built" stale for lines. Entity + Dto exist (`JournalEntryTemplateLine.cs`, `AccountingDtos.cs:311`) but no Commands/Queries/Endpoints/Hooks/UI. Exemplar = `JournalEntryLines` (Create/Update/Remove commands, `useJournalEntryLines.ts` invalidate pattern, `JournalEntryDetailPage.tsx:172` read table, `JournalEntryCreatePage.tsx:214` editor + `BalanceIndicator`). Decision: mirror that exemplar 1:1 except FK `TemplateId` + no Draft gate (template active flag instead).

## R1: Backend Completeness Assessment

**Decision**: Backend is fully built. No new backend work needed.

**Evidence**:
- Domain entities: `Journal.cs`, `JournalEntryTemplate.cs` with enums
- Commands: CreateJournal, UpdateJournal, CreateTemplate, UpdateTemplate (with handlers + validators)
- Queries: GetJournalsList, GetJournalById, GetTemplatesList, GetTemplateById
- Endpoints: Journals.cs, Templates.cs (IEndpointGroup pattern)
- NSwag-generated clients: `JournalsClient`, `TemplatesClient` in web-api-client.ts

**Rationale**: All CRUD operations, validation, and permission gating are implemented. The frontend consumes these via generated API clients.

## R2: Frontend Pattern Analysis

**Decision**: Follow existing `account-groups/` subdirectory pattern for new `journals/` and `templates/` modules.

**Evidence**:
- `account-groups/pages/` + `account-groups/components/` structure exists
- `useJournalsList.ts` hook already exists (needs extension for type filter)
- `shared/client.ts` pattern for API client instantiation
- `PageHeader`, `FilterBar`, `FilterSelect` components available

**Alternatives considered**:
- Root-level pages (like Accounts pages) — rejected because journals/templates are distinct entities warranting their own subdirectories, similar to account-groups.

## R3: API Client Method Signatures

**Decision**: Use NSwag-generated clients directly. Extend shared/client.ts with instances.

**JournalsClient methods**:
- `journalsAll(isActive?, type?)` → `JournalDto[]`
- `journalsPOST(body: CreateJournalCommand)` → `void`
- `journalsGET(id)` → `JournalDto` (inferred from endpoint)
- `journalsPUT(id, body: UpdateJournalCommand)` → `void`

**TemplatesClient methods**:
- `templatesAll(isActive?, journalId?, templateType?)` → `JournalEntryTemplateDto[]`
- `templatesPOST(body: CreateTemplateCommand)` → `void`
- `templatesGET(id)` → `JournalEntryTemplateDto` (inferred from endpoint)
- `templatesPUT(id, body: UpdateTemplateCommand)` → `void`

## R4: Permission Code Names

**Decision**: Use backend-registered codes as-is. Frontend uses `usePermission` hook for gating.

**Codes** (from backend endpoints):
- `Accounting.Journals.Read` — list + detail
- `Accounting.Journals.Create` — create
- `Accounting.Journals.Edit` — update
- `Accounting.Templates.Read` — list + detail
- `Accounting.Templates.Create` — create
- `Accounting.Templates.Update` — update

**Note**: Permission codes in spec (View/Create/Update) differ slightly from backend (Read/Create/Edit). Frontend must match backend registration. Resolve at implementation time.

## R5: Journal Picker for Entry Creation (Spec 023 Integration)

**Decision**: Expose `useJournalsList({ isActive: true })` hook for picker consumption. Spec 023 entry creation page will import this hook.

**Impact**: No new endpoints needed. The existing `GET /api/Journals?IsActive=true` endpoint already supports active-only filtering.
