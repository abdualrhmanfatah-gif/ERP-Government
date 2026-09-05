# Implementation Plan: إدارة مجموعات الحسابات (Account Groups)

**Branch**: `005-account-groups` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-account-groups/spec.md` (clarified 2026-09-02, 5/5 questions resolved)

## Summary

تمكين إدارة هرمية لمجموعات الحسابات (Asset/Liability/Equity/Revenue/Expense) كبيانات أساسية لدليل الحسابات مع ضمان سلامة الشجرة (Restrict FK ذاتي، Level 1-5 محتسب، منع حلقات، فرادة كود دائمة، وراثة نوع إلزامية)، تزامن متفائل RowVersion، تعطيل/تفعيل آمن بفحص عميق لكل الأحفاد والحسابات، وتدقيق INSERT-ONLY. الواجهتان خلفية وأمامية: توسيع أوامر `CreateAccountGroup`/`UpdateAccountGroup` الموجودة، إضافة `ToggleAccountGroupActive` و `GetAccountGroupsList` ببحث/فلترة/ترقيم وضعان (شجرة على الجذور بدون بحث، مسطحة مع breadcrumb مع بحث)، و `GetAccountGroupDetail` (تفاصيل + فروع مباشرة + حسابات + AuditTrail)، مع واجهة عربية RTL بتوكنز ووضع مظلم ومكونات مشتركة.

## Technical Context

**Language/Version**: C# 12 / .NET SDK 10.0.201 (rollForward latestFeature, `global.json`), TypeScript ~5.9, React 19.1 — repo-verified via `global.json` + `package.json`.

**Primary Dependencies**: Backend: MediatR 14.1, FluentValidation 12.1, AutoMapper 16.1, EF Core 10.0.5, ASP.NET Core Identity/EF, Aspire.Hosting.SqlServer 13.2/SqlServer EF. Frontend: `react-router-dom` 7.6, `@tanstack/react-query` 5.102, `@tanstack/react-table` 9.2, `react-hook-form` 7.86 + `zod` 3.25 + `@hookform/resolvers`, `shadcn`/`@base-ui/react` 1.7, `tailwindcss` 3.4, `next-themes`, `lucide-react`, `sonner`. Build: Vite 8 + `nswag` OpenAPI generation (`nswag.json`).

**Storage**: SQL Server (Aspire SqlServer) via EF Core Code First. Migrations only (`src/Infrastructure/Migrations/*`), no auto-DDL outside throwaway hosts. `AccountGroups` جدول موجود (`AccountGroupConfiguration`: Code 20 unique, Name 200, Description 500, Level tinyint, RowVersion IsRowVersion, ParentId FK Restrict, HasIndex ParentId). `Accounts` FK AccountGroupId Restrict. AuditTrail/SecurityAuditLog INSERT-ONLY عبر triggers (`AuditTrails_Immutable.sql`).

**Testing**: Backend: NUnit 4.5.1 + Shouldly 4.3 + Moq 4.20 + Respawn 7.0 + coverlet + `Microsoft.AspNetCore.Mvc.Testing` + Playwright 1.58 + Reqnroll 3.3. Functional tests ضد DB حقيقي مع reset per-test (مبدأ XI). Frontend: Vitest 4.1 + Testing Library + jsdom + ESLint.

**Target Platform**: Web app: ASP.NET Core 10 (`src/Web`) + Vite SPA (`src/Web/ClientApp`), يستضيف عبر Aspire (`src/AppHost`, `ServiceDefaults`). Linux container-ready. health endpoints + telemetry already wired.

**Project Type**: Web application (backend + frontend) — `Option 2` sole choice.

**Performance Goals**: SC-010: قائمة هرمية <2s p95 لـ 1000 مجموعة (شجرة جذور) و <2s لمسطح بحث؛ إنشاء <60s end-to-end (SC-002)؛ رفض كود مكرر <2s (SC-003) حتى تحت تزامن؛ كشف حلقة/تزامن 100% (SC-004/005)؛ تعطيل محجوب 100% (SC-006)؛ كتابة Audit <1s (SC-007). Backend: ترقيم صفحات 20 افتراضي، حجم JSON Audit 16KB max مع truncation.

**Constraints**: 5 مستويات max (FR-009) — رفض server-side؛ توافق نوع/رصيد (Asset/Expense→Debit, Liability/Equity/Revenue→Credit) + وراثة نوع إلزامي (ابن=أب) — server-side؛ فحص عميق عند التعطيل (كل الأحفاد + كل حسابات الشجرة)؛ فرادة كود دائمة (نشطة+معطلة)؛ IsActive افتراضي true؛ Restrict FK ذاتي؛ RowVersion كل تعديل/تعطيل؛ فشل مغلق للصلاحيات (VII) + تسجيل قرار؛ Audit INSERT-ONLY؛ RTL logical props (start/end) + dark mode + design tokens فقط (X) + مكونات مشتركة؛ رسائل عربية + problem-details (400/401/403/404/409).

**Scale/Scope**: ~1k–10k مجموعة، شجرة 5 عمق، 5 أنواع، حسابات دليل مرتبطة (Accounts). نطاق الميزة: 5 قصص، 25 FR، 10 SC، 3 كيانات رئيسية (AccountGroup, Account, AuditTrail). لا حذف فيزيائي، لا استيراد جماعي في V1.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence / Action |
|-----------|--------|-------------------|
| **I. Layered Integrity (NON-NEGOTIABLE)** | PASS | Use cases in `Application.Accounting` only; depend on `IApplicationDbContext`, `IUser`, `IIdentityService` abstractions. Web endpoints delegate. Domain `AccountGroup` no project refs. Will verify no Application→Infrastructure/Web ref. |
| **II. Bounded Contexts & Events** | PASS | Accounting BC owns `AccountGroup`/`Account`. No cross-module writes. AccountGroups not event-sourced; deactivation check reads Accounts via shared `IApplicationDbContext` (allowed cross-module read). No outbox needed. |
| **III. Server-Side Business-Rule Integrity** | PASS | All rules server-side: fradā, depth 5, normal-balance compat, type inheritance, cycle detect, deep-deactivate guard, posted-entries guard — each returns `Result.Failure` (400/409) not exception. Frontend validation mirrors only. |
| **IV. Financial Integrity (NON-NEGOTIABLE)** | PASS | AccountGroups master-data only; no journal posting. No balancing needed. No violation. |
| **V. Budget Control** | PASS | Not applicable (master data). No spending chain. |
| **VI. Data Integrity** | PASS | Migrations only; FK `ParentId→AccountGroups.Id` Restrict (existing config PASS); RowVersion IsRowVersion (existing PASS); no hard delete (FR-015); Code unique permanent via `HasIndex(Code).IsUnique()` (existing + will enforce case-insensitive check in handler + DB collation, idempotent seed). |
| **VII. Authorization & SoD (NON-NEGOTIABLE)** | PASS | Every endpoint + use case declares `[Authorize]`: Read=`Accounting.ChartOfAccounts.Read`, Create=`Accounting.ChartOfAccounts.Create`, Edit=`Accounting.ChartOfAccounts.Edit` (includes update + toggle). Fail-closed + SecurityAuditLog. Frontend `usePermission` UX only. Re-uses existing `PermissionCodes` constants. |
| **VIII. Approval & Audit Immutability** | PASS | No approval workflow. Audit via existing `AuditableEntityInterceptor` + `AuditTrailChangeTracker` → `AuditTrails` INSERT-ONLY trigger. Deactivate/activate also audited via same interceptor. Will confirm trigger `AuditTrails_Immutable.sql`. |
| **IX. API & Frontend Contract** | PASS | OpenAPI single source (`nswag run`). Endpoints carry stable operation ids + `Produces` + problem-details. RowVersion round-trip. No breaking rename. Frontend uses generated `web-api-client.ts` or contract-conformant hooks. |
| **X. UI & Design System** | PASS | RTL logical props, dark mode, design tokens (`src/Web/ClientApp/src/design-system/tokens.*` + `generate-tokens.ts`), shared ui lib (`src/Web/ClientApp/src/components/ui/*` + `DataGrid`, `PageHeader`, `ConfirmDialog`, `ConflictDialog`). All screens tested RTL+dark. |
| **XI. Testing & Evidence** | PASS | Plan includes unit tests per use case (success + failure), functional tests vs real DB (cycle, depth, concurrency 409, deep-deactivate, posted-guard, audit immutability), browser acceptance for list/search/deactivate journey. No always-pass placeholder. |
| **XII. Controlled Change** | PASS | No layer/module boundary change, no new module. Uses existing `Accounting` module. No ADR required. |

**Gate Verdict**: PASS — proceed to Phase 0. No violations requiring justification. Re-check after Phase 1.

## Project Structure

### Documentation (this feature)

```text
specs/005-account-groups/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── account-groups-api.yaml   # OpenAPI fragment (endpoints)
│   └── types.ts          # DTO contracts excerpt
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root) — actual layout

```text
src/
├── Domain/
│   └── Accounting/Entities/AccountGroup.cs   # BaseAuditableEntity + RowVersion
│                Enums/AccountGroupType.cs    # Asset/Liability/Equity/Revenue/Expense
│                Enums/NormalBalanceType.cs   # Debit/Credit
├── Application/
│   └── Accounting/
│       ├── Common/AccountingDtos.cs         # AccountGroupDto existing (expand)
│       ├── Commands/AccountGroups/CreateAccountGroup/  # enhance: ParentId, Level, validation
│       ├── Commands/AccountGroups/UpdateAccountGroup/  # enhance: ParentId, reparent, level recalc
│       ├── Commands/AccountGroups/ToggleAccountGroupActive/ # new
│       └── Queries/AccountGroups/
│           ├── GetAccountGroupsList/  # enhance: search, type, isActive, pagination dual-mode
│           ├── GetAccountGroupById/   # existing
│           └── GetAccountGroupDetail/ # new: children + accounts + audit
├── Infrastructure/
│   ├── Data/Configurations/Accounting/AccountGroupConfiguration.cs  # Code unique, Restrict
│   ├── Data/Interceptors/ AuditableEntityInterceptor + AuditTrailChangeTracker
│   └── Migrations/  # optional: add collation/index tweak if needed (case-insensitive) or no-op
├── Web/
│   ├── Endpoints/Accounting/AccountGroups.cs  # GET/GET{id}/POST/PUT + PATCH deactivate
│   └── ClientApp/src/
│       ├── features/accounting/account-groups/
│       │   ├── pages/  AccountGroupsListPage, AccountGroupDetailPage, AccountGroupForm
│       │   ├── components/ GroupTree, GroupFormFields, ConflictDialog, StatusBadge
│       │   ├── hooks/  useAccountGroupsList, useAccountGroupDetail, useCreateGroup, useUpdateGroup, useToggleActive
│       │   └── types.ts
│       ├── shared/constants/permissions.ts  # Accounting.ChartOfAccounts.*
│       ├── components/ui/  DataGrid, PageHeader, ConfirmDialog, AuditTimeline, etc.
│       └── design-system/  tokens.ts / tokens.css.scss (no hard-coded values)
tests/
├── Application.Tests/Accounting/AccountGroups/*  # unit handlers + validators
├── FunctionalTests/Accounting/AccountGroupsScenarios.cs # real DB, Respawn
└── Web.ClientApp.Tests/  # Vitest for pages/hooks
```

**Structure Decision**: Web application — `src/Domain`→`Application`→`Infrastructure`→`Web` + `src/Web/ClientApp` Vite SPA. No new projects. Feature lives entirely inside existing `Accounting` module boundaries (Principle I/II). Frontend under `src/Web/ClientApp/src/features/accounting/account-groups/`.

## Complexity Tracking

> No Constitution violations to justify. Table intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |
