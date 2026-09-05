# Research: Financial Reports Module

**Feature**: 007-financial-reports | **Date**: 2026-09-03

## R1: Report Engine Architecture

**Decision**: Single `IReportEngine` interface in Application layer with per-report query handlers (MediatR `IRequestHandler`) and format providers in Infrastructure.

**Rationale**: Follows existing CQRS pattern (GetTrialBalanceQuery as precedent). Application layer owns the query + DTO shapes; Infrastructure implements Excel/PDF export. Keeps layers clean per Principle I.

**Alternatives considered**:
- *Single monolithic ReportService*: Rejected — violates single-responsibility, hard to test individual reports.
- *T4/Source-generated reports*: Rejected — too rigid for 4 distinct report structures with different section groupings.

## R2: Permission Model for Reports

**Decision**: 6 distinct permissions under `accounting.reports.*` namespace in `PermissionCodes.cs`:
- `accounting.reports.balance-sheet` → ViewBalanceSheet
- `accounting.reports.income-statement` → ViewIncomeStatement
- `accounting.reports.general-ledger` → ViewGeneralLedger
- `accounting.reports.cash-flow` → ViewCashFlow
- `accounting.reports.export` → ExportReports
- `accounting.reports.print` → PrintReports

**Rationale**: Matches existing `Accounting.SubModule.Action` pattern (e.g., `Accounting.Balances.Read`). Separate export/print permissions gate non-view actions per Principle VII. Existing `REPORTS_READ`/`REPORTS_EXPORT` seed data in SecurityPermissionSeedData will be replaced/augmented with the 6 granular codes.

**Alternatives considered**:
- *Single `accounting.reports.read` permission*: Rejected — spec requires per-report gating for role separation.
- *Reuse existing `REPORTS_READ`/`REPORTS_EXPORT`*: Rejected — too coarse; doesn't support per-report access control.

## R3: Excel Export Technology

**Decision**: ClosedXML (NuGet package) for OpenXML spreadsheet generation.

**Rationale**: Mature, well-documented library for .NET. Supports RTL, styling, merged cells, auto-width columns. No dependency on Excel being installed. Already referenced in `Directory.Packages.props` investigation (not present — will be added).

**Alternatives considered**:
- *EPPlus*: Similar capability but licensing concerns (commercial license for v5+). ClosedXML is MIT.
- *NPOI*: Java port, less idiomatic .NET API.
- *ExcelJS (frontend)*: Rejected — spec requires server-side generation for determinism (FR-012).

## R4: PDF Export Technology

**Decision**: QuestPDF for fluent PDF generation with RTL/Arabic support.

**Rationale**: Fluent API (no XML templates). Built-in RTL support with Arabic font fallback. Landscape/portrait control. Page numbering. MIT license. Active maintenance.

**Alternatives considered**:
- *iTextSharp/iText7*: Powerful but AGPL license (commercial license required for closed-source).
- *PdfSharp*: Less feature-rich, no built-in RTL.
- *wkhtmltopdf*: External process dependency, harder to control RTL/fonts.

## R5: Balance Sheet Equity Calculation with Net Income

**Decision**: Balance Sheet handler queries Income Statement handler internally (same use case, direct method call) to compute current-period Net Income for the Equity section.

**Rationale**: Net Income = Revenue - Expenses for the current fiscal period. Balance Sheet needs this to complete the accounting equation. Direct call within the handler keeps it deterministic (same database context, same query). No duplication.

**Alternatives considered**:
- *Separate API call to Income Statement*: Rejected — introduces unnecessary coupling and potential inconsistency.
- *Materialized NetIncome field on AccountBalance*: Rejected — violates Principle IV (derived balances must be rebuildable from posted data).

## R6: Cash Flow Statement Opening Cash Computation

**Decision**: Query GL cash accounts (Account.Type=Cash OR Account.IsReconcilable=true) for balance as of startDate minus one day.

**Rationale**: Clarified in spec session 2026-09-03. Live query avoids dependency on materialization timing. Consistent with Principle IV (posted entries are source of truth).

**Alternatives considered**:
- *Prior period materialized balance*: Rejected — may be stale if materialization hasn't run.

## R7: General Ledger Server-Side Pagination

**Decision**: Page/pageSize query parameters on the API. ReportResult includes TotalLines. Default page size 500. Frontend uses TanStack Table pagination.

**Rationale**: Clarified in spec session 2026-09-03. Keeps API responses bounded. Prevents browser freeze on 50k+ rows. Aligns with existing pagination patterns (Pagination component exists in UI library).

**Alternatives considered**:
- *Cursor-based pagination*: More complex, unnecessary for append-only GL data.
- *No pagination / hard cap*: Doesn't meet UX requirements for full GL review.

## R8: Frontend Report Data Fetching

**Decision**: Custom `useReport` hook using TanStack Query with ETag-based caching via `If-None-Match` header.

**Rationale**: Spec requires ETag support (FR-010). TanStack Query supports custom fetchers with headers. ETag avoids re-downloading identical report data. Follows existing hook patterns (useAccountsList, useMoves).

**Alternatives considered**:
- *NSwag-generated client*: Works for JSON endpoints but doesn't handle ETag or file downloads cleanly.
- *SWR*: Already using TanStack Query; adding SWR would be redundant.

## R9: Format Provider Pattern

**Decision**: `IReportExporter` interface in Application with `ExportAsync(ReportResult, ReportFormat, Stream)` method. Implementations: `ExcelReportExporter` (Infrastructure) and `PdfReportExporter` (Infrastructure).

**Rationale**: Application defines the contract; Infrastructure implements. Determinism guarantee: all formats receive the same ReportResult object. Single source of truth (FR-012).

**Alternatives considered**:
- *Per-report format classes*: Rejected — too many classes for 4 reports × 2 formats.
- *Template-based approach*: Rejected — less flexible for complex layouts.

## R10: Audit Trail for Reports

**Decision**: Log to existing `SecurityAuditLog` entity (INSERT-ONLY) via a `ReportAuditService` that wraps report execution.

**Rationale**: Principle VII requires every authorization decision recorded. Principle VIII requires INSERT-ONLY audit trails. Existing SecurityAuditLog entity already has the right shape. Wrapper pattern keeps audit concern separate from business logic.

**Alternatives considered**:
- *Separate ReportAuditLog table*: Rejected — redundant with existing SecurityAuditLog.
- *Middleware-based logging*: Rejected — too coarse; can't capture per-report params and success/failure.
