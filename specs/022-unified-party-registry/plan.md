# Implementation Plan: Unified Party Registry & Shared Document Panels

**Branch**: `022-unified-party-registry` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/022-unified-party-registry/spec.md`

## Summary

Build the party management UI (list + detail pages) and three reusable document panels (approvals timeline, status log, attachments with mandatory gate) that plug into any existing or future document screen. No new backend entities — all work is frontend over existing backend services.

## Technical Context

**Language/Version**: TypeScript 5.9, React 19

**Primary Dependencies**: Tanstack Query 5, react-hook-form 7, zod 3, shadcn/ui (Tailwind CSS 3), react-router-dom 7, Vitest 4

**Storage**: N/A (frontend only — backend persists via EF Core + SQL Server)

**Testing**: Vitest 4 + @testing-library/react + jsdom + jest-dom/vitest

**Target Platform**: Desktop web (Chromium, Firefox, Safari) — RTL Arabic layout

**Project Type**: Web application (React SPA with Vite 8)

**Performance Goals**: Party search < 1s for 10k records; related documents < 2s for 100 items; duplicate tax check < 2s on blur

**Constraints**: Desktop-first; Arabic-primary UI; no mobile-responsive requirement; no new backend entities

**Scale/Scope**: 2 new pages (PartiesListPage, PartyDetailPage), 3 new shared components (ApprovalsPanel, StatusLogPanel, AttachmentsPanel), 2 feature folders (features/parties/, features/documents/)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution.md found. Skipping constitution gate check.

Relevant AGENTS.md principles applied:
- Minimal APIs only — no controllers (backend already follows this)
- Every handler returns Result<T> (frontend handles Result wrapper in API client)
- [Authorize(Policy = PermissionCodes.X)] on every endpoint (backend has placeholder policies — UI hides buttons per spec clarification)
- Frontend: React 19 + TypeScript + Vite in src/Web/ClientApp
- API clients: NSwag-generated + manual fetch wrappers in features/<domain>/shared/client.ts
- Tests: Vitest + @testing-library/react, co-located in features/<domain>/__tests__/

## Project Structure

### Documentation (this feature)

```text
specs/022-unified-party-registry/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-contracts.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/
│   ├── parties/                              # NEW — party management
│   │   ├── __tests__/
│   │   │   ├── PartiesListPage.test.tsx      # filter/search/toggle tests
│   │   │   ├── PartyDetailPage.test.tsx      # detail + related docs tests
│   │   │   └── DuplicateTaxWarning.test.tsx  # duplicate tax number UX
│   │   ├── shared/
│   │   │   ├── client.ts                     # partiesClient (manual fetch wrappers)
│   │   │   ├── types.ts                      # DTOs, enums, label maps, filter types
│   │   │   └── index.ts                      # barrel export
│   │   ├── hooks/
│   │   │   └── useParties.ts                 # react-query hooks wrapping partiesClient
│   │   └── pages/
│   │       ├── PartiesListPage.tsx           # list with search, filter, toggle active
│   │       └── PartyDetailPage.tsx           # profile + related documents tabs
│   │
│   └── documents/                            # NEW — shared document panels (no routes)
│       ├── __tests__/
│       │   ├── ApprovalsPanel.test.tsx       # renders fixture data, pending states
│       │   ├── StatusLogPanel.test.tsx       # renders fixture data, empty states
│       │   └── AttachmentsPanel.test.tsx     # upload/delete, gate badge states
│       ├── components/
│       │   ├── ApprovalsPanel.tsx            # reusable approvals timeline
│       │   ├── StatusLogPanel.tsx            # reusable status history log
│       │   └── AttachmentsPanel.tsx          # upload/delete + gate badge indicator
│       └── shared/
│           ├── client.ts                     # documentsClient (generic document endpoints)
│           ├── types.ts                      # shared document DTOs
│           └── index.ts                      # barrel export
│
├── app/
│   └── routes.tsx                            # ADD: /parties route entries
```

**Structure Decision**: Two feature folders. `features/parties/` owns party-specific pages, hooks, and API client. `features/documents/` owns the three reusable panels with no routes — imported by parties and future document screens (024, 025, 021). This follows the existing pattern where `features/budgeting/components/` holds cross-cutting panels like `ApprovalHistoryPanel.tsx`.

---

## 1. FIELD-COVERAGE TABLE

Every field from spec.md Field Contract mapped to screen → component → editable/readonly → formatter → permission. Absent = plan defect.

### Party Fields

| Field | Screen | Component | Mode | Formatter / Validator | Permission |
|-------|--------|-----------|------|----------------------|------------|
| PartyCode | PartiesListPage | DataGrid column | read-only | `{PREFIX}-{D6}` — auto-generated, not editable | Parties.View |
| PartyCode | PartyDetailPage | Profile header | read-only | Display only in header | Parties.View |
| PartyType | PartiesListPage | DataGrid column | read-only | Translated label via PARTY_TYPE_LABELS | Parties.View |
| PartyType | PartyDetailPage | Profile header (detail) / Create form field | editable (create only, locked after save) | Dropdown: Supplier=0, Customer=1, GovEntity=2, TaxAuthority=3, Other=4 | Parties.Create / Parties.Update |
| NameAr | PartiesListPage | DataGrid column (primary display) | read-only | Arabic text, primary search target | Parties.View |
| NameAr | PartyDetailPage | Profile header + Edit form field | editable | Required. Max 200 chars. zod: `z.string().min(1).max(200)` | Parties.View / Parties.Update |
| NameEn | PartyDetailPage | Profile body + Edit form field | editable | Optional. Max 200 chars. zod: `z.string().max(200).optional()` | Parties.View / Parties.Update |
| TaxNumber | PartiesListPage | DataGrid column | read-only | Display only | Parties.View |
| TaxNumber | PartyDetailPage | Profile body + Edit form field | editable | Max 20 chars. Soft duplicate warning on blur. zod: `z.string().max(20).optional()` | Parties.View / Parties.Update |
| NationalId | PartyDetailPage | Profile body + Edit form field | editable | Max 20 chars. zod: `z.string().max(20).optional()` | Parties.View / Parties.Update |
| Phone | PartyDetailPage | Profile body + Edit form field | editable | Max 20 chars. zod: `z.string().max(20).optional()` | Parties.View / Parties.Update |
| Email | PartyDetailPage | Profile body + Edit form field | editable | Max 200 chars. Email format. zod: `z.string().email().max(200).optional()` | Parties.View / Parties.Update |
| Address | PartyDetailPage | Profile body + Edit form field | editable | Max 500 chars. Free text. zod: `z.string().max(500).optional()` | Parties.View / Parties.Update |
| Notes | PartyDetailPage | Profile body + Edit form field | editable | Max 1000 chars. Free text. zod: `z.string().max(1000).optional()` | Parties.View / Parties.Update |
| IsActive | PartiesListPage | DataGrid column (badge) | read-only | Badge: active=green, inactive=gray | Parties.View |
| IsActive | PartyDetailPage | Toggle button | action (toggle) | Toggle Active button with deactivation guard | Parties.Update |

### ApprovalHistory Fields (shared panel — read-only)

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| ApproverName | Any document detail | ApprovalsPanel timeline card title | read-only | User full name lookup | Parties.View (on party detail) |
| DecisionAt | Any document detail | ApprovalsPanel timeline card timestamp | read-only | Relative time + absolute on hover (RTL) | Parties.View |
| Decision | Any document detail | ApprovalsPanel timeline card badge | read-only | Approved=green, Rejected=red, Pending=amber | Parties.View |
| Reason | Any document detail | ApprovalsPanel timeline card body | read-only | Text or "—" if null | Parties.View |

### DocumentStatusLog Fields (shared panel — read-only)

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| FromStatus | Any document detail | StatusLogPanel table cell | read-only | Status badge (localized) | Parties.View |
| ToStatus | Any document detail | StatusLogPanel table cell | read-only | Status badge (localized) | Parties.View |
| ChangedBy | Any document detail | StatusLogPanel table cell | read-only | User full name lookup | Parties.View |
| ChangedAt | Any document detail | StatusLogPanel table cell | read-only | Relative time + absolute on hover | Parties.View |
| Reason | Any document detail | StatusLogPanel table cell | read-only | Text or "—" if null | Parties.View |

### Attachment Fields (shared panel — editable)

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| FileName | Any document detail | AttachmentsPanel list item primary | read-only | Click to download | Parties.View |
| AttachmentTypeCode | Any document detail | AttachmentsPanel list item badge | read-only | From requirements or "Other" | Parties.View |
| UploadedBy | Any document detail | AttachmentsPanel list item secondary | read-only | User full name lookup | Parties.View |
| CreatedAt | Any document detail | AttachmentsPanel list item secondary | read-only | Relative time | Parties.View |
| SizeBytes | Any document detail | AttachmentsPanel list item secondary | read-only | Human-readable (KB/MB) | Parties.View |
| Gate status | Any document detail | AttachmentsPanel badge above panel | computed | "Missing required attachment: {code}" (amber) or "All required attachments uploaded" (green) | Parties.View |

### PartyDocumentResponse Fields (related documents — read-only)

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| DocumentType | PartyDetailPage | RelatedDocuments DataGrid column | read-only | Localized document type label | Parties.View |
| DocumentNumber | PartyDetailPage | RelatedDocuments DataGrid column | read-only | Document number display | Parties.View |
| Status | PartyDetailPage | RelatedDocuments DataGrid column | read-only | Status badge | Parties.View |
| Date | PartyDetailPage | RelatedDocuments DataGrid column | read-only | Date format (RTL) | Parties.View |
| Amount | PartyDetailPage | RelatedDocuments DataGrid column | read-only | Money format `decimal(23,2)` | Parties.View |

---

## 2. DESIGN SECTION

### Design System Reference

All design values from `src/Web/ClientApp/src/design-system/tokens.ts` (SSOT). No invented colors, spacing, typography.

**RTL**: All pages use `dir="rtl"` on root container. Arabic-first: NameAr is primary display, NameEn secondary.

**Dark Mode**: All components must work in both `light` and `dark` themes using semantic tokens.

### PartiesListPage Layout

```
┌─────────────────────────────────────────────┐
│ PageHeader: "قائمة الأطراف" + "طرف جديد"   │  typography: headline-sm, spacing: 4
├─────────────────────────────────────────────┤
│ FilterBar                                   │  spacing: 3, gap: 2
│  [PartyType ▾] [IsActive ▾] [Search 🔍]    │  components: Select, Input
├─────────────────────────────────────────────┤
│ DataGrid                                    │  border: container, radius: lg
│  PartyCode | NameAr | PartyType | TaxNumber │  columns: 4
│  | IsActive(badge) | Actions                │  typography: body-sm
│  ...rows (skeleton loading)                 │  statusColors for badge
├─────────────────────────────────────────────┤
│ Pagination                                  │  spacing: 4
└─────────────────────────────────────────────┘
```

**Tokens used**: `semanticColors.surface`, `semanticColors.onSurface`, `semanticColors.containerBorder`, `statusColors.active`, `statusColors.closed`, `spacing[3]`, `spacing[4]`, `borderRadius.lg`, `fontSize.sm`, `fontFamily.sans`, `shadows.sm`

**Existing components reused**: `DataGrid` (from budgeting list pages), `Select` (shadcn), `Input` (shadcn), `Badge` (shadcn), `Pagination` (from budgeting), `Button` (shadcn), `Skeleton` (shadcn)

**New components**: None — all from shadcn or existing patterns.

### PartyDetailPage Layout

```
┌─────────────────────────────────────────────┐
│ PageHeader: PartyCode + PartyType badge     │  typography: headline-sm
├──────────────────────┬──────────────────────┤
│ Profile Section      │ Actions              │  layout: grid 2-col
│  NameAr (primary)    │ [Edit] [Toggle]      │  typography: headline-sm for NameAr
│  NameEn              │                      │  body-sm for others
│  TaxNumber           │                      │  spacing: 4
│  NationalId          │                      │
│  Phone / Email       │                      │
│  Address / Notes     │                      │
├──────────────────────┴──────────────────────┤
│ CollapsibleSection: "سجل الموافقات"          │  Accordion from shadcn
│  ApprovalsPanel(documentType, documentId)   │  components: ApprovalsPanel
├─────────────────────────────────────────────┤
│ CollapsibleSection: "سجل الحالات"            │
│  StatusLogPanel(documentType, documentId)   │  components: StatusLogPanel
├─────────────────────────────────────────────┤
│ CollapsibleSection: "المرفقات"               │
│  AttachmentsPanel(documentType, documentId) │  components: AttachmentsPanel
├─────────────────────────────────────────────┤
│ CollapsibleSection: "المستندات ذات الصلة"    │
│  RelatedDocuments DataGrid                  │  columns: 5
│  [Filter by type ▾]                         │  Pagination
└─────────────────────────────────────────────┘
```

**Tokens used**: `semanticColors.surface`, `semanticColors.onSurface`, `semanticColors.surfaceContainer`, `statusColors` for badges, `spacing[4]`, `spacing[6]`, `borderRadius.lg`, `fontSize.base`, `fontFamily.sans`, `shadows.sm`

**Existing components reused**: `Accordion` (shadcn, collapsible sections), `Badge` (shadcn), `Button` (shadcn), `Skeleton` (shadcn), `DataGrid` (existing), `Dialog` (shadcn, for edit mode and confirmation)

**New components**: None.

### ApprovalsPanel Layout

```
┌─────────────────────────────────────────────┐
│ Timeline (vertical, RTL-ordered)            │
│  ┌─ Card ─────────────────────────────────┐ │
│  │ ApproverName          DecisionAt (rel) │ │  typography: label-md for name, body-sm for time
│  │ Badge(Decision)                        │ │  statusColors for badge
│  │ Reason text or "—"                     │ │  body-sm
│  └────────────────────────────────────────┘ │  spacing: 3 between cards
│  ...more cards                             │
│ Empty: "لا توجد موافقات بعد"               │  typography: body-md, muted color
└─────────────────────────────────────────────┘
```

### StatusLogPanel Layout

```
┌─────────────────────────────────────────────┐
│ Table                                       │  typography: body-sm
│  FromStatus → ToStatus | ChangedBy | Time   │  columns: 4
│  ...rows                                    │  border: divider
│ Empty: "لم تُسجَّل تغييرات حالة"           │
└─────────────────────────────────────────────┘
```

### AttachmentsPanel Layout

```
┌─────────────────────────────────────────────┐
│ GateBadge (if showGate=true)                │  Badge: amber/green
│ [رفع مرفق ▾] (Upload Attachment)            │  Button + Dropdown
├─────────────────────────────────────────────┤
│ AttachmentList                              │
│  FileName (clickable) | Type | Size | Date  │  typography: body-sm
│  [Delete] per row                           │  spacing: 2
│ Empty: "لا توجد مرفقات"                     │
└─────────────────────────────────────────────┘
```

---

## 3. STATE MATRIX

Per page: all applicable UI states. Implementation MUST cover every cell marked "YES".

| Page | Loading | Empty | Error | Unauthorized | Not Found | Pending | Conflict | Validation Summary |
|------|---------|-------|-------|--------------|-----------|---------|----------|-------------------|
| PartiesListPage | YES — Skeleton rows (5 rows × 6 cols) | YES — "لا توجد أطراف" + illustration | YES — Error banner + retry button | YES — Redirect to /unauthorized | — | — | — | YES — Inline per-field errors on create/edit modal |
| PartyDetailPage | YES — Skeleton profile + skeleton panels | — | YES — Error banner + retry button | YES — Redirect to /unauthorized | YES — "الطرف غير موجود" page | — | YES — Conflict dialog: "تم تعديل الطرف من مستخدم آخر. هل تريد إعادة التحميل؟" | YES — Inline per-field errors on edit form |
| ApprovalsPanel | YES — Skeleton timeline (3 cards) | YES — "لا توجد موافقات بعد" | YES — Inline error + retry | — | — | YES — Pending badge on undecided steps | — | — |
| StatusLogPanel | YES — Skeleton table (3 rows) | YES — "لم تُسجَّل تغييرات حالة" | YES — Inline error + retry | — | — | — | — | — |
| AttachmentsPanel | YES — Skeleton list (3 items) | YES — "لا توجد مرفقات" | YES — Inline error + retry | — | — | — | — | YES — File size > 10MB: "الحد الأقصى للحجم 10 ميجابايت"; invalid type: inline error |
| RelatedDocuments | YES — Skeleton table (3 rows) | YES — "لا توجد مستندات ذات صلة" | YES — Inline error + retry | — | — | — | — | — |

---

## 4. TEST MAP

One Vitest test per spec "Tests Expected" item. Behavior-level assertions only.

| Spec Test ID | Test File | Test Name | Assertion |
|-------------|-----------|-----------|-----------|
| TEST-01 | PartiesListPage.test.tsx | renders party list with expected columns | `getAllByRole('columnheader')` contains PartyCode, NameAr, PartyType, TaxNumber, IsActive |
| TEST-02 | PartiesListPage.test.tsx | filters by PartyType | Select "Supplier" → `getAllByRole('row')` contains only Supplier parties |
| TEST-03 | PartiesListPage.test.tsx | searches by NameAr | Type "أحمد" in search → rows filtered to matching parties |
| TEST-04 | PartiesListPage.test.tsx | searches by TaxNumber | Type "12345" in search → rows filtered to matching parties |
| TEST-05 | PartiesListPage.test.tsx | hides New Party button without permission | Render with user lacking `Parties.Create` → `queryByText('طرف جديد')` is null |
| TEST-06 | PartiesListPage.test.tsx | hides toggle active without permission | Render with user lacking `Parties.Update` → toggle buttons not present |
| TEST-07 | PartyDetailPage.test.tsx | renders party profile with all 11 fields | `getAllByText` for PartyCode, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes, PartyType, IsActive |
| TEST-08 | PartyDetailPage.test.tsx | edit form pre-fills all values | Click Edit → form fields contain current party values |
| TEST-09 | PartyDetailPage.test.tsx | hides Edit button without permission | Render with user lacking `Parties.Update` → `queryByText('تعديل')` is null |
| TEST-10 | PartyDetailPage.test.tsx | deactivation guard blocks on open documents | Toggle active → dialog lists open documents → confirm required |
| TEST-11 | PartyDetailPage.test.tsx | renders related documents list | `getAllByRole('columnheader')` in related section contains DocumentType, DocumentNumber, Status, Date, Amount |
| TEST-12 | PartyDetailPage.test.tsx | shows empty state for related documents | Party with no documents → `getByText('لا توجد مستندات ذات صلة')` |
| TEST-13 | PartyDetailPage.test.tsx | filters related documents by type | Select "ReceiptVoucher" → only voucher rows visible |
| TEST-14 | DuplicateTaxWarning.test.tsx | shows warning on duplicate TaxNumber | Enter existing TaxNumber → blur → ConfirmDialog appears |
| TEST-15 | DuplicateTaxWarning.test.tsx | user can confirm to proceed | Confirm in dialog → form submits |
| TEST-16 | DuplicateTaxWarning.test.tsx | warning clears when TaxNumber changes | Change TaxNumber → dialog disappears |
| TEST-17 | ApprovalsPanel.test.tsx | renders approval timeline with fields | ApproverName, DecisionAt, Decision, Reason all present in DOM |
| TEST-18 | ApprovalsPanel.test.tsx | shows empty state | Empty approvals → `getByText('لا توجد موافقات بعد')` |
| TEST-19 | ApprovalsPanel.test.tsx | shows Pending badge for undecided steps | Undecided step → badge with "قيد الانتظار" text |
| TEST-20 | StatusLogPanel.test.tsx | renders status log table | FromStatus, ToStatus, ChangedBy, ChangedAt, Reason columns present |
| TEST-21 | StatusLogPanel.test.tsx | shows empty state | Empty log → `getByText('لم تُسجَّل تغييرات حالة')` |
| TEST-22 | AttachmentsPanel.test.tsx | renders attachment list | FileName, UploadedBy, CreatedAt, Size present per item |
| TEST-23 | AttachmentsPanel.test.tsx | upload dropdown shows requirements + Other | Open upload → dropdown has requirement types + "أخرى" option |
| TEST-24 | AttachmentsPanel.test.tsx | delete shows confirmation | Click delete → ConfirmDialog appears |
| TEST-25 | AttachmentsPanel.test.tsx | gate badge shows missing required | Gate check returns ["INVOICE"] → amber badge "مرفق مطلوب مفقود: INVOICE" |
| TEST-26 | AttachmentsPanel.test.tsx | gate badge shows satisfied | Gate check returns [] → green badge |
| TEST-27 | AttachmentsPanel.test.tsx | oversized upload rejected | Select 15MB file → error "الحد الأقصى للحجم 10 ميجابايت" |

---

## 5. COMPLETENESS GATE

Converge will verify EVERY field name below appears in the implemented source code under `src/Web/ClientApp/src/features/parties/` or `src/Web/ClientApp/src/features/documents/`.

### Party Fields (must appear in parties feature)

```
PartyCode, PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes, IsActive
```

### ApprovalHistory Fields (must appear in documents feature)

```
ApproverName, DecisionAt, Decision, Reason
```

### DocumentStatusLog Fields (must appear in documents feature)

```
FromStatus, ToStatus, ChangedBy, ChangedAt, Reason
```

### Attachment Fields (must appear in documents feature)

```
FileName, AttachmentTypeCode, UploadedBy, CreatedAt, SizeBytes, Gate
```

### PartyDocumentResponse Fields (must appear in parties feature)

```
DocumentType, DocumentNumber, Status, Date, Amount
```

### Permission Codes (must appear in parties feature)

```
Parties.View, Parties.Create, Parties.Update
```

### Verification Command

```bash
# After implementation, every field must be greppable:
rg -l "PartyCode|PartyType|NameAr|NameEn|TaxNumber|NationalId|Phone|Email|Address|Notes|IsActive" src/Web/ClientApp/src/features/parties/
rg -l "ApproverName|DecisionAt|Decision|FromStatus|ToStatus|ChangedBy|ChangedAt|FileName|AttachmentTypeCode|UploadedBy|SizeBytes|Gate" src/Web/ClientApp/src/features/documents/
rg -l "Parties\.View|Parties\.Create|Parties\.Update" src/Web/ClientApp/src/features/parties/
```

**Any field missing from grep output = CONVERGE FAIL. Gap listed explicitly.**

---

## Complexity Tracking

> No constitution violations — no constitution exists. No complexity justifications needed.
