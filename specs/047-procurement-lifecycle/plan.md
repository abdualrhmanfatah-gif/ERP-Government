# Implementation Plan: Procurement Lifecycle Rebuild

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-12 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/047-procurement-lifecycle/spec.md`

## Summary

Complete rebuild of the procurement module covering the full lifecycle: Purchase Request → RFQ → Quotation/Evaluation → Purchase Order → Budget Encumbrance → Goods Receipt → Supplier Invoice (three-way match) → Payment & Closure. The rebuild unifies supplier references via `SupplierPartyId`, removes legacy fields, adds missing entity relationships, introduces typed status enums, enforces approval/audit via `ApprovalHistory` and `DocumentStatusLog`, and builds all 7 document types with commands, queries, endpoints, and Arabic RTL frontend screens. Advance payments and services are out of scope.

**GRN Frontend Scope (FR-110–FR-126)**: Three pages — list with server-side pagination/filters, create (requires `?purchaseOrderId=`, no standalone PO selection), and detail with confirm/reject dialogs. Permission codes aligned to `GoodsReceipts.*` (not `GoodsReceiptNotes.*`). Current user resolved via existing `useUserDetail` pattern. Backend gaps documented (no PO eligibility endpoint, no user-name in GRN response).

**Supplier Invoice Frontend Scope (FR-127–FR-153)**: Three pages — list with server-side pagination/filters (search, status, purchaseOrderId), create (PO-linked via `?purchaseOrderId=`, no standalone), and detail with status-gated lifecycle actions (Submit, Match, Cancel). Backend gaps documented: match is status-transition-only (no three-way matching), AcceptWithNotes is notes-only (no status change), SupplierName returns null in both list and detail, endpoint group lacks authorization on routes, AcceptWithNotes endpoint not wired. Frontend bugs documented: circular self-import in types, response type mismatch in list hook, missing pagination/error/loading states, missing create/detail pages, missing permission codes. Clarifications resolved: AcceptWithNotes is notes-only, PO-linked creation mandatory, Unit Price pre-filled from PO and editable.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire

**Storage**: SQL Server via EF Core (existing DbContext, migrations pattern)

**Testing**: xUnit + FluentAssertions; Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests

**Target Platform**: Web application (backend API + React SPA frontend)

**Project Type**: Web application with layered architecture (Domain → Application → Infrastructure → Web)

**Performance Goals**: Standard web application response times (<500ms p95 for CRUD operations); no special performance constraints identified

**Constraints**: Arabic-only RTL UI; government procurement domain; must integrate with existing Budget (spec 046), Payment (spec 045), and Committee modules

**Scale/Scope**: 7 new document types × 2 entities each = ~14 new/modified entities; 7 user stories; ~109 functional requirements (FR-001–FR-153); 11 frontend screens (GRN: 3 pages; Supplier Invoice: 3 pages; PO: 4 pages; PR: list); GRN frontend (3 pages: list, create, detail) with permission alignment and PO-linked creation flow; Supplier Invoice frontend (3 pages: list, create, detail) with permission alignment, PO-linked creation, status-gated actions, and 6 documented backend gaps

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after GRN frontend scope addition (2026-09-12).*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | New code follows Domain→Application→Infrastructure→Web layers. No cross-layer references. |
| II. Bounded Contexts | ✅ PASS | Procurement is an existing module. Cross-module reads via shared persistence abstraction. Ledger postings via domain events. |
| III. Server-Side Business Rules | ✅ PASS | All validation/enforcement in handlers. Status transitions guarded. No business logic in endpoints. |
| IV. Financial Integrity | ✅ PASS | Encumbrance amounts computed from PO lines. Journal entries balance. Corrections via reversal only. |
| V. Budget Control | ✅ PASS | Encumbrance created at PO approval. Availability checked before encumbrance. Liquidation at receipt. |
| VI. Data Integrity | ✅ PASS | New migrations only. Restrict on all FKs. Optimistic concurrency via RowVersion. No hard deletes. |
| VII. Authorization & SoD | ✅ PASS (with note) | All endpoints declare permissions. SoD between PR requestor/approver not enforced (deliberate policy, documented in spec FR-014a). |
| VIII. Approval & Audit | ✅ PASS | All transitions recorded in ApprovalHistory + DocumentStatusLog. Inline approval fields removed. |
| IX. API Contract | ✅ PASS | OpenAPI-first. NSwag client generation. Problem details for errors. |
| X. UI Consistency | ✅ PASS | Arabic RTL. Design tokens. Shared component library. Logical CSS properties. |
| XI. Testing | ✅ PASS | TDD mandatory. Functional tests against real DB. No test 045 exception applies. |
| XII. Controlled Change | ✅ PASS | Module already registered. No new modules added. |

**Gate Result**: PASS — no violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/047-procurement-lifecycle/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── procurement-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   └── Procurement/
│       ├── Entities/        # PurchaseRequest, PurchaseRequestDetail, RequestForQuotation, RFQSupplier, Quotation, QuotationDetail, PurchaseOrder, PurchaseOrderDetail, GoodsReceiptNote, GoodsReceiptNoteDetail, SupplierInvoice, SupplierInvoiceDetail
│       └── Enums/           # PurchaseRequestStatus, RFQStatus, RFQSupplierStatus, QuotationStatus, PurchaseOrderStatus, PurchaseOrderDetailStatus, GRNStatus, SupplierInvoiceStatus
├── Application/
│   └── Procurement/
│       ├── Commands/
│       │   ├── PurchaseRequests/   # Create, Update, Submit, Approve, Reject, Cancel
│       │   ├── RequestForQuotations/ # Create, Publish, Close, Cancel
│       │   ├── Quotations/          # Create, Submit, StartEvaluation, CompleteEvaluation, Select, Award, Reject
│       │   ├── PurchaseOrders/      # Create, Submit, Approve, Issue, Cancel, Close
│       │   ├── GoodsReceiptNotes/   # Create, Confirm, Reject
│       │   └── SupplierInvoices/    # Create, Submit, Match, AcceptWithNotes, Cancel
│       ├── Queries/
│       │   ├── PurchaseRequests/    # Get, GetById, GetList
│       │   ├── RequestForQuotations/
│       │   ├── Quotations/
│       │   ├── PurchaseOrders/
│       │   ├── GoodsReceiptNotes/
│       │   └── SupplierInvoices/
│       └── Common/                  # Shared validators, mapping profiles
├── Infrastructure/
│   └── Data/
│       ├── Configurations/Procurement/  # EF entity configurations
│       └── Migrations/                  # New migration(s)
├── Web/
│   └── Endpoints/
│       └── Procurement/             # PurchaseRequests.cs, RequestForQuotations.cs, Quotations.cs, PurchaseOrders.cs, GoodsReceiptNotes.cs, SupplierInvoices.cs, ProcurementDashboard.cs
└── Web/ClientApp/
    └── src/
        └── features/
            └── procurement/         # purchase-requests/, request-for-quotations/, quotations/, purchase-orders/, goods-receipt-notes/, supplier-invoices/, dashboard/
```

**Structure Decision**: Standard layered architecture following existing repository conventions. Each document type gets its own command/query/endpoint group. Frontend follows entity-based feature folder pattern per AGENTS.md.

## Complexity Tracking

No constitution violations. No complexity tracking needed.
