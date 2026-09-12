# Implementation Plan: RFQ & Quotation Management

**Branch**: `050-rfq-quotation-ui` | **Date**: 2026-09-11 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/050-rfq-quotation-ui/spec.md`

## Summary

Complete the RFQ and Quotation management feature within the existing Procurement module. Domain entities and most application commands/queries already exist. Work focuses on: (1) fixing endpoint wiring gaps in Quotations.cs, (2) adding missing backend commands (UpdateQuotation, RejectQuotation route, supplier response route), (3) adding FluentValidation validators, (4) building complete frontend pages for RFQ (detail, edit) and Quotation (list, detail, create, edit), (5) wiring all frontend routes and navigation.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire

**Storage**: SQL Server via EF Core (existing DbContext, no schema changes needed)

**Testing**: xUnit + FluentAssertions; Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests

**Target Platform**: Web application (backend API + React SPA frontend)

**Project Type**: Web application with layered architecture (Domain → Application → Infrastructure → Web)

**Performance Goals**: Standard web application response times (<500ms p95 for CRUD operations)

**Constraints**: Arabic-only RTL UI; government procurement domain; must integrate with existing Procurement module entities and commands; no schema changes; no mock data

**Scale/Scope**: 4 existing entities (RequestForQuotation, RFQSupplier, Quotation, QuotationDetail); 2 new commands (UpdateQuotation, UpdateRFQ); 2 new validators; 7 frontend pages; 8 routes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | New code follows Domain→Application→Infrastructure→Web. No cross-layer references. |
| II. Bounded Contexts | ✅ PASS | Procurement is an existing module. Cross-module reads via shared persistence abstraction. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | Validators enforce rules server-side. Status transitions guarded in handlers. |
| IV. Financial Integrity | ✅ PASS | No journal entries in this feature. Monetary fields are display-only on quotations. |
| V. Budget Control | ✅ PASS | Budget linkage handled by PO spec 047. No encumbrance in this feature. |
| VI. Data Integrity | ✅ PASS | No schema changes. Restrict FKs on existing entities. Optimistic concurrency via RowVersion. |
| VII. Authorization & SoD | ✅ PASS | All endpoints declare PermissionCodes. No SoD changes. |
| VIII. Approval & Audit | ✅ PASS | Status transitions recorded in existing ApprovalHistory + DocumentStatusLog. |
| IX. API Contract | ✅ PASS | OpenAPI-first. NSwag client generation. Result<T> for all responses. |
| X. UI Consistency | ✅ PASS | Arabic RTL. Design tokens. Shared shadcn components. Logical CSS properties. |
| XI. Testing | ✅ PASS | TDD mandatory. Tests before implementation. No 045 exception applies. |
| XII. Controlled Change | ✅ PASS | No new modules. Existing module extended only. |

**Gate Result**: PASS — no violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/050-rfq-quotation-ui/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Procurement/
│   └── Entities/          # RequestForQuotation, RFQSupplier, Quotation, QuotationDetail (EXIST)
├── Application/Procurement/
│   ├── Commands/
│   │   ├── RequestForQuotations/
│   │   │   ├── CreateRFQ/          (EXIST)
│   │   │   ├── PublishRFQ/         (EXIST)
│   │   │   ├── RecordRFQResponse/  (EXIST)
│   │   │   ├── CloseRFQCollection/ (EXIST)
│   │   │   ├── CancelRFQ/          (EXIST)
│   │   │   └── UpdateRFQ/          (NEW)
│   │   └── Quotations/
│   │       ├── CreateQuotation/    (EXIST)
│   │       ├── SubmitQuotation/    (EXIST)
│   │       ├── StartEvaluation/    (EXIST)
│   │       ├── CompleteEvaluation/ (EXIST)
│   │       ├── SelectQuotation/    (EXIST)
│   │       ├── RejectQuotation/    (EXIST)
│   │       ├── AwardQuotation/     (EXIST)
│   │       └── UpdateQuotation/    (NEW)
│   └── Queries/
│       ├── RequestForQuotations/   (EXIST - GetRFQs, GetRFQById)
│       └── Quotations/             (EXIST - GetQuotations, GetQuotationById)
├── Web/Endpoints/Procurement/
│   ├── RequestForQuotations.cs     (EXIST - needs UpdateRFQ + RecordResponse routes)
│   └── Quotations.cs               (EXIST - needs GET /, GET /{id} fix, PUT /{id}, POST /reject)
└── Web/ClientApp/src/
    ├── features/procurement/
    │   ├── request-for-quotations/
    │   │   ├── pages/              (EXIST: RFQsListPage; NEW: RFQDetailPage, RFQCreatePage, RFQEditPage)
    │   │   ├── hooks/              (EXIST: useRFQs)
    │   │   └── shared/             (EXIST: types; NEW: schemas.ts)
    │   └── quotations/
    │       ├── pages/              (NEW: QuotationsListPage, QuotationDetailPage, QuotationCreatePage, QuotationEditPage)
    │       ├── hooks/              (NEW: useQuotations)
    │       └── shared/             (NEW: types.ts, schemas.ts, client.ts)
    └── components/
        └── ProcurementRFQ*         (NEW - feature-scoped components if needed)
```

## Complexity Tracking

No constitution violations. No complexity tracking needed.
